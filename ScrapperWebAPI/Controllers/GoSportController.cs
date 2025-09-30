using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using ScrapperWebAPI.Models.GoSport;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
namespace ScrapperWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoSportController : ControllerBase
{
    private readonly HttpClient _httpClient = new HttpClient();

    [HttpGet("brands")]
    public async Task<ActionResult<Dictionary<string, string>>> GetBrands()
    {
        var allBrands = new Dictionary<string, string>();

        for (int page = 1; page <= 6; page++)
        {
            string url = $"https://www.gosport.az/brands?page={page}";
            var html = await _httpClient.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var brandNodes = doc.DocumentNode.SelectNodes("//div[@class='col-md-3 text-center mb-5']/a");

            if (brandNodes != null)
            {
                foreach (var node in brandNodes)
                {
                    string href = node.GetAttributeValue("href", "");
                    string name = node.SelectSingleNode(".//h4")?.InnerText.Trim();

                    if (!string.IsNullOrEmpty(href) && !string.IsNullOrEmpty(name))
                    {
                        if (!allBrands.ContainsKey(name))
                            allBrands.Add(name, href);
                    }
                }
            }
        }

        return Ok(allBrands);
    }

    [HttpGet("products")]
    public async Task<ActionResult<List<GoSportProduct>>> GetProducts([FromQuery] string href)
    {
        if (string.IsNullOrEmpty(href))
            return BadRequest("Href is required");

        var allData = new ConcurrentBag<GoSportProduct>();
        int page = 1;
        int? lastPageNumber = null;

        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        using var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(15) // Timeout azaldıldı
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        while (true)
        {
            var url = $"{href}?page={page}";
            string html;

            try
            {
                html = await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch page {page}: {ex.Message}");
                break;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (lastPageNumber == null)
            {
                lastPageNumber = GetLastPageNumber(doc);
                Console.WriteLine($"Total pages found: {lastPageNumber}");
            }

            var productDivs = doc.DocumentNode.SelectNodes("//div[contains(@class, 'product-card-1')]");

            if (productDivs == null || productDivs.Count == 0)
            {
                Console.WriteLine($"No products found on page {page}");
                break;
            }

            // PARALEL İŞLƏMƏ - Hər səhifədəki məhsulları paralel yükləyirik
            var tasks = productDivs.Select(async productDiv =>
            {
                var product = ExtractProductFromDiv(productDiv, href);
                if (product != null)
                {
                    // Her detail sorgusu üçün yeni HttpClient yaratmaq əvəzinə mövcud client-i istifadə edirik
                    await EnrichProductWithDetails(product, client);
                    allData.Add(product);
                }
            });

            // Bütün məhsulları paralel yükləyirik, maksimum 10 eyni vaxtda
            var semaphore = new SemaphoreSlim(10); // Eyni vaxtda maksimum 10 sorğu
            var limitedTasks = tasks.Select(async task =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await task;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(limitedTasks);

            Console.WriteLine($"Page {page}: Found {productDivs.Count} products");


            if (lastPageNumber.HasValue && page >= lastPageNumber.Value)
                break;

            page++;
        }

        Console.WriteLine($"Total products scraped: {allData.Count}");
        return Ok(allData.ToList());
    }

    // OPTIMIZƏ EDİLMİŞ METOD
    private async Task EnrichProductWithDetails(GoSportProduct product, HttpClient client)
    {
        if (string.IsNullOrEmpty(product.ProductUrl))
            return;

        try
        {
            // Retry mexanizmi əlavə edildi
            var detailHtml = await GetWithRetry(client, product.ProductUrl, maxRetries: 2);
            if (detailHtml == null) return;

            var detailDoc = new HtmlDocument();
            detailDoc.LoadHtml(detailHtml);

            ExtractDetailPrice(detailDoc, product);
            ExtractImages(detailDoc, product);
            ExtractDescription(detailDoc, product);
            ExtractCategories(detailDoc, product);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enriching product {product.Id}: {ex.Message}");
        }
    }

    // RETRY MEXANİZMİ
    private async Task<string> GetWithRetry(HttpClient client, string url, int maxRetries = 2)
    {
        for (int i = 0; i <= maxRetries; i++)
        {
            try
            {
                return await client.GetStringAsync(url);
            }
            catch (Exception ex) when (i < maxRetries)
            {
                Console.WriteLine($"Retry {i + 1} for {url}: {ex.Message}");
                await Task.Delay(100 * (i + 1)); // Gecikdirmə artırılır
            }
        }
        return null;
    }

    private void ExtractDetailPrice(HtmlDocument doc, GoSportProduct product)
    {
        var priceDiv = doc.DocumentNode.SelectSingleNode("//div[@class='product-price fs-3 fw-500 mb-2']");
        if (priceDiv == null) return;

        // Helper method to clean price text
        string CleanPrice(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            // Rəqəmlərdən başqa hər şeyi silirik (AZN, boşluq, s.)
            var text = Regex.Replace(input, @"[^\d.,]", "");

            // Gizli boşluqları da təmizlə
            text = text.Replace("\u00A0", "")   // non-breaking space
                       .Replace("\u200B", "")   // zero-width space
                       .Trim();

            return text;
        }

        // Discounted price (indirimli)
        var discountedSpan = priceDiv.SelectNodes(".//span[contains(@class,'text-primary')]")
                             ?.LastOrDefault(s => !s.GetClasses().Contains("visually-hidden"));


        if (discountedSpan != null)
        {
            var priceText = CleanPrice(discountedSpan.InnerText);
            if (decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var discountedPrice))
                product.DiscountedPrice = discountedPrice;
        }

        // Original price (əsas qiymət)
        var originalDel = priceDiv.SelectNodes(".//del[@class='fs-6 text-muted']")
                                  ?.LastOrDefault(d => !d.GetClasses().Contains("visually-hidden"));

        if (originalDel != null)
        {
            var priceText = CleanPrice(originalDel.InnerText);
            if (decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var originalPrice))
                product.Price = originalPrice;
        }

        // Əgər endirim yoxdursa → əsas qiymətə kopyala
        if (product.Price == 0 && product.DiscountedPrice.HasValue)
        {
            product.Price = product.DiscountedPrice.Value;
            product.DiscountedPrice = null;
        }

        // Endirim var ya yox
        product.HasDiscount = product.DiscountedPrice.HasValue && product.Price > product.DiscountedPrice;
    }

    private void ExtractImages(HtmlDocument doc, GoSportProduct product)
    {
        var images = doc.DocumentNode.SelectNodes("//div[contains(@class,'product-gallery')]//img");

        if (images != null && images.Count > 0)
        {
            foreach (var img in images)
            {
                var src = img.GetAttributeValue("src", "");
                if (!string.IsNullOrEmpty(src))
                {
                    if (!src.StartsWith("http"))
                        src = "https://www.gosport.az" + src;

                    product.AdditionalImages.Add(src);
                }
            }

            if (string.IsNullOrEmpty(product.ImageUrl) && product.AdditionalImages.Count > 0)
                product.ImageUrl = product.AdditionalImages[0];
        }
    }

    private void ExtractDescription(HtmlDocument doc, GoSportProduct product)
    {
        var descriptionDiv = doc.DocumentNode.SelectSingleNode("//div[@id='pd_description']//div[@class='product-description']//p");
        if (descriptionDiv != null)
        {
            product.ShortDescription = System.Net.WebUtility.HtmlDecode(descriptionDiv.InnerText.Trim());
        }
    }

    private void ExtractCategories(HtmlDocument doc, GoSportProduct product)
    {
        var categoriesSection = doc.DocumentNode.SelectSingleNode("//p[contains(.//label, 'Kateqoriyalar:')]");
        if (categoriesSection != null)
        {
            var categoryLinks = categoriesSection.SelectNodes(".//a");
            if (categoryLinks != null)
            {
                var categories = new List<string>();
                foreach (var link in categoryLinks)
                {
                    var categoryName = link.InnerText.Trim();
                    if (!string.IsNullOrEmpty(categoryName))
                        categories.Add(categoryName);
                }
                product.Categories = categories;
            }
        }
    }

    private int GetLastPageNumber(HtmlDocument doc)
    {
        try
        {
            var paginationDiv = doc.DocumentNode
                .SelectSingleNode("//div[@class='col-md-12 text-center']//nav//ul[@class='pagination']");

            if (paginationDiv == null) return 1;

            var lastPageNumber = 1;

            var pageLinks = paginationDiv.SelectNodes(".//li[@class='page-item']/a[@class='page-link']");
            if (pageLinks != null)
            {
                foreach (var link in pageLinks)
                {
                    var href = link.GetAttributeValue("href", "");
                    var pageMatch = Regex.Match(href, @"page=(\d+)");

                    if (pageMatch.Success && int.TryParse(pageMatch.Groups[1].Value, out var pageNum))
                    {
                        if (pageNum > lastPageNumber)
                            lastPageNumber = pageNum;
                    }
                }
            }

            var activePage = paginationDiv.SelectSingleNode(".//li[@class='page-item active']//span[@class='page-link']");
            if (activePage != null && int.TryParse(activePage.InnerText.Trim(), out var activePageNum))
            {
                if (activePageNum > lastPageNumber)
                    lastPageNumber = activePageNum;
            }

            return lastPageNumber;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting last page number: {ex.Message}");
            return 1;
        }
    }

    private GoSportProduct ExtractProductFromDiv(HtmlNode productDiv, string _baseUrl)
    {
        try
        {
            var product = new GoSportProduct();

            var addToCartBtn = productDiv.SelectSingleNode(".//a[contains(@onclick, 'trackAddToCart')]");
            if (addToCartBtn != null)
            {
                var onClickValue = addToCartBtn.GetAttributeValue("onclick", "");
                var idMatch = Regex.Match(onClickValue, @"""id"":(\d+)");
                if (idMatch.Success)
                    product.Id = int.Parse(idMatch.Groups[1].Value);

                var modelMatch = Regex.Match(onClickValue, @"""model"":""([^""]+)""");
                if (modelMatch.Success)
                    product.Model = modelMatch.Groups[1].Value;

                var descMatch = Regex.Match(onClickValue, @"""short_description"":""([^""]+)""");
                if (descMatch.Success)
                    product.ShortDescription = System.Net.WebUtility.HtmlDecode(descMatch.Groups[1].Value);
            }

            var titleElement = productDiv.SelectSingleNode(".//h6[@class='product-title']//a");
            if (titleElement != null)
            {
                product.Name = System.Net.WebUtility.HtmlDecode(titleElement.InnerText.Trim());
                var href = titleElement.GetAttributeValue("href", "");
                if (!string.IsNullOrEmpty(href))
                    product.ProductUrl = href.StartsWith("http") ? href : "https://www.gosport.az" + href;
            }

            var brandElement = productDiv.SelectSingleNode(".//div[@class='product-meta small']//a");
            if (brandElement != null)
                product.Brand = brandElement.InnerText.Trim();

            var imageElement = productDiv.SelectSingleNode(".//div[@class='product-media rounded-0 p-1']//img");
            if (imageElement != null)
            {
                var imgSrc = imageElement.GetAttributeValue("src", "");
                if (!string.IsNullOrEmpty(imgSrc))
                {
                    if (imgSrc.StartsWith("/"))
                        product.ImageUrl = "https://www.gosport.az" + imgSrc;
                    else if (!imgSrc.StartsWith("http"))
                        product.ImageUrl = "https://www.gosport.az/" + imgSrc;
                    else
                        product.ImageUrl = imgSrc;
                }
            }

            var priceDiv = productDiv.SelectSingleNode(".//div[@class='product-price']");
            if (priceDiv != null)
            {
                var primaryPrice = priceDiv.SelectSingleNode(".//span[@class='text-primary' and not(contains(@class, 'visually-hidden'))]");
                if (primaryPrice != null)
                {
                    var priceText = Regex.Replace(primaryPrice.InnerText, @"[^\d.,]", "").Trim();
                    if (decimal.TryParse(priceText, out var discountedPrice))
                        product.DiscountedPrice = discountedPrice;
                }

                var originalPrice = priceDiv.SelectSingleNode(".//del[@class='fs-sm text-muted' and not(contains(@class, 'visually-hidden'))]");
                if (originalPrice != null)
                {
                    var priceText = Regex.Replace(originalPrice.InnerText, @"[^\d.,]", "").Trim();
                    if (decimal.TryParse(priceText, out var price))
                        product.Price = price;
                }

                if (product.Price == 0 && product.DiscountedPrice.HasValue)
                {
                    product.Price = product.DiscountedPrice.Value;
                    product.DiscountedPrice = null;
                }

                product.HasDiscount = product.DiscountedPrice.HasValue && product.Price > product.DiscountedPrice;
            }

            var sizeElements = productDiv.SelectNodes(".//div[@class='nav-thumbs']//div[@class='form-check radio-text form-check-inline cursor-pointer']");
            if (sizeElements != null)
            {
                foreach (var sizeElement in sizeElements)
                {
                    var input = sizeElement.SelectSingleNode(".//input[@type='radio']");
                    var label = sizeElement.SelectSingleNode(".//label");

                    if (input != null && label != null)
                    {
                        var size = new GoSportProductSize
                        {
                            SizeName = label.InnerText.Trim(),
                            IsAvailable = input.GetAttributeValue("disabled", "") != "disabled"
                        };

                        var inputValue = input.GetAttributeValue("value", "");
                        if (!string.IsNullOrEmpty(inputValue))
                        {
                            ExtractSizeDetailsFromJson(inputValue, size);
                        }

                        product.Sizes.Add(size);
                    }
                }
            }

            return product;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting product: {ex.Message}");
            return null;
        }
    }

    private void ExtractSizeDetailsFromJson(string jsonValue, GoSportProductSize size)
    {

    }
}
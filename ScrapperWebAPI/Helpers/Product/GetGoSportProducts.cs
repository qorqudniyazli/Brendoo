using HtmlAgilityPack;
using ScrapperWebAPI.Helpers.Mappers;
using ScrapperWebAPI.Models.GoSport;
using ScrapperWebAPI.Models.ProductDtos;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Text;

namespace ScrapperWebAPI.Helpers.Product;

public static class GetGoSportProducts
{
    public async static Task<List<ProductToListDto>> GetByProductByBrand(string brand)
    {
        string href = await GetBrandLink(brand);
        var allData = new ConcurrentBag<GoSportProduct>();

        int page = 1;
        int? lastPageNumber = null;

        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            MaxConnectionsPerServer = 25
        };

        using var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(12)
        };

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        using var apiClient = new HttpClient()
        {
            Timeout = TimeSpan.FromMinutes(10)
        };

        while (true)
        {
            var url = $"{href}?page={page}";
            string html = null;

            bool pageSuccess = false;
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    html = await client.GetStringAsync(url);
                    pageSuccess = true;
                    break;
                }
                catch (Exception ex)
                {
                    if (attempt < 3)
                    {
                        await Task.Delay(2000 * attempt);
                    }
                }
            }

            if (!pageSuccess)
            {
                page++;
                continue;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (lastPageNumber == null)
            {
                lastPageNumber = GetLastPageNumber(doc);
            }

            var productDivs = doc.DocumentNode.SelectNodes("//div[contains(@class, 'product-card-1')]");

            if (productDivs == null || productDivs.Count == 0)
            {
                if (lastPageNumber.HasValue && page >= lastPageNumber.Value)
                    break;

                page++;
                continue;
            }

            foreach (var productDiv in productDivs)
            {
                try
                {
                    if (productDiv == null)
                    {
                        Console.WriteLine("GOSPORT: Null product div atildi");
                        continue;
                    }

                    var product = ExtractProductFromDiv(productDiv, href);
                    if (product == null)
                    {
                        Console.WriteLine("GOSPORT: Mehsul extract edilmedi");
                        continue;
                    }

                    if (string.IsNullOrEmpty(product.ProductUrl))
                    {
                        Console.WriteLine("GOSPORT: ProductUrl bos - atildi");
                        continue;
                    }

                    await EnrichProductWithDetails(product, client);
                    allData.Add(product);

                    var mappedProducts = await GoSportMapper.MapAsync(new List<GoSportProduct> { product });

                    if (mappedProducts != null && mappedProducts.Count > 0)
                    {
                        await SendSingleProductToExternalAPI(mappedProducts[0], brand, page, apiClient);
                    }
                    else
                    {
                        Console.WriteLine("GOSPORT: Map xetasi - mehsul atildi");
                    }

                    await Task.Delay(300);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("GOSPORT MEHSUL XETA: " + ex.Message);
                    continue;
                }
            }

            if (lastPageNumber.HasValue && page >= lastPageNumber.Value)
                break;

            page++;
            await Task.Delay(1000);
        }

        var allMappedData = await GoSportMapper.MapAsync(allData.ToList());
        return allMappedData;
    }

    private static async Task SendSingleProductToExternalAPI(ProductToListDto product, string brand, int pageNumber, HttpClient apiClient)
    {
        try
        {
            var sizes = new List<object>();
            if (product.Sizes != null)
            {
                foreach (var size in product.Sizes)
                {
                    sizes.Add(new { sizeName = size.SizeName, onStock = size.OnStock });
                }
            }

            var colors = new List<object>();
            if (product.Colors != null)
            {
                foreach (var color in product.Colors)
                {
                    colors.Add(new { name = color.Name, hex = color.Hex });
                }
            }

            var productData = new
            {
                name = product.Name ?? "",
                brand = product.Brand ?? "",
                price = product.Price,
                productUrl = product.ProductUrl,
                discountedPrice = product.DiscountedPrice,
                description = !string.IsNullOrEmpty(product.Description) && product.Description.Length > 150
                    ? product.Description.Substring(0, 150) + "..."
                    : product.Description ?? "",
                images = product.ImageUrl ?? new List<string>(),
                sizes = sizes,
                colors = colors,
                store = "gosport",
                category = brand,
                processedAt = DateTime.Now.ToString("HH:mm:ss")
            };

            var productList = new List<object> { productData };

            const int maxRetries = 3;
            bool success = false;

            for (int retry = 0; retry < maxRetries && !success; retry++)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(productData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await apiClient.PostAsync(
                        "http://192.168.10.148:5000/api/stock/add",
                        content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("GOSPORT: " + brand + " - " + product.Name + " gonderildi");
                        success = true;
                    }
                    else
                    {
                        if (retry < maxRetries - 1)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(5 * (retry + 1)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (retry < maxRetries - 1)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5 * (retry + 1)));
                    }
                }
            }

            if (!success)
            {
                Console.WriteLine("GOSPORT: " + brand + " - " + product.Name + " ATILDI");
            }

            await Task.Delay(500);
        }
        catch (Exception ex)
        {
            Console.WriteLine("GOSPORT SendSingleProduct xetasi: " + ex.Message);
        }
    }

    public async static Task<string> GetBrandLink(string brand)
    {
        using var httpClient = new HttpClient();

        var searchTasks = Enumerable.Range(1, 6).Select(async page =>
        {
            try
            {
                string url = $"https://www.gosport.az/en/brands?page={page}";
                var html = await httpClient.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var brandNodes = doc.DocumentNode.SelectNodes("//div[@class='col-md-3 text-center mb-5']/a");
                if (brandNodes != null)
                {
                    foreach (var node in brandNodes)
                    {
                        string href = node.GetAttributeValue("href", "");
                        string name = node.SelectSingleNode(".//h4")?.InnerText.Trim();

                        if (string.Equals(name, brand, StringComparison.OrdinalIgnoreCase))
                        {
                            return href;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return "";
        });

        var results = await Task.WhenAll(searchTasks);
        return results.FirstOrDefault(r => !string.IsNullOrEmpty(r)) ?? "";
    }

    private static async Task EnrichProductWithDetails(GoSportProduct product, HttpClient client)
    {
        if (string.IsNullOrEmpty(product.ProductUrl))
            return;

        try
        {
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
        }
    }

    private static async Task<string> GetWithRetry(HttpClient client, string url, int maxRetries = 2)
    {
        for (int i = 0; i <= maxRetries; i++)
        {
            try
            {
                return await client.GetStringAsync(url);
            }
            catch (Exception ex) when (i < maxRetries)
            {
                await Task.Delay(150 * (i + 1));
            }
        }
        return null;
    }

    private static void ExtractDetailPrice(HtmlDocument doc, GoSportProduct product)
    {
        var priceDiv = doc.DocumentNode.SelectSingleNode("//div[@class='product-price fs-3 fw-500 mb-2']");
        if (priceDiv == null) return;

        string CleanPrice(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var text = Regex.Replace(input, @"[^\d.,]", "");
            text = text.Replace("\u00A0", "").Replace("\u200B", "").Trim();
            return text;
        }

        var discountedSpan = priceDiv.SelectNodes(".//span[contains(@class,'text-primary')]")
                             ?.LastOrDefault(s => !s.GetClasses().Contains("visually-hidden"));

        if (discountedSpan != null)
        {
            var priceText = CleanPrice(discountedSpan.InnerText);
            if (decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var discountedPrice))
                product.DiscountedPrice = discountedPrice;
        }

        var originalDel = priceDiv.SelectNodes(".//del[@class='fs-6 text-muted']")
                                  ?.LastOrDefault(d => !d.GetClasses().Contains("visually-hidden"));

        if (originalDel != null)
        {
            var priceText = CleanPrice(originalDel.InnerText);
            if (decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var originalPrice))
                product.Price = originalPrice;
        }

        if (product.Price == 0 && product.DiscountedPrice.HasValue)
        {
            product.Price = product.DiscountedPrice.Value;
            product.DiscountedPrice = null;
        }

        product.HasDiscount = product.DiscountedPrice.HasValue && product.Price > product.DiscountedPrice;
    }

    private static void ExtractImages(HtmlDocument doc, GoSportProduct product)
    {
        var images = doc.DocumentNode.SelectNodes("//div[contains(@class,'product-gallery')]//img");

        if (images != null && images.Count > 0)
        {
            product.AdditionalImages.Clear();

            foreach (var img in images)
            {
                var src = img.GetAttributeValue("src", "");
                if (!string.IsNullOrEmpty(src))
                {
                    if (!src.StartsWith("http"))
                        src = "https://www.gosport.az" + src;

                    if (!product.AdditionalImages.Contains(src))
                    {
                        product.AdditionalImages.Add(src);
                    }
                }
            }

            if (string.IsNullOrEmpty(product.ImageUrl) && product.AdditionalImages.Count > 0)
                product.ImageUrl = product.AdditionalImages[0];
        }
    }

    private static void ExtractDescription(HtmlDocument doc, GoSportProduct product)
    {
        var descriptionDiv = doc.DocumentNode.SelectSingleNode("//div[@id='pd_description']//div[@class='product-description']//p");
        if (descriptionDiv != null)
        {
            product.ShortDescription = System.Net.WebUtility.HtmlDecode(descriptionDiv.InnerText.Trim());
        }
    }

    private static void ExtractCategories(HtmlDocument doc, GoSportProduct product)
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

    private static int GetLastPageNumber(HtmlDocument doc)
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
            return 1;
        }
    }

    private static GoSportProduct ExtractProductFromDiv(HtmlNode productDiv, string _baseUrl)
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

                        product.Sizes.Add(size);
                    }
                }
            }

            return product;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}
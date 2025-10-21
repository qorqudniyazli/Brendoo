using HtmlAgilityPack;
using ScrapperWebAPI.Models.ProductDtos;
using Newtonsoft.Json;
using System.Text;

namespace ScrapperWebAPI.Helpers.Product;

public class ProductDto
{
    public string Title { get; set; }
    public string Url { get; set; }
    public List<string> Images { get; set; } = new();
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int? DiscountPercent { get; set; }
    public List<string> Sizes { get; set; } = new();
}

public static class GetOliviaProducts
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    static GetOliviaProducts()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
    }

    public static async Task<int> GetProductCount(string categoryUrl)
    {
        var response = await _httpClient.GetAsync(categoryUrl);
        if (!response.IsSuccessStatusCode) return 0;

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var countNode = doc.DocumentNode.SelectSingleNode("//span[@data-productcount]");
        if (countNode != null)
        {
            var count = countNode.GetAttributeValue("data-productcount", "0");
            return int.TryParse(count, out var result) ? result : 0;
        }

        return 0;
    }

    public static async Task<List<string>> GetProductUrls(string categoryUrl)
    {
        var totalCount = await GetProductCount(categoryUrl);
        var pageCount = (int)Math.Ceiling(totalCount / 20.0);

        var productUrls = new List<string>();

        for (int page = 1; page <= pageCount; page++)
        {
            var pageUrl = $"{categoryUrl}?p={page}";
            var response = await _httpClient.GetAsync(pageUrl);

            if (!response.IsSuccessStatusCode) continue;

            var html = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var products = doc.DocumentNode.SelectNodes("//div[@class='prodItem has-discount' or @class='prodItem']");

            if (products != null)
            {
                foreach (var product in products)
                {
                    var url = product.GetAttributeValue("data-product-url", "");
                    if (!string.IsNullOrEmpty(url))
                    {
                        productUrls.Add(url);
                    }
                }
            }

            await Task.Delay(200);
        }

        return productUrls;
    }

    public static async Task<ProductDto> GetProductDetails(string productUrl)
    {
        var response = await _httpClient.GetAsync(productUrl);
        if (!response.IsSuccessStatusCode) return null;

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var product = new ProductDto { Url = productUrl };

        // Title
        var titleNode = doc.DocumentNode.SelectSingleNode("//span[@class='productInformation__title']");
        product.Title = titleNode?.InnerText.Trim() ?? "";

        // Images
        var imageNodes = doc.DocumentNode.SelectNodes("//div[@class='slider111__thumbs thumbnails']//img");
        if (imageNodes != null)
        {
            foreach (var img in imageNodes)
            {
                var src = img.GetAttributeValue("src", "");
                if (!string.IsNullOrEmpty(src) && !product.Images.Contains(src))
                {
                    product.Images.Add(src);
                }
            }
        }

        if (product.Images.Count == 0)
        {
            var mainImage = doc.DocumentNode.SelectSingleNode("//picture[@class='product-image']//img");
            if (mainImage != null)
            {
                var src = mainImage.GetAttributeValue("src", "");
                if (!string.IsNullOrEmpty(src))
                {
                    product.Images.Add(src);
                }
            }
        }

        // Discounted Price
        var discountedPriceNode = doc.DocumentNode.SelectSingleNode("//div[@class='prodCart__prices']//strong[@data-price-amount]");
        if (discountedPriceNode != null)
        {
            var priceText = discountedPriceNode.GetAttributeValue("data-price-amount", "");
            if (decimal.TryParse(priceText, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var discPrice))
            {
                product.DiscountedPrice = discPrice;
            }
        }

        // Original Price
        var originalPriceNode = doc.DocumentNode.SelectSingleNode("//span[@data-price-amount and @data-price-type='finalPrice']");
        if (originalPriceNode != null)
        {
            var priceText = originalPriceNode.GetAttributeValue("data-price-amount", "");
            if (decimal.TryParse(priceText, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var origPrice))
            {
                product.Price = origPrice;
            }
        }

        if (!product.DiscountedPrice.HasValue && product.Price > 0)
        {
            product.DiscountedPrice = null;
        }
        else if (product.DiscountedPrice.HasValue && product.Price == 0)
        {
            product.Price = product.DiscountedPrice.Value;
            product.DiscountedPrice = null;
        }

        // Discount percent
        var discountNode = doc.DocumentNode.SelectSingleNode("//div[@class='discount']/span");
        if (discountNode != null)
        {
            var discountText = discountNode.InnerText.Replace("-", "").Replace("%", "").Trim();
            if (int.TryParse(discountText, out var discount))
            {
                product.DiscountPercent = discount;
            }
        }

        // Sizes
        var volumeNodes = doc.DocumentNode.SelectNodes("//div[@class='har__title' and contains(text(), 'Объем/вес')]/following-sibling::div[@class='har__znach ']");
        if (volumeNodes != null)
        {
            foreach (var volumeNode in volumeNodes)
            {
                var size = volumeNode.GetAttributeValue("data-text", "").Trim();
                if (string.IsNullOrEmpty(size))
                {
                    size = volumeNode.InnerText.Trim();
                }
                if (!string.IsNullOrEmpty(size) && !product.Sizes.Contains(size))
                {
                    product.Sizes.Add(size);
                }
            }
        }

        return product;
    }

    public static async Task<List<ProductToListDto>> GetAllProductsFromCategory(string categoryName)
    {
        var categoryUrl = await GetOliviaCategories.GetCategoryUrl(categoryName);

        if (string.IsNullOrEmpty(categoryUrl))
            return new List<ProductToListDto>();

        var productUrls = await GetProductUrls(categoryUrl);
        var products = new List<ProductDto>();

        // API client-i yaradırıq
        using var apiClient = new HttpClient()
        {
            Timeout = TimeSpan.FromMinutes(10)
        };

        int processedCount = 0;

        foreach (var url in productUrls)
        {
            try
            {
                var product = await GetProductDetails(url);
                if (product != null)
                {
                    products.Add(product);

                    // Map edirik və API-ya göndəririk
                    var mappedProduct = MapToDto(product);
                    if (mappedProduct != null)
                    {
                        await SendSingleProductToExternalAPI(mappedProduct, categoryName, processedCount, apiClient);
                        processedCount++;
                    }
                }
                await Task.Delay(300);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OLIVIA MEHSUL XETA: {ex.Message}");
            }
        }

        return MapToDtoList(products);
    }

    private static async Task SendSingleProductToExternalAPI(ProductToListDto product, string category, int productNumber, HttpClient apiClient)
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
                sizes = sizes.Take(1),
                colors = colors,
                store = "olivia",
                category = category,
                processedAt = DateTime.Now.ToString("HH:mm:ss")
            };

            const int maxRetries = 3;
            bool success = false;

            for (int retry = 0; retry < maxRetries && !success; retry++)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(productData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await apiClient.PostAsync(
                        "http://69.62.114.202/api/stock/add",
                        content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"OLIVIA: {category} - {product.Name} gonderildi");
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
                Console.WriteLine($"OLIVIA: {category} - {product.Name} ATILDI");
            }

            await Task.Delay(500);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OLIVIA SendSingleProduct xetasi: {ex.Message}");
        }
    }

    public static ProductToListDto MapToDto(ProductDto oliviaProduct)
    {
        if (oliviaProduct == null)
            return null;

        var validSizes = FilterValidSizes(oliviaProduct.Sizes);

        return new ProductToListDto
        {
            Name = oliviaProduct.Title,
            Brand = ExtractBrand(oliviaProduct.Title),
            Description = "",
            Price = oliviaProduct.Price,
            DiscountedPrice = oliviaProduct.DiscountedPrice,
            ProductUrl = oliviaProduct.Url,
            Colors = new List<Color>(),
            Sizes = validSizes.Select(s => new Sizes
            {
                SizeName = s,
                OnStock = true
            }).ToList(),
            ImageUrl = oliviaProduct.Images
        };
    }

    public static List<ProductToListDto> MapToDtoList(List<ProductDto> oliviaProducts)
    {
        if (oliviaProducts == null || !oliviaProducts.Any())
            return new List<ProductToListDto>();

        return oliviaProducts
            .Select(MapToDto)
            .Where(p => p != null)
            .ToList();
    }

    private static List<string> FilterValidSizes(List<string> sizes)
    {
        if (sizes == null || !sizes.Any())
            return new List<string>();

        var validSizes = new List<string>();

        foreach (var size in sizes)
        {
            var trimmedSize = size.Trim();

            if (string.IsNullOrWhiteSpace(trimmedSize) ||
                trimmedSize.Equals("Нет информации", StringComparison.OrdinalIgnoreCase) ||
                trimmedSize.Equals("Россия", StringComparison.OrdinalIgnoreCase) ||
                trimmedSize.Equals("Турция", StringComparison.OrdinalIgnoreCase) ||
                trimmedSize.Length > 50)
            {
                continue;
            }

            if (IsValidSizeFormat(trimmedSize))
            {
                validSizes.Add(trimmedSize);
            }
        }

        return validSizes;
    }

    private static bool IsValidSizeFormat(string size)
    {
        var validUnits = new[] { "мл", "л", "гр", "кг", "г", "ml", "l", "g", "kg", "см", "cm", "мм", "mm" };
        return validUnits.Any(unit => size.ToLower().Contains(unit));
    }

    private static string ExtractBrand(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "";

        var parts = title.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 3)
        {
            return $"{parts[2]} {(parts.Length > 3 ? parts[3] : "")}".Trim();
        }

        return "";
    }
}
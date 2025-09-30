using Newtonsoft.Json;
using ScrapperWebAPI.Helpers.Mappers;
using ScrapperWebAPI.Models.ProductDtos;
using ScrapperWebAPI.Models.Zara;
using ScrapperWebAPI.Models.Zara.Product;
using System.Net.Http;
using System.Text;
using ZaraScraperWebApi.Models;

namespace ScrapperWebAPI.Helpers.Product;

public static class GetZaraProduct
{
    private static readonly HttpClient _httpClient;
    private static int _consecutiveFailures = 0;
    private const int MAX_CONSECUTIVE_FAILURES = 10;

    static GetZaraProduct()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
            MaxConnectionsPerServer = 20,
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 3
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    public async static Task<List<ProductToListDto>> GetByCategoryName(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            Console.WriteLine("ZARA: Bos kategori - atildi");
            return new List<ProductToListDto>();
        }

        try
        {
            var categoryId = await GetCategoryLink(category);
            if (categoryId == 0)
            {
                Console.WriteLine("ZARA: " + category + " - CategoryId tapilmadi");
                return new List<ProductToListDto>();
            }

            string link = CreateLink(categoryId);

            using var apiClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(30)
            };

            var productLinks = await GetProductLinks(link);
            if (productLinks == null || productLinks.Count == 0)
            {
                Console.WriteLine("ZARA: " + category + " - Mehsul linki tapilmadi");
                return new List<ProductToListDto>();
            }

            var allProducts = new List<ProductToListDto>();

            Console.WriteLine("ZARA: " + category + " - Cemi " + productLinks.Count + " mehsul linki tapildi");

            int processedCount = 0;
            int successCount = 0;
            int failCount = 0;

            foreach (var seoLink in productLinks)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(seoLink))
                    {
                        failCount++;
                        continue;
                    }

                    if (_consecutiveFailures >= MAX_CONSECUTIVE_FAILURES)
                    {
                        Console.WriteLine("ZARA KRITIK: " + category + " - Coxlu ardıcıl xeta! Proses dayandırılır");
                        break;
                    }

                    var product = await ProcessProductLink(seoLink);

                    if (product != null)
                    {
                        allProducts.Add(product);
                        await SendSingleProductToExternalAPI(product, category, apiClient);

                        successCount++;
                        _consecutiveFailures = 0;
                    }
                    else
                    {
                        failCount++;
                        _consecutiveFailures++;
                    }

                    processedCount++;

                    if (processedCount % 10 == 0)
                    {
                        Console.WriteLine("ZARA: " + category + " - " + processedCount + "/" + productLinks.Count + " islendi (Ugurlu: " + successCount + ", Ugursuz: " + failCount + ")");
                    }

                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ZARA MEHSUL XETA: " + category + " - " + ex.Message);
                    failCount++;
                    _consecutiveFailures++;
                    continue;
                }
            }

            Console.WriteLine("ZARA NETICE: " + category + " - Ugurlu: " + successCount + ", Ugursuz: " + failCount + ", Cemi: " + processedCount);

            return allProducts;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ZARA UMUMI XETA: " + category + " - " + ex.Message);
            return new List<ProductToListDto>();
        }
    }

    private static async Task SendSingleProductToExternalAPI(ProductToListDto product, string category, HttpClient apiClient)
    {
        if (product == null)
        {
            Console.WriteLine("ZARA API: Null mehsul - atildi");
            return;
        }

        try
        {
            var sizes = new List<object>();
            if (product.Sizes != null)
            {
                foreach (var size in product.Sizes)
                {
                    if (size != null)
                    {
                        sizes.Add(new { sizeName = size.SizeName ?? "", onStock = size.OnStock });
                    }
                }
            }

            var colors = new List<object>();
            if (product.Colors != null)
            {
                foreach (var color in product.Colors)
                {
                    if (color != null)
                    {
                        colors.Add(new { name = color.Name ?? "", hex = color.Hex ?? "" });
                    }
                }
            }

            var productData = new
            {
                name = product.Name ?? "Mehsul",
                brand = product.Brand ?? "ZARA",
                price = product.Price,
                productUrl = product.ProductUrl ?? "",
                discountedPrice = product.DiscountedPrice,
                description = !string.IsNullOrEmpty(product.Description) && product.Description.Length > 150
                    ? product.Description.Substring(0, 150) + "..."
                    : product.Description ?? "",
                images = product.ImageUrl ?? new List<string>(),
                sizes = sizes,
                colors = colors,
                store = "zara",
                category = category,
                processedAt = DateTime.Now.ToString("HH:mm:ss")
            };

            //var productList = new List<object> { productData };

            const int maxRetries = 5;
            bool success = false;

            for (int retry = 0; retry < maxRetries && !success; retry++)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(productData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

                    var response = await apiClient.PostAsync(
                        "http://192.168.10.148:5000/api/stock/add",
                        content,
                        cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("ZARA API: " + category + " - " + product.Name + " gonderildi");
                        success = true;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("ZARA API XETA: " + response.StatusCode + " - " + errorContent.Substring(0, Math.Min(100, errorContent.Length)));

                        if (retry < maxRetries - 1)
                        {
                            int delaySeconds = 10 * (retry + 1);
                            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("ZARA API TIMEOUT: " + product.Name + " (Cehd " + (retry + 1) + ")");
                    if (retry < maxRetries - 1)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(20 * (retry + 1)));
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine("ZARA API HTTP XETA: " + ex.Message + " (Cehd " + (retry + 1) + ")");
                    if (retry < maxRetries - 1)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(15 * (retry + 1)));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ZARA API UMUMI XETA: " + ex.Message);
                    if (retry < maxRetries - 1)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10 * (retry + 1)));
                    }
                }
            }

            if (!success)
            {
                Console.WriteLine("ZARA API: " + category + " - " + product.Name + " ATILDI (butun cehdler ugursuz)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ZARA SendSingleProduct KRITIK XETA: " + ex.Message);
        }
    }

    private static async Task<HashSet<string>> GetProductLinks(string link)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                return new HashSet<string>();
            }

            var response = await GetWithRetry(link);
            if (response == null)
            {
                Console.WriteLine("ZARA: Product links gelen cavab null");
                return new HashSet<string>();
            }

            var result = JsonConvert.DeserializeObject<ZaraRootSeo>(response);
            HashSet<string> links = new HashSet<string>();

            if (result?.ProductGroups != null)
            {
                foreach (var productGroup in result.ProductGroups)
                {
                    if (productGroup?.Elements != null)
                    {
                        foreach (var element in productGroup.Elements)
                        {
                            if (element?.CommercialComponents != null)
                            {
                                foreach (var component in element.CommercialComponents)
                                {
                                    if (component?.Seo != null &&
                                        !string.IsNullOrEmpty(component.Seo.Keyword) &&
                                        !string.IsNullOrEmpty(component.Seo.SeoProductId))
                                    {
                                        var seo = $"https://www.zara.com/az/ru/{component.Seo.Keyword}-p{component.Seo.SeoProductId}.html?ajax=true";
                                        links.Add(seo);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return links;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ZARA GetProductLinks XETA: " + ex.Message);
            return new HashSet<string>();
        }
    }

    private static async Task<ProductToListDto> ProcessProductLink(string seoLink)
    {
        if (string.IsNullOrWhiteSpace(seoLink))
            return null;

        try
        {
            var linkJson = await GetWithRetry(seoLink);
            if (linkJson == null)
            {
                return null;
            }

            var productDetail = JsonConvert.DeserializeObject<Root>(linkJson);
            if (productDetail?.product == null)
            {
                return null;
            }

            var dto = RootMapper.MapToDto(productDetail);
            if (dto == null)
            {
                return null;
            }

            return dto;
        }
        catch (JsonException ex)
        {
            Console.WriteLine("ZARA JSON XETA: " + ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ZARA ProcessProduct XETA: " + ex.Message);
            return null;
        }
    }

    private static async Task<string> GetWithRetry(string url, int maxRetries = 4)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        for (int i = 0; i <= maxRetries; i++)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

                var response = await _httpClient.GetAsync(url, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    if (i == maxRetries)
                    {
                        Console.WriteLine("ZARA HTTP XETA: " + response.StatusCode);
                        return null;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                if (i == maxRetries)
                {
                    Console.WriteLine("ZARA TIMEOUT: " + url.Substring(0, Math.Min(60, url.Length)));
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                if (i == maxRetries)
                {
                    Console.WriteLine("ZARA HTTP REQUEST XETA: " + ex.Message);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ZARA GetWithRetry UMUMI XETA: " + ex.Message);
                return null;
            }

            if (i < maxRetries)
            {
                await Task.Delay(1000 * (i + 1));
            }
        }

        return null;
    }

    public async static Task<long> GetCategoryLink(string category)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return 0;
            }

            var parsed = ParseMenu(category);
            if (string.IsNullOrEmpty(parsed.Sub))
            {
                Console.WriteLine("ZARA: Kategori parse xetasi - " + category);
                return 0;
            }

            string url = "https://www.zara.com/az/ru/categories?categoryId=2536906&categorySeoId=640&ajax=true";

            var json = await GetWithRetry(url);
            if (json == null)
            {
                Console.WriteLine("ZARA: Kategoriya linkini yukleye bilmedi");
                return 0;
            }

            ZaraCategoryRoot data = JsonConvert.DeserializeObject<ZaraCategoryRoot>(json);
            if (data?.Categories == null)
            {
                Console.WriteLine("ZARA: Categories deserialize xetasi");
                return 0;
            }

            foreach (var cat in data.Categories)
            {
                if (cat?.Subcategories != null && cat.Subcategories.Count > 0)
                {
                    foreach (var sub in cat.Subcategories)
                    {
                        if (sub != null && string.Equals(sub.Name, parsed.Sub, StringComparison.OrdinalIgnoreCase))
                        {
                            long subcategoryId = sub.Id;

                            if (sub.Subcategories != null && sub.Subcategories.Count > 0)
                            {
                                subcategoryId = sub.Subcategories[0].Id;
                            }

                            return subcategoryId;
                        }
                    }
                }
            }

            Console.WriteLine("ZARA: Kategori ID tapilmadi - " + category);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ZARA GetCategoryLink XETA: " + ex.Message);
            return 0;
        }
    }

    public static (string Main, string Sub) ParseMenu(string input)
    {
        if (string.IsNullOrEmpty(input))
            return (null, null);

        try
        {
            var parts = input.Split('-', 2);

            string main = parts.Length > 0 ? parts[0] : null;
            string sub = parts.Length > 1 ? parts[1] : null;

            return (main, sub);
        }
        catch
        {
            return (null, null);
        }
    }

    private static string CreateLink(long id)
    {
        return $"https://www.zara.com/az/ru/category/{id}/products?ajax=true";
    }

    public static void Dispose()
    {
        try
        {
            _httpClient?.Dispose();
        }
        catch
        {
        }
    }
}
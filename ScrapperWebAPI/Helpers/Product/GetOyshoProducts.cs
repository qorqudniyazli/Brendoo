using Newtonsoft.Json;
using ScrapperWebAPI.Helpers.Mappers;
using ScrapperWebAPI.Models.MassimoModels;
using ScrapperWebAPI.Models.ProductDtos;
using System.Net;
using System.Text;

namespace ScrapperWebAPI.Helpers.Product;

public static class GetOyshoProducts
{
    private static readonly HttpClient _client;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5, 5);
    private static readonly JsonSerializerSettings _jsonSettings;

    static GetOyshoProducts()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            AllowAutoRedirect = true,
            MaxConnectionsPerServer = 10
        };

        _client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _client.DefaultRequestHeaders.Add("Accept", "application/json, text/html, */*");
        _client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7,az;q=0.6");
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        _client.DefaultRequestHeaders.Add("Referer", "https://www.oysho.com/");
        _client.DefaultRequestHeaders.Add("Origin", "https://www.oysho.com");
        _client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        _client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
        _client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
        _client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
        _client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

        _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Error = (sender, args) =>
            {
                var error = args.ErrorContext.Error;
                var path = args.ErrorContext.Path;

                Console.WriteLine($"OYSHO JSON xeta: Path: {path} | Error: {error.Message}");
                args.ErrorContext.Handled = true;
            }
        };
    }

    public static async Task<List<ProductToListDto>> FetchProductsAsync(List<string> urls)
    {
        if (urls == null || urls.Count == 0)
        {
            Console.WriteLine("OYSHO: Bos URL list");
            return new List<ProductToListDto>();
        }

        Console.WriteLine($"OYSHO: Basladi - {urls.Count} URL");

        var allProducts = new List<ProductToListDto>();
        var stats = new ScraperStats();

        await WarmupConnection();

        var tasks = urls.Select(async (url, index) =>
        {
            await _semaphore.WaitAsync();
            try
            {
                var products = await ProcessSingleUrl(url, index + 1, urls.Count);

                lock (allProducts)
                {
                    if (products != null && products.Count > 0)
                    {
                        allProducts.AddRange(products);
                        stats.SuccessCount++;
                    }
                    else
                    {
                        stats.EmptyCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                lock (allProducts)
                {
                    stats.ErrorCount++;
                }
                Console.WriteLine($"OYSHO ERROR: URL {index + 1} - {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        PrintStats(stats, allProducts.Count);

        return allProducts;
    }

    private static async Task<List<ProductToListDto>> ProcessSingleUrl(string url, int current, int total)
    {
        try
        {
            var json = await GetWithRetry(url);

            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine($"OYSHO: {current}/{total} - Bos cavab");
                return null;
            }

            Root data;
            try
            {
                data = JsonConvert.DeserializeObject<Root>(json, _jsonSettings);
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"OYSHO: {current}/{total} - JSON xetasi: {jsonEx.Message.Substring(0, Math.Min(80, jsonEx.Message.Length))}");
                return null;
            }

            if (data?.products == null || data.products.Count == 0)
            {
                Console.WriteLine($"OYSHO: {current}/{total} - Mehsul yoxdur");
                return null;
            }

            var products = MassimoProductMapper.Map(data);

            if (products != null && products.Count > 0)
            {
                Console.WriteLine($"OYSHO: {current}/{total} - {products.Count} mehsul");
                await SendProductsToAPI(products, url);
                return products;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OYSHO: {current}/{total} - {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    private static async Task<string> GetWithRetry(string url, int maxRetries = 3)
    {
        Exception lastException = null;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));

                var response = await _client.GetAsync(url, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(5000 * attempt);
                    continue;
                }

                if ((int)response.StatusCode >= 500)
                {
                    await Task.Delay(2000 * attempt);
                    continue;
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                lastException = new Exception("Timeout");
                if (attempt < maxRetries)
                {
                    await Task.Delay(1000 * attempt);
                }
            }
            catch (HttpRequestException httpEx)
            {
                lastException = httpEx;
                if (attempt < maxRetries)
                {
                    await Task.Delay(1000 * attempt);
                }
            }
        }

        Console.WriteLine($"OYSHO RETRY: {maxRetries} cehd ugursuz - {lastException?.Message}");
        return null;
    }

    private static async Task SendProductsToAPI(List<ProductToListDto> products, string sourceUrl)
    {
        if (products == null || products.Count == 0)
        {
            Console.WriteLine("OYSHO API: Məhsul yoxdur, API çağırılmadı");
            return;
        }

        int totalProducts = products.Count;
        int sentCount = 0;
        int errorCount = 0;

        try
        {
            using var apiClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };

            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];

                try
                {
                    // Məhsul məlumatlarını hazırla
                    List<object> sizes = new List<object>();
                    if (product.Sizes != null)
                    {
                        sizes = product.Sizes.Select(s => (object)new
                        {
                            sizeName = s.SizeName ?? "",
                            onStock = s.OnStock
                        }).ToList();
                    }

                    List<object> colors = new List<object>();
                    if (product.Colors != null)
                    {
                        colors = product.Colors.Select(c => (object)new
                        {
                            name = c.Name ?? "",
                            hex = c.Hex ?? ""
                        }).ToList();
                    }

                    var productData = new
                    {
                        name = product.Name ?? "Məhsul",
                        brand = product.Brand ?? "Oysho",
                        price = product.Price / 100,
                        productUrl = product.ProductUrl ?? "",
                        discountedPrice = product.DiscountedPrice,
                        description = TruncateDescription(product.Description),
                        images = product.ImageUrl ?? new List<string>(),
                        sizes = sizes,
                        colors = colors,
                        store = "oysho",
                        category = ExtractCategoryFromUrl(sourceUrl),
                        processedAt = DateTime.Now.ToString("HH:mm:ss")
                    };

                    // Retry mexanizmi
                    bool success = false;
                    for (int attempt = 1; attempt <= 3 && !success; attempt++)
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
                                sentCount++;
                                success = true;

                                if ((i + 1) % 10 == 0)
                                {
                                    Console.WriteLine($"OYSHO API: [{i + 1}/{totalProducts}] göndərildi (Uğurlu={sentCount}, Xəta={errorCount})");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"OYSHO API xəta: {product.Name} - HTTP {response.StatusCode} (Cəhd {attempt}/3)");

                                if (attempt < 3)
                                {
                                    await Task.Delay(2000 * attempt);
                                }
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            Console.WriteLine($"OYSHO API timeout: {product.Name} (Cəhd {attempt}/3)");

                            if (attempt < 3)
                            {
                                await Task.Delay(3000 * attempt);
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            Console.WriteLine($"OYSHO API HTTP xəta: {product.Name} - {ex.Message} (Cəhd {attempt}/3)");

                            if (attempt < 3)
                            {
                                await Task.Delay(2000 * attempt);
                            }
                        }
                    }

                    if (!success)
                    {
                        errorCount++;
                        Console.WriteLine($"OYSHO API: {product.Name} ATILDI (bütün cəhdlər uğursuz)");
                    }

                    // Hər məhsul arasında kiçik gecikmə
                    await Task.Delay(200);
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Console.WriteLine($"OYSHO API xəta: {product.Name} - {ex.Message} - DAVAM EDİR");
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OYSHO API kritik xəta: {ex.Message} - Göndərilən: {sentCount}/{totalProducts}");
        }

        Console.WriteLine($"OYSHO API NƏTİCƏ: Cəmi={totalProducts}, Göndərilən={sentCount}, Xəta={errorCount}");
    }

    private static async Task WarmupConnection()
    {
        try
        {
            await _client.GetAsync("https://www.oysho.com/az/");
            await Task.Delay(500);
        }
        catch
        {
            // Warmup xetasini ignore et
        }
    }

    private static string TruncateDescription(string description)
    {
        if (string.IsNullOrEmpty(description))
            return "";

        return description.Length > 200
            ? description.Substring(0, 200) + "..."
            : description;
    }

    private static string ExtractCategoryFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return "unknown";

        try
        {
            var match = System.Text.RegularExpressions.Regex.Match(url, @"category/(\d+)/");
            return match.Success ? match.Groups[1].Value : "unknown";
        }
        catch
        {
            return "unknown";
        }
    }

    private static void PrintStats(ScraperStats stats, int totalProducts)
    {
        Console.WriteLine($"\nOYSHO NETICE:");
        Console.WriteLine($"  Ugurlu: {stats.SuccessCount}");
        Console.WriteLine($"  Xeta: {stats.ErrorCount}");
        Console.WriteLine($"  Bos: {stats.EmptyCount}");
        Console.WriteLine($"  Cemi mehsul: {totalProducts}");
        Console.WriteLine($"  Ortalama: {(stats.SuccessCount > 0 ? totalProducts / stats.SuccessCount : 0)} mehsul/URL\n");
    }

    private class ScraperStats
    {
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public int EmptyCount { get; set; }
    }
}
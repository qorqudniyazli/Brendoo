using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScrapperWebAPI.Helpers.Mappers;
using ScrapperWebAPI.Models.BershkaModels;
using ScrapperWebAPI.Models.ProductDtos;

namespace ScrapperWebAPI.Helpers.Product
{
    public static class GetBershkaProducts
    {
        private static readonly HttpClient _client;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5, 5);
        private static readonly JsonSerializerSettings _jsonSettings;

        static GetBershkaProducts()
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
            _client.DefaultRequestHeaders.Add("Referer", "https://www.bershka.com/");
            _client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            _jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Error = (sender, args) =>
                {
                    var error = args.ErrorContext.Error;
                    var path = args.ErrorContext.Path;

                    Console.WriteLine($"BERSHKA JSON xeta: Path: {path} | Error: {error.Message}");
                    args.ErrorContext.Handled = true;
                }
            };
        }

        public static async Task<List<ProductToListDto>> FetchProductsAsync(List<string> urls)
        {
            if (urls == null || urls.Count == 0)
            {
                Console.WriteLine("BERSHKA: Bos URL list");
                return new List<ProductToListDto>();
            }

            Console.WriteLine($"BERSHKA: Basladi - {urls.Count} URL");

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
                    Console.WriteLine($"BERSHKA ERROR: URL {index + 1} - {ex.Message}");
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
                    Console.WriteLine($"BERSHKA: {current}/{total} - Bos cavab");
                    return null;
                }

                Root data;
                try
                {
                    data = JsonConvert.DeserializeObject<Root>(json, _jsonSettings);
                }
                catch (JsonException jsonEx)
                {
                    Console.WriteLine($"BERSHKA: {current}/{total} - JSON xetasi: {jsonEx.Message.Substring(0, Math.Min(80, jsonEx.Message.Length))}");
                    return null;
                }

                if (data?.products == null || data.products.Count == 0)
                {
                    Console.WriteLine($"BERSHKA: {current}/{total} - Mehsul yoxdur");
                    return null;
                }

                var products = BershkaProductMapper.Map(data);

                if (products != null && products.Count > 0)
                {
                    Console.WriteLine($"BERSHKA: {current}/{total} - {products.Count} mehsul");
                    await SendProductsToAPI(products, url);
                    return products;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BERSHKA: {current}/{total} - {ex.GetType().Name}: {ex.Message}");
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

            Console.WriteLine($"BERSHKA RETRY: {maxRetries} cehd ugursuz - {lastException?.Message}");
            return null;
        }

        private static async Task SendProductsToAPI(List<ProductToListDto> products, string sourceUrl)
        {
            if (products == null || products.Count == 0)
                return;

            try
            {
                using var apiClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };

                foreach (var product in products)
                {
                    try
                    {
                        var sizes = product.Sizes?.Select(s => new
                        {
                            sizeName = s.SizeName ?? "",
                            onStock = s.OnStock
                        }).ToList();

                        var colors = product.Colors?.Select(c => new
                        {
                            name = c.Name ?? "",
                            hex = c.Hex ?? ""
                        }).ToList();

                        var productData = new
                        {
                            name = product.Name ?? "",
                            brand = product.Brand ?? "Bershka",
                            price = product.Price / 100,
                            productUrl = product.ProductUrl ?? "",
                            discountedPrice = product.DiscountedPrice,
                            description = TruncateDescription(product.Description),
                            images = product.ImageUrl ?? new List<string>(),
                            sizes = sizes,
                            colors = colors,
                            store = "bershka",
                            category = ExtractCategoryFromUrl(sourceUrl),
                            processedAt = DateTime.Now.ToString("HH:mm:ss")
                        };

                        var json = JsonConvert.SerializeObject(productData);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await apiClient.PostAsync(
                            "http://69.62.114.202/api/stock/add",
                            content);

                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"BERSHKA API xeta: {product.Name} - {response.StatusCode}");
                        }

                        await Task.Delay(200);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"BERSHKA API xeta: {product.Name} - {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BERSHKA API kritik xeta: {ex.Message}");
            }
        }

        private static async Task WarmupConnection()
        {
            try
            {
                await _client.GetAsync("https://www.bershka.com/az/");
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
                var match = System.Text.RegularExpressions.Regex.Match(url, @"categoryId=(\d+)");
                return match.Success ? match.Groups[1].Value : "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        private static void PrintStats(ScraperStats stats, int totalProducts)
        {
            Console.WriteLine($"\nBERSHKA NETICE:");
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
}
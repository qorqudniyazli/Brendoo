using ScrapperWebAPI.Models.GoSport;
using ScrapperWebAPI.Models.ProductDtos;

namespace ScrapperWebAPI.Helpers.Mappers
{
    public static class GoSportMapper
    {
        private static readonly HttpClient _httpClient;
        private static readonly SemaphoreSlim _downloadSemaphore = new SemaphoreSlim(2);

        private static int _consecutiveFailures = 0;
        private const int MAX_CONSECUTIVE_FAILURES = 5;
        private const int MAX_IMAGE_SIZE_MB = 5;
        private const int BATCH_SIZE = 2;

        static GoSportMapper()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
                MaxConnectionsPerServer = 5,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 3
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }

        public static List<ProductToListDto> Map(List<GoSportProduct> products)
        {
            return MapAsync(products).Result;
        }

        public static async Task<List<ProductToListDto>> MapAsync(List<GoSportProduct> products)
        {
            if (products == null || products.Count == 0)
            {
                Console.WriteLine("MAPPER: Mehsul listi bosdur");
                return new List<ProductToListDto>();
            }

            var list = new List<ProductToListDto>();

            foreach (var item in products)
            {
                try
                {
                    if (item == null)
                    {
                        Console.WriteLine("MAPPER: Null mehsul atildi");
                        continue;
                    }

                    var product = new ProductToListDto()
                    {
                        Name = item.Name ?? "Ad yoxdur",
                        Price = item.Price,
                        Description = item.ShortDescription ?? "",
                        Brand = item.Brand ?? "",
                        ProductUrl = item.ProductUrl ?? "",
                        DiscountedPrice = item.DiscountedPrice,
                        ImageUrl = new List<string>(),
                        Colors = new List<Color>(),
                        Sizes = new List<Sizes>()
                    };

                    if (_consecutiveFailures >= MAX_CONSECUTIVE_FAILURES)
                    {
                        Console.WriteLine("MAPPER XETA: Coxlu ardıcıl xeta! Sekil yukleme dayandırıldı");
                        product.ImageUrl = new List<string>();
                    }
                    else
                    {
                        product.ImageUrl = await ConvertImagesToBase64Parallel(item.AdditionalImages, item.Name);
                    }

                    if (item.Sizes != null)
                    {
                        foreach (var size in item.Sizes)
                        {
                            if (size != null)
                            {
                                product.Sizes.Add(new Sizes
                                {
                                    SizeName = size.SizeName ?? "",
                                    OnStock = size.IsAvailable
                                });
                            }
                        }
                    }

                    list.Add(product);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("MAPPER XETA: Mehsul map xetasi - " + ex.Message);
                    continue;
                }
            }

            Console.WriteLine("MAPPER: " + list.Count + "/" + products.Count + " mehsul ugurla map edildi");
            return list;
        }

        private static async Task<List<string>> ConvertImagesToBase64Parallel(List<string> imageUrls, string productName)
        {
            if (imageUrls == null || imageUrls.Count == 0)
            {
                return new List<string>();
            }

            var validUrls = imageUrls.Where(url => !string.IsNullOrWhiteSpace(url)).ToList();

            if (validUrls.Count == 0)
            {
                return new List<string>();
            }

            Console.WriteLine("SEKIL: " + productName + " - " + validUrls.Count + " sekil yuklenir");

            var base64Images = new List<string>();
            int successCount = 0;
            int failCount = 0;

            for (int i = 0; i < validUrls.Count; i += BATCH_SIZE)
            {
                var batch = validUrls.Skip(i).Take(BATCH_SIZE).ToList();

                try
                {
                    var tasks = batch.Select(async imageUrl =>
                    {
                        await _downloadSemaphore.WaitAsync();
                        try
                        {
                            return await DownloadImageAsBase64(imageUrl);
                        }
                        finally
                        {
                            _downloadSemaphore.Release();
                        }
                    });

                    var results = await Task.WhenAll(tasks);

                    foreach (var result in results)
                    {
                        if (!string.IsNullOrEmpty(result))
                        {
                            base64Images.Add(result);
                            successCount++;
                            _consecutiveFailures = 0;
                        }
                        else
                        {
                            failCount++;
                            _consecutiveFailures++;
                        }
                    }

                    if (i + BATCH_SIZE < validUrls.Count)
                    {
                        await Task.Delay(500);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SEKIL BATCH XETA: " + ex.Message);
                    failCount += batch.Count;
                    _consecutiveFailures += batch.Count;
                }
            }

            Console.WriteLine("SEKIL: " + productName + " - " + successCount + "/" + validUrls.Count + " ugurlu");

            return base64Images;
        }

        private static async Task<string> DownloadImageAsBase64(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            const int maxRetries = 3;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

                    var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl, cts.Token);

                    if (imageBytes == null || imageBytes.Length == 0)
                    {
                        if (retry < maxRetries - 1)
                        {
                            await Task.Delay(1000 * (retry + 1));
                            continue;
                        }
                        return null;
                    }

                    double sizeMB = imageBytes.Length / (1024.0 * 1024.0);
                    if (sizeMB > MAX_IMAGE_SIZE_MB)
                    {
                        Console.WriteLine("SEKIL XETA: Coxlu boyuk - " + sizeMB.ToString("F2") + "MB");
                        return null;
                    }

                    var mimeType = GetMimeTypeFromUrl(imageUrl);
                    var base64String = Convert.ToBase64String(imageBytes);

                    return "data:" + mimeType + ";base64," + base64String;
                }
                catch (TaskCanceledException)
                {
                    if (retry < maxRetries - 1)
                    {
                        await Task.Delay(2000);
                        continue;
                    }
                    Console.WriteLine("SEKIL TIMEOUT: " + imageUrl.Substring(0, Math.Min(50, imageUrl.Length)));
                    return null;
                }
                catch (HttpRequestException ex)
                {
                    if (retry < maxRetries - 1)
                    {
                        await Task.Delay(2000);
                        continue;
                    }
                    Console.WriteLine("SEKIL HTTP XETA: " + ex.Message);
                    return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SEKIL UMUMI XETA: " + ex.Message);
                    return null;
                }
            }

            return null;
        }

        private static string GetMimeTypeFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "image/jpeg";

            try
            {
                var extension = Path.GetExtension(url).ToLowerInvariant();

                return extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    ".bmp" => "image/bmp",
                    ".svg" => "image/svg+xml",
                    _ => "image/jpeg"
                };
            }
            catch
            {
                return "image/jpeg";
            }
        }

        public static void Dispose()
        {
            try
            {
                _httpClient?.Dispose();
                _downloadSemaphore?.Dispose();
            }
            catch
            {
            }
        }
    }
}
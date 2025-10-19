using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ScrapperWebAPI.Models.BrandDtos;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ScrapperWebAPI.Helpers
{
    public static class GetStradivariusCategories
    {
        private static readonly HttpClient httpClient;

        static GetStradivariusCategories()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            httpClient = new HttpClient(handler);
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Anti-bot headers
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/html, */*");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7,az;q=0.6");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            httpClient.DefaultRequestHeaders.Add("Referer", "https://www.stradivarius.com/");
            httpClient.DefaultRequestHeaders.Add("Origin", "https://www.stradivarius.com");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\"");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");
        }

        // Cookie almaq üçün warmup
        private static async Task WarmUpConnection()
        {
            try
            {
                await httpClient.GetAsync("https://www.stradivarius.com/az/");
                await Task.Delay(500);
            }
            catch
            {
                // Ignore
            }
        }

        public static async Task<List<int>> GetCategoryIds()
        {
            var categoryIds = new List<int>();

            try
            {
                // Əvvəlcə connection warmup
                await WarmUpConnection();
                await Task.Delay(1000); // Anti-bot delay

                string apiUrl = "https://www.stradivarius.com/itxrest/2/catalog/store/55009626/50109552/category?languageId=-1&typeCatalog=1&appId=1";

                var response = await httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API Error: {response.StatusCode}");
                    return categoryIds;
                }

                string jsonContent = await response.Content.ReadAsStringAsync();

                // JSON parse
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                // categories array-ni tap
                if (root.TryGetProperty("categories", out JsonElement categories))
                {
                    foreach (var category in categories.EnumerateArray())
                    {
                        // Sadəcə birinci səviyyə subcategories
                        if (category.TryGetProperty("subcategories", out JsonElement subcategories))
                        {
                            foreach (var subcategory in subcategories.EnumerateArray())
                            {
                                if (subcategory.TryGetProperty("id", out JsonElement subId))
                                {
                                    categoryIds.Add(subId.GetInt32());
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"Total subcategories found: {categoryIds.Count}");
                return categoryIds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return categoryIds;
            }
        }

        // Category ID və onun product ID-lərini birlikdə saxlamaq üçün class
        private class CategoryProducts
        {
            public int CategoryId { get; set; }
            public List<string> ProductIds { get; set; } = new List<string>();
        }

        public static async Task<Dictionary<int, List<string>>> GetProductIdsByCategory(List<int> categoryIds)
        {
            var categoryProductsMap = new Dictionary<int, List<string>>();

            try
            {
                foreach (var categoryId in categoryIds)
                {
                    try
                    {
                        await Task.Delay(500); // Anti-bot delay hər sorğu arasında

                        string apiUrl = $"https://www.stradivarius.com/itxrest/3/catalog/store/55009626/50109552/category/{categoryId}/product?languageId=-1&showProducts=false&priceFilter=true&appId=1";

                        Console.WriteLine($"Fetching products for category: {categoryId}");

                        var response = await httpClient.GetAsync(apiUrl);

                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"API Error for category {categoryId}: {response.StatusCode}");
                            continue;
                        }

                        string jsonContent = await response.Content.ReadAsStringAsync();

                        // JSON parse
                        var jsonDoc = JsonDocument.Parse(jsonContent);
                        var root = jsonDoc.RootElement;

                        // productIds array-ni tap
                        if (root.TryGetProperty("productIds", out JsonElement productIds))
                        {
                            var productIdList = new List<string>();

                            foreach (var productId in productIds.EnumerateArray())
                            {
                                productIdList.Add(productId.GetInt64().ToString());
                            }

                            if (productIdList.Count > 0)
                            {
                                categoryProductsMap[categoryId] = productIdList;
                                Console.WriteLine($"Category {categoryId}: {productIdList.Count} products found");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing category {categoryId}: {ex.Message}");
                        continue;
                    }
                }

                Console.WriteLine($"\nTotal categories with products: {categoryProductsMap.Count}");

                return categoryProductsMap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return categoryProductsMap;
            }
        }

        public static async Task<List<string>> GetAllProductLinks()
        {
            var productLinks = new List<string>();

            try
            {
                Console.WriteLine("=== Starting Stradivarius Scraper ===\n");

                // 1. Category ID-lərini al
                Console.WriteLine("Step 1: Fetching category IDs...");
                var categoryIds = await GetCategoryIds();

                if (categoryIds.Count == 0)
                {
                    Console.WriteLine("No categories found!");
                    return productLinks;
                }

                Console.WriteLine($"Found {categoryIds.Count} categories\n");

                // 2. Hər category üçün product ID-lərini al
                Console.WriteLine("Step 2: Fetching product IDs from all categories...");
                var categoryProductsMap = await GetProductIdsByCategory(categoryIds);

                if (categoryProductsMap.Count == 0)
                {
                    Console.WriteLine("No products found!");
                    return productLinks;
                }

                // 3. Hər category üçün ayrı-ayrı linklər yarat
                Console.WriteLine("Step 3: Creating product links...\n");
                int batchSize = 50;
                int totalLinksCreated = 0;

                foreach (var categoryEntry in categoryProductsMap)
                {
                    int categoryId = categoryEntry.Key;
                    List<string> productIds = categoryEntry.Value;

                    int categoryBatches = (int)Math.Ceiling(productIds.Count / (double)batchSize);

                    for (int i = 0; i < productIds.Count; i += batchSize)
                    {
                        var batch = productIds.Skip(i).Take(batchSize).ToList();

                        string baseLink = $"https://www.stradivarius.com/itxrest/3/catalog/store/55009626/50109552/productsArray?languageId=-1&categoryId={categoryId}&productIds=";

                        // Product ID-ləri əlavə et
                        string productIdString = string.Join("%2C", batch);
                        string fullLink = baseLink + productIdString + "&appId=1";

                        productLinks.Add(fullLink);
                        totalLinksCreated++;

                        int batchNumber = (i / batchSize) + 1;
                        Console.WriteLine($"Category {categoryId} - Batch {batchNumber}/{categoryBatches}: {batch.Count} products");
                    }
                }

                Console.WriteLine($"\n=== Completed! Created {productLinks.Count} product links ===");

                return productLinks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllProductLinks: {ex.Message}");
                return productLinks;
            }
        }
    }
}
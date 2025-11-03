using System.Net;
using System.Text.Json;

namespace ScrapperWebAPI.Helpers;

public static class GetOyshoCategories
{
    private static readonly HttpClient httpClient;

    static GetOyshoCategories()
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
        httpClient.DefaultRequestHeaders.Add("Referer", "https://www.oysho.com/");
        httpClient.DefaultRequestHeaders.Add("Origin", "https://www.oysho.com");
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
            await httpClient.GetAsync("https://www.oysho.com/az/");
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

            string apiUrl = "https://www.oysho.com/itxrest/2/catalog/store/65009676/60361120/category?languageId=-1&typeCatalog=1&appId=1";

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

    public static async Task<List<long>> GetProductIds(List<int> categoryIds)
    {
        var allProductIds = new List<long>();

        try
        {
            foreach (var categoryId in categoryIds)
            {
                try
                {
                    await Task.Delay(500); // Anti-bot delay hər sorğu arasında

                    string apiUrl = $"https://www.oysho.com/itxrest/3/catalog/store/65009676/60361120/category/{categoryId}/product?languageId=-1&appId=1";

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
                        foreach (var productId in productIds.EnumerateArray())
                        {
                            allProductIds.Add(productId.GetInt64());
                        }

                        Console.WriteLine($"Category {categoryId}: {productIds.GetArrayLength()} products found");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing category {categoryId}: {ex.Message}");
                    continue;
                }
            }

            // Dublikatları təmizlə
            var uniqueProductIds = allProductIds.Distinct().ToList();
            Console.WriteLine($"\nTotal unique products: {uniqueProductIds.Count}");

            return uniqueProductIds;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return allProductIds;
        }
    }

    public static async Task<string> CreateLink(List<long> productIds)
    {
        string baselink = "https://www.oysho.com/itxrest/3/catalog/store/65009676/60361120/productsArray?languageId=-1&appId=1&productIds=";

        foreach (var item in productIds)
        {
            baselink += (item + "%2C");
        }

        // Son vergülü sil
        if (baselink.EndsWith("%2C"))
        {
            baselink = baselink.Substring(0, baselink.Length - 3);
        }

        return baselink;
    }

    public static async Task<List<string>> GetAllProductLinks()
    {
        var productLinks = new List<string>();

        try
        {
            Console.WriteLine("=== Starting Oysho Scraper ===\n");

            // 1. Category ID-lərini al
            Console.WriteLine("Step 1: Fetching category IDs...");
            var categoryIds = await GetCategoryIds();

            if (categoryIds.Count == 0)
            {
                Console.WriteLine("No categories found!");
                return productLinks;
            }

            Console.WriteLine($"Found {categoryIds.Count} categories\n");

            // 2. Product ID-lərini al
            Console.WriteLine("Step 2: Fetching product IDs from all categories...");
            var productIds = await GetProductIds(categoryIds);

            if (productIds.Count == 0)
            {
                Console.WriteLine("No products found!");
                return productLinks;
            }

            Console.WriteLine($"\nTotal unique products: {productIds.Count}\n");

            // 3. Product ID-ləri 50-lik qruplara böl (API limiti üçün)
            Console.WriteLine("Step 3: Creating product links...");
            int batchSize = 50;
            int totalBatches = (int)Math.Ceiling(productIds.Count / (double)batchSize);

            for (int i = 0; i < productIds.Count; i += batchSize)
            {
                var batch = productIds.Skip(i).Take(batchSize).ToList();

                string baseLink = "https://www.oysho.com/itxrest/3/catalog/store/65009676/60361120/productsArray?languageId=-1&appId=1&productIds=";

                // Product ID-ləri əlavə et
                string productIdString = string.Join("%2C", batch);
                string fullLink = baseLink + productIdString;

                productLinks.Add(fullLink);

                int batchNumber = (i / batchSize) + 1;
                Console.WriteLine($"Batch {batchNumber}/{totalBatches}: {batch.Count} products");
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

    private static void ExtractSubcategoryIds(JsonElement subcategories, HashSet<int> categoryIds)
    {
        foreach (var subcategory in subcategories.EnumerateArray())
        {
            if (subcategory.TryGetProperty("id", out JsonElement subId))
            {
                categoryIds.Add(subId.GetInt32());
            }

            // Daha dərin nested subcategories varsa
            if (subcategory.TryGetProperty("subcategories", out JsonElement nestedSubs))
            {
                ExtractSubcategoryIds(nestedSubs, categoryIds);
            }
        }
    }
}
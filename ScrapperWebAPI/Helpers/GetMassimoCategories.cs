using System.Net;
using System.Text.Json;

namespace ScrapperWebAPI.Helpers;

public static class GetMassimoCategories
{
    private static readonly HttpClient httpClient;

    static GetMassimoCategories()
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
        httpClient.DefaultRequestHeaders.Add("Referer", "https://www.massimodutti.com/");
        httpClient.DefaultRequestHeaders.Add("Origin", "https://www.massimodutti.com");
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
            await httpClient.GetAsync("https://www.massimodutti.com/az/");
            await Task.Delay(500);
        }
        catch
        {
            // Ignore
        }
    }

    public static async Task<List<int>> GetCategoryIds()
    {
        // *** BU HashSet İSTİFADƏ EDİRİK - DUPLIKAT OLMASIN ***
        var categoryIds = new HashSet<int>();

        try
        {
            // Əvvəlcə connection warmup
            await WarmUpConnection();
            await Task.Delay(1000); // Anti-bot delay

            string apiUrl = "https://www.massimodutti.com/itxrest/2/catalog/store/35009526/30359534/category?languageId=-20&appId=1";

            var response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                return categoryIds.ToList();
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
                    // *** ƏSAS DƏYİŞİKLİK: Rekursiv funksiya çağırırıq ***
                    if (category.TryGetProperty("subcategories", out JsonElement subcategories))
                    {
                        ExtractSubcategoryIds(subcategories, categoryIds);
                    }
                }
            }

            Console.WriteLine($"✅ MASSIMO: Cəmi kateqoriya tapıldı: {categoryIds.Count}");
            return categoryIds.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ MASSIMO Kateqoriya xətası: {ex.Message}");
            return categoryIds.ToList();
        }
    }

    public static async Task<List<string>> GetProductIds(List<int> categoryIds)
    {
        var allProductIds = new HashSet<string>(); // *** DUPLIKAT OLMASIN DEYƏ HashSet ***
        int processedCategories = 0;
        int totalCategories = categoryIds.Count;

        try
        {
            Console.WriteLine($"\n🔍 MASSIMO: {totalCategories} kateqoriyadan məhsullar çəkilir...\n");

            foreach (var categoryId in categoryIds)
            {
                try
                {
                    await Task.Delay(500); // Anti-bot delay hər sorğu arasında

                    string apiUrl = $"https://www.massimodutti.com/itxrest/3/catalog/store/35009526/30359534/category/{categoryId}/product?languageId=-20&appId=1&showProducts=false";

                    var response = await httpClient.GetAsync(apiUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"⚠️ Kateqoriya {categoryId}: HTTP {response.StatusCode}");
                        continue;
                    }

                    string jsonContent = await response.Content.ReadAsStringAsync();

                    // JSON parse
                    var jsonDoc = JsonDocument.Parse(jsonContent);
                    var root = jsonDoc.RootElement;

                    // productIds array-ni tap
                    if (root.TryGetProperty("productIds", out JsonElement productIds))
                    {
                        int categoryProductCount = 0;
                        foreach (var productId in productIds.EnumerateArray())
                        {
                            allProductIds.Add(productId.GetInt64().ToString());
                            categoryProductCount++;
                        }

                        processedCategories++;

                        // Hər 5 kateqoriyada bir progress göstər
                        if (processedCategories % 5 == 0 || categoryProductCount > 0)
                        {
                            Console.WriteLine($"📊 [{processedCategories}/{totalCategories}] Kateqoriya {categoryId}: {categoryProductCount} məhsul (Cəmi unique: {allProductIds.Count})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Kateqoriya {categoryId} xətası: {ex.Message}");
                    continue;
                }
            }

            Console.WriteLine($"\n{'=' * 70}");
            Console.WriteLine($"✅ MASSIMO NƏTİCƏ:");
            Console.WriteLine($"   Yoxlanılan kateqoriya: {processedCategories}/{totalCategories}");
            Console.WriteLine($"   Tapılan unique məhsul: {allProductIds.Count}");
            Console.WriteLine($"{'=' * 70}\n");

            return allProductIds.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ MASSIMO kritik xəta: {ex.Message}");
            return allProductIds.ToList();
        }
    }

    public static async Task<string> CreateLink(List<int> Productids)
    {
        string baselink = "https://www.massimodutti.com/itxrest/3/catalog/store/35009526/30359534/productsArray?languageId=-20&appId=1&productIds=";
        var list = new List<string>();
        foreach (var item in Productids)
        {
            baselink += (item + "%2C");
        }
        return baselink;
    }

    public static async Task<List<string>> GetAllProductLinks()
    {
        var productLinks = new List<string>();

        try
        {
            Console.WriteLine("=== 🚀 MASSIMO DUTTI SCRAPER BAŞLADI ===\n");

            // 1. Category ID-lərini al (İNDİ REKURSIV ÇƏKƏCƏK)
            Console.WriteLine("📁 Addım 1: Kateqoriya ID-ləri çəkilir (Rekursiv)...");
            var categoryIds = await GetCategoryIds();

            if (categoryIds.Count == 0)
            {
                Console.WriteLine("⚠️ Heç bir kateqoriya tapılmadı!");
                return productLinks;
            }

            Console.WriteLine($"✅ {categoryIds.Count} kateqoriya tapıldı\n");

            // 2. Product ID-lərini al
            Console.WriteLine("🛍️ Addım 2: Hər kateqoriyadan məhsullar çəkilir...");
            var productIds = await GetProductIds(categoryIds);

            if (productIds.Count == 0)
            {
                Console.WriteLine("⚠️ Heç bir məhsul tapılmadı!");
                return productLinks;
            }

            Console.WriteLine($"✅ Cəmi {productIds.Count} unique məhsul tapıldı\n");

            // 3. Product ID-ləri 50-lik qruplara böl (API limiti üçün)
            Console.WriteLine("🔗 Addım 3: Məhsul linkləri yaradılır...");
            int batchSize = 50;
            int totalBatches = (int)Math.Ceiling(productIds.Count / (double)batchSize);

            for (int i = 0; i < productIds.Count; i += batchSize)
            {
                var batch = productIds.Skip(i).Take(batchSize).ToList();

                string baseLink = "https://www.massimodutti.com/itxrest/3/catalog/store/35009526/30359534/productsArray?languageId=-20&appId=1&productIds=";

                // Product ID-ləri əlavə et
                string productIdString = string.Join("%2C", batch);
                string fullLink = baseLink + productIdString;

                productLinks.Add(fullLink);

                int batchNumber = (i / batchSize) + 1;
                Console.WriteLine($"  📦 Batch {batchNumber}/{totalBatches}: {batch.Count} məhsul");
            }

            Console.WriteLine($"\n{'=' * 70}");
            Console.WriteLine($"✅ TAMAMLANDI! {productLinks.Count} məhsul linki yaradıldı");
            Console.WriteLine($"{'=' * 70}\n");

            return productLinks;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GetAllProductLinks xətası: {ex.Message}");
            return productLinks;
        }
    }

    // *** BU FUNKSIYA İNDİ IŞLƏYIR - REKURSIV OLARAQ BÜTÜN NESTED KATEQORIYALARI ÇƏKIR ***
    private static void ExtractSubcategoryIds(JsonElement subcategories, HashSet<int> categoryIds)
    {
        foreach (var subcategory in subcategories.EnumerateArray())
        {
            if (subcategory.TryGetProperty("id", out JsonElement subId))
            {
                categoryIds.Add(subId.GetInt32());
            }

            // Daha dərin nested subcategories varsa - REKURSIV ÇAĞIR
            if (subcategory.TryGetProperty("subcategories", out JsonElement nestedSubs))
            {
                ExtractSubcategoryIds(nestedSubs, categoryIds);
            }
        }
    }
}
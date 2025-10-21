using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace ScrapperWebAPI.Helpers;

public static class GetBreshkaCategories
{
    private static readonly HttpClient httpClient;

    static GetBreshkaCategories()
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
        httpClient.DefaultRequestHeaders.Add("Referer", "https://www.bershka.com/");
        httpClient.DefaultRequestHeaders.Add("Origin", "https://www.bershka.com");
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
            await httpClient.GetAsync("https://www.bershka.com/az/");
            await Task.Delay(500);
        }
        catch
        {
            // Ignore
        }
    }
    public static async Task<List<string>> GetCategoryIds()
    {
        try
        {
            await WarmUpConnection();
            await Task.Delay(1000);
            var response = await httpClient.GetAsync("https://www.bershka.com/az/h-man.html");
            var response1 = await httpClient.GetAsync("https://www.bershka.com/az/h-woman.html");

            // HƏR İKİ SORĞU UĞURSUZ OLARSA DAYANDIR
            if (!response.IsSuccessStatusCode && !response1.IsSuccessStatusCode)
            {
                Console.WriteLine($"BERSHKA: Hər iki URL açılmadı");
                return new List<string>();
            }

            // BİR SORĞU UĞURSUZ OLARSA DAVAM ET
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"BERSHKA: MAN URL açılmadı - {response.StatusCode}");
            }
            if (!response1.IsSuccessStatusCode)
            {
                Console.WriteLine($"BERSHKA: WOMAN URL açılmadı - {response1.StatusCode}");
            }

            var html = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : "";
            var html1 = response1.IsSuccessStatusCode ? await response1.Content.ReadAsStringAsync() : "";

            var doc = new HtmlDocument();
            var doc1 = new HtmlDocument();
            doc.LoadHtml(html);
            doc1.LoadHtml(html1);

            var categoryIds = new HashSet<string>();

            // Bütün href-ləri tap
            var links = doc.DocumentNode.SelectNodes("//a[@href]");
            var links1 = doc1.DocumentNode.SelectNodes("//a[@href]");

            if (links != null)
            {
                foreach (var link in links)
                {
                    var href = link.GetAttributeValue("href", "");
                    // celement= parametrini tap
                    var match = Regex.Match(href, @"celement=(\d+)");
                    if (match.Success)
                    {
                        var categoryId = match.Groups[1].Value;
                        categoryIds.Add(categoryId);
                    }
                }
            }
            if (links1 != null)
            {
                foreach (var link in links1)
                {
                    var href = link.GetAttributeValue("href", "");
                    // celement= parametrini tap
                    var match = Regex.Match(href, @"celement=(\d+)");
                    if (match.Success)
                    {
                        var categoryId = match.Groups[1].Value;
                        categoryIds.Add(categoryId);
                    }
                }
            }
            Console.WriteLine($"BERSHKA: {categoryIds.Count} kateqoriya ID-si tapıldı");
            return categoryIds.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BERSHKA XƏTA: {ex.Message}");
            return new List<string>();
        }
    }
    public static async Task<List<string>> GetAllCategoryLinks()
    {
        var categoryIds = await GetCategoryIds();
        var categoryLinks = new List<string>();


        foreach (var categoryId in categoryIds)
        {
            var link = await GetCateogoryProducts(int.Parse(categoryId));
            if (!string.IsNullOrEmpty(link) && link != "Xeta")
            {
                categoryLinks.Add(link);
            }

            await Task.Delay(500); // Rate limiting
        }
        return categoryLinks;
    }

    public static async Task<string> GetCateogoryProducts(int storeId)
    {
        string apiUrl = $"https://www.bershka.com/itxrest/3/catalog/store/45009576/40259547/category/{storeId}/product?showProducts=false&showNoStock=false&appId=1&languageId=-20&locale=ru_RU";

        try
        {
            await WarmUpConnection();
            await Task.Delay(1000);

            var response = await httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"BERSHKA API: Xəta - {response.StatusCode}");

                // Debug info
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error details: {errorBody.Substring(0, Math.Min(100, errorBody.Length))}");

                return "";
            }

            var json = await response.Content.ReadAsStringAsync();

            // JSON-u parse et
            var jsonObject = JObject.Parse(json);

            // productIds array-ini tap və long list-ə çevir
            var productIds = jsonObject["productIds"]?
                .ToObject<List<long>>() ?? new List<long>();

            var Query = "";
            foreach (var item in productIds)
            {
                Query += item.ToString() + "%2C";
            }

            var link = $"https://www.bershka.com/itxrest/3/catalog/store/45009576/40259547/productsArray?categoryId={storeId}&productIds={Query}&appId=1&languageId=-20&locale=ru_RU";
            Console.WriteLine($"BERSHKA: {productIds.Count} məhsul ID-si tapıldı");
            return link;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BERSHKA GetProductIds XƏTA: {ex.Message}");
            return "Xeta";
        }
    }
}
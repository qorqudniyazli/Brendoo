using HtmlAgilityPack;
using ScrapperWebAPI.Models.BrandDtos;
using System.Net.Http;

public static class GetOliviaCategories
{
    private static readonly HttpClient _httpClient = new HttpClient();
    public static async Task<List<BrandToListDto>> GetAll(IHttpClientFactory httpClientFactory)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

        var response = await httpClient.GetAsync("https://olivia.az/ru/");
        if (!response.IsSuccessStatusCode) throw new Exception("Site unavailable");

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var result = new List<BrandToListDto>(200);
        var items = doc.DocumentNode.SelectNodes("//li[@class='catalogMenu__li-link ']");

        if (items == null) return result;

        foreach (var item in items)
        {
            var main = item.SelectSingleNode(".//span[@class='catalogMenu__title']")?.InnerText.Trim();
            if (string.IsNullOrEmpty(main)||main.Contains("Koreya") )continue;

            var subs = item.SelectNodes(".//div[@class='contentMenu__item']");
            if (subs == null) continue;

            foreach (var sub in subs)
            {
                var subName = sub.SelectSingleNode(".//a[@class='contentMenu__title']")?.InnerText.Trim();
                if (string.IsNullOrEmpty(subName)) continue;

                var thirds = sub.SelectNodes(".//div[@class='contentMenu__ul']//a");

                if (thirds != null)
                {
                    foreach (var third in thirds)
                    {
                        var thirdName = third.InnerText.Trim();
                        if (!string.IsNullOrEmpty(thirdName))
                        {
                            result.Add(new BrandToListDto
                            {
                                Name = $"{thirdName}",
                                Type = "category",
                                Img = "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"
                            });
                        }
                    }
                }
            }
        }

        return result;
    }
    public static async Task<string> GetCategoryUrl(string categoryName)
    {
        var response = await _httpClient.GetAsync("https://olivia.az/ru/");
        if (!response.IsSuccessStatusCode) throw new Exception("Site unavailable");

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var items = doc.DocumentNode.SelectNodes("//li[@class='catalogMenu__li-link ']");
        if (items == null) return null;

        foreach (var item in items)
        {
            var main = item.SelectSingleNode(".//span[@class='catalogMenu__title']")?.InnerText.Trim();
            if (string.IsNullOrEmpty(main) || main.Contains("Koreya")) continue;

            var subs = item.SelectNodes(".//div[@class='contentMenu__item']");
            if (subs == null) continue;

            foreach (var sub in subs)
            {
                var thirds = sub.SelectNodes(".//div[@class='contentMenu__ul']//a");

                if (thirds != null)
                {
                    foreach (var third in thirds)
                    {
                        var thirdName = third.InnerText.Trim();
                        if (thirdName.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                        {
                            return third.GetAttributeValue("href", "");
                        }
                    }
                }
            }
        }

        return null;
    }
}
using HtmlAgilityPack;
using ScrapperWebAPI.Models.BrandDtos;
using System.Net.Http;

namespace ScrapperWebAPI.Helpers
{
    public static class GetGoSportBrands
    {
        public async static Task<List<BrandToListDto>> GetAll()
        {
            HttpClient _httpClient = new HttpClient();
            var list = new List<BrandToListDto>();

            for (int page = 1; page <= 6; page++)
            {
                string url = $"https://www.gosport.az/brands?page={page}";
                var html = await _httpClient.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var brandNodes = doc.DocumentNode.SelectNodes("//div[@class='col-md-3 text-center mb-5']/a");

                if (brandNodes != null)
                {
                    foreach (var node in brandNodes)
                    {
                        string href = node.GetAttributeValue("href", "");
                        string name = node.SelectSingleNode(".//h4")?.InnerText.Trim();
                        string imgUrl = node.SelectSingleNode(".//img")?.GetAttributeValue("src", "");

                        string imageBase64 = null;

                        if (!string.IsNullOrEmpty(imgUrl))
                        {
                            try
                            {
                                var imgBytes = await _httpClient.GetByteArrayAsync(imgUrl);
                                imageBase64 = Convert.ToBase64String(imgBytes);
                            }
                            catch
                            {
                                imageBase64 = null;   
                            }
                        }

                        if (!string.IsNullOrEmpty(href) && !string.IsNullOrEmpty(name))
                        {
                            list.Add(new BrandToListDto()
                            {
                                Name = name,
                                Type = "brand",
                                Img = imageBase64
                            });
                        }
                    }
                }
            }
            return list;
        }

    }
}

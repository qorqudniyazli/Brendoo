using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ScrapperWebAPI.Helpers.Mappers;
using ScrapperWebAPI.Models.BershkaModels;
using ScrapperWebAPI.Models.ProductDtos;

namespace ScrapperWebAPI.Helpers.Product
{
    public static class GetBershkaProducts
    {
        public static async Task<List<ProductToListDto>> FetchProductsAsync(List<string> urls)
        {
            var results = new List<ProductToListDto>();
            using var client = new HttpClient();

            foreach (var url in urls)
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<Root>(json);

                    if (data != null)
                        results.AddRange(BershkaProductMapper.Map(data));
                }
                catch (HttpRequestException ex)
                {
                    // Sorğuda xəta baş verərsə, məsələn link yanlışdır
                    Console.WriteLine($"Xəta: {url} üçün sorğu uğursuz oldu. {ex.Message}");
                }
                catch (JsonException ex)
                {
                    // JSON çevirmə zamanı xəta olarsa
                    Console.WriteLine($"JSON çevirmə xətası: {url} -> {ex.Message}");
                }
            }

            return results;
        }
    }
}

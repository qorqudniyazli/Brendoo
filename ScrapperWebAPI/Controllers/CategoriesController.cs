using Microsoft.AspNetCore.Mvc;
using ScrapperWebAPI.Helpers;
using ScrapperWebAPI.Helpers.Product;
using System.Text;
using Newtonsoft.Json;

namespace ScrapperWebAPI.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory httpClientFactory;

    public CategoriesController(HttpClient httpClient, IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClient;
        this.httpClientFactory = httpClientFactory;
    }

    [HttpGet("test")]
    public async Task<IActionResult> A(string cateogry)
    {
        var data = await GetOliviaProducts.GetAllProductsFromCategory(cateogry);
        return Ok(data);

    }

    [HttpGet]
    public async Task<IActionResult> Get(string store)
    {
        try
        {
            Console.WriteLine("BASLADI: " + store.ToUpper());

            List<object> dataToSend = new List<object>();
            List<string> categoryNames = new List<string>();

            if (store.ToLower() == "gosport")
            {
                var data = await GetGoSportBrands.GetAll();
                foreach (var item in data)
                {
                    dataToSend.Add(new
                    {
                        name = item.Name,
                        type = item.Type,
                        img = item.Img,
                        store = store
                    });
                    categoryNames.Add(item.Name);
                }
            }
            else if (store.ToLower() == "zara")
            {
                var data = await GetZaraCategories.GetAll();
                foreach (var item in data)
                {
                    dataToSend.Add(new
                    {
                        name = item.Name,
                        type = item.Type,
                        img = item.Img,
                        store = store
                    });
                    categoryNames.Add(item.Name);
                }
            }
            else if (store.ToLower() == "olivia")
            {
                var data = await GetOliviaCategories.GetAll(httpClientFactory);
                foreach (var item in data)
                {
                    dataToSend.Add(new
                    {
                        name = item.Name,
                        type = item.Type,
                        img = item.Img,
                        store = store
                    });
                    categoryNames.Add(item.Name);
                }
            }
            else if (store.ToLower() == "bershka")
            { 
                categoryNames = await GetBreshkaCategories.GetAllCategoryLinks();
                dataToSend.Add(new
                {
                    name = "test",
                    type = "test",
                    img = "test",
                    store = store
                });
            }
            else if (store.ToLower() == "massimo")
            {
                categoryNames = await GetMassimoCategories.GetAllProductLinks();
                dataToSend.Add(new
                {
                    name = "test",
                    type = "test",
                    img = "test",
                    store = store
                });
            }
            else if (store.ToLower() == "stradivarius")
            {
                categoryNames = await GetStradivariusCategories.GetAllProductLinks();
                dataToSend.Add(new
                {
                    name = "test",
                    type = "test",
                    img = "test",
                    store = store
                });
            }
            else
            {
                return BadRequest("Store not found");
            }

            if (dataToSend.Count == 0)
            {
                return Ok(new { message = "Hec bir kateqoriya tapilmadi" });
            }

            Console.WriteLine("Kategoriya sayi: " + dataToSend.Count);

            await SendToExternalApi(dataToSend);
            Console.WriteLine("Kategoriyalar gonderildi");

            Console.WriteLine("Mehsullar islenmeye baslayir...");
            await ProcessAllCategories(store, categoryNames);
            Console.WriteLine("Butun mehsullar tamamlandi");

            return Ok(new
            {
                message = store.ToUpper() + " tamam oldu",
                categoriesCount = dataToSend.Count,
                categories = dataToSend
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in Categories GET: " + ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task ProcessAllCategories(string store, List<string> categoryNames)
    {
        for (int i = 0; i < categoryNames.Count; i++)
        {
            var categoryName = categoryNames[i];

            try
            {
                Console.WriteLine("[" + (i + 1) + "/" + categoryNames.Count + "] " + categoryName + " basladi");

                if (store.ToLower() == "gosport")
                {
                    var products = await GetGoSportProducts.GetByProductByBrand(categoryName);
                    Console.WriteLine(categoryName + ": " + (products?.Count ?? 0) + " mehsul");
                }
                else if (store.ToLower() == "zara")
                {
                    var products = await GetZaraProduct.GetByCategoryName(categoryName);
                    Console.WriteLine(categoryName + ": " + (products?.Count ?? 0) + " mehsul");
                }
                else if (store.ToLower() == "olivia")
                {
                    var products = await GetOliviaProducts.GetAllProductsFromCategory(categoryName);
                    Console.WriteLine(categoryName + ": " + (products?.Count ?? 0) + " mehsul");
                }
                else if (store.ToLower() == "bershka")
                {
                    
                    var data = await GetBershkaProducts.FetchProductsAsync(categoryNames);
                    break;
                }
                else if (store.ToLower() == "massimo")
                {

                    var data = await GetMassimoProducts.FetchProductsAsync(categoryNames);
                    break;
                }

                else if (store.ToLower() == "stradivarius")
                {

                    var data = await GetStradivariusProducts.FetchProductsAsync(categoryNames);
                    break;
                }

                Console.WriteLine("[" + (i + 1) + "/" + categoryNames.Count + "] " + categoryName + " tamamlandi");

                if (i < categoryNames.Count - 1)
                {
                    await Task.Delay(2000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("XETA " + categoryName + ": " + ex.Message);
                continue;
            }
        }
    }

    private async Task SendToExternalApi(List<object> data)
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "http://69.62.114.202/api/stock/add-category",
                content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Categories API ugurlu");
            }
            else
            {
                Console.WriteLine("Categories API xetasi: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Categories API exception: " + ex.Message);
        }
    }
}
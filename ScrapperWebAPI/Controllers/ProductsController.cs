using Microsoft.AspNetCore.Mvc;
using ScrapperWebAPI.Helpers.Product;
using ScrapperWebAPI.Models.ProductDtos;
using System.Text;
using Newtonsoft.Json;
using ScrapperWebAPI.Helpers;

namespace ScrapperWebAPI.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(string store, string category)
    {
        try
        {
            Console.WriteLine("Products API cagrildi: " + store + " - " + category);

            List<ProductToListDto> allProducts = new List<ProductToListDto>();

            if (store.ToLower() == "gosport")
            {
                allProducts = await GetGoSportProducts.GetByProductByBrand(category);
            }
            else if (store.ToLower() == "zara")
            {
                allProducts = await GetZaraProduct.GetByCategoryName(category);
            }
            else
            {
                return BadRequest("This store can not be found");
            }

            if (allProducts == null || allProducts.Count == 0)
            {
                Console.WriteLine("Hec bir mehsul tapilmadi: " + store + " - " + category);
                return Ok(new List<ProductToListDto>());
            }

            Console.WriteLine("Tapilan mehsul sayi: " + allProducts.Count);

            return Ok(allProducts);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Products GET xetasi: " + ex.Message);
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll(string store)
    {
        try
        {
            Console.WriteLine("GetAll basladi: " + store.ToUpper());

            if (store.ToLower() == "zara")
            {
                var categories = await GetZaraCategories.GetAll();

                Console.WriteLine("Zara kategoriya sayi: " + categories.Count);

                foreach (var category in categories)
                {
                    try
                    {
                        Console.WriteLine("Islenir: " + category.Name);

                        var products = await GetZaraProduct.GetByCategoryName(category.Name);

                        if (products != null && products.Count > 0)
                        {
                            Console.WriteLine(category.Name + " ucun " + products.Count + " mehsul tapildi");
                        }
                        else
                        {
                            Console.WriteLine(category.Name + " ucun mehsul yoxdur");
                        }

                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Xeta: " + category.Name + " - " + ex.Message);
                        continue;
                    }
                }
            }
            else if (store.ToLower() == "gosport")
            {
                var brands = await GetGoSportBrands.GetAll();

                Console.WriteLine("GoSport brand sayi: " + brands.Count);

                foreach (var brand in brands)
                {
                    try
                    {
                        Console.WriteLine("Islenir: " + brand.Name);

                        var products = await GetGoSportProducts.GetByProductByBrand(brand.Name);

                        if (products != null && products.Count > 0)
                        {
                            Console.WriteLine(brand.Name + " ucun " + products.Count + " mehsul tapildi");
                        }
                        else
                        {
                            Console.WriteLine(brand.Name + " ucun mehsul yoxdur");
                        }

                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Xeta: " + brand.Name + " - " + ex.Message);
                        continue;
                    }
                }
            }
            else
            {
                return BadRequest("Store not found");
            }

            return Ok(new { message = store.ToUpper() + " magazasi tamamlandi" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetAll xetasi: " + ex.Message);
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using ScrapperWebAPI.Helpers.Product;
using System.Text.Json;

namespace ScrapperWebAPI.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class DebugController : ControllerBase
{
    [HttpGet("test-images")]
    public async Task<IActionResult> TestImages(string store = "gosport", string category = "NIKE")
    {
        try
        {
            Console.WriteLine($"Testing images for: {store} - {category}");

            List<ScrapperWebAPI.Models.ProductDtos.ProductToListDto> products;

            if (store.ToLower() == "gosport")
            {
                products = await GetGoSportProducts.GetByProductByBrand(category);
            }
            else
            {
                products = await GetZaraProduct.GetByCategoryName(category);
            }

            if (products == null || products.Count == 0)
            {
                return Ok(new { message = "Məhsul tapılmadı", store, category });
            }

            var firstProduct = products[0];

            var debugInfo = new
            {
                productName = firstProduct.Name,
                imageUrlType = firstProduct.ImageUrl?.GetType().Name,
                imageUrlCount = firstProduct.ImageUrl?.Count ?? 0,
                imageUrls = firstProduct.ImageUrl?.Take(3).ToList(), // İlk 3 şəkil
                hasImages = firstProduct.ImageUrl != null && firstProduct.ImageUrl.Count > 0,

                allProducts = products.Take(2).Select(p => new
                {
                    name = p.Name,
                    price = p.Price,
                    imageCount = p.ImageUrl?.Count ?? 0,
                    firstImagePreview = p.ImageUrl?.FirstOrDefault()?.Substring(0, Math.Min(50, p.ImageUrl.FirstOrDefault()?.Length ?? 0)) + "...",
                    hasImageUrl = p.ImageUrl != null,
                    imageUrlIsEmpty = p.ImageUrl?.Count == 0
                }).ToList()
            };

            return Ok(debugInfo);
        }
        catch (Exception ex)
        {
            return Ok(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpGet("test-json")]
    public async Task<IActionResult> TestJSON(string store = "gosport", string category = "NIKE")
    {
        try
        {
            List<ScrapperWebAPI.Models.ProductDtos.ProductToListDto> products;

            if (store.ToLower() == "gosport")
            {
                products = await GetGoSportProducts.GetByProductByBrand(category);
            }
            else
            {
                products = await GetZaraProduct.GetByCategoryName(category);
            }

            if (products == null || products.Count == 0)
            {
                return Ok(new { message = "Məhsul tapılmadı" });
            }

            var formattedProducts = new List<object>();

            foreach (var product in products.Take(2)) // Yalnız ilk 2 məhsul test üçün
            {
                var sizes = new List<object>();
                if (product.Sizes != null)
                {
                    foreach (var size in product.Sizes)
                    {
                        sizes.Add(new { sizeName = size.SizeName, onStock = size.OnStock });
                    }
                }

                var colors = new List<object>();
                if (product.Colors != null)
                {
                    foreach (var color in product.Colors)
                    {
                        colors.Add(new { name = color.Name, hex = color.Hex });
                    }
                }

                var productData = new
                {
                    name = product.Name ?? "",
                    brand = product.Brand ?? "",
                    price = product.Price,
                    discountedPrice = product.DiscountedPrice,
                    description = !string.IsNullOrEmpty(product.Description) && product.Description.Length > 150
                        ? product.Description.Substring(0, 150) + "..."
                        : product.Description ?? "",
                    images = product.ImageUrl ?? new List<string>(),
                    sizes = sizes,
                    colors = colors,
                    store = store,
                    category = category,
                    processedAt = DateTime.Now.ToString("HH:mm:ss")
                };

                formattedProducts.Add(productData);
            }

            var json = JsonSerializer.Serialize(formattedProducts, new JsonSerializerOptions { WriteIndented = true });

            return Ok(new
            {
                totalProducts = products.Count,
                formattedProductsCount = formattedProducts.Count,
                jsonSize = json.Length,
                sampleJson = json,
                products = formattedProducts
            });
        }
        catch (Exception ex)
        {
            return Ok(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}
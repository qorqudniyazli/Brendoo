using ScrapperWebAPI.Models.GoSport;
using ScrapperWebAPI.Models.ProductDtos;

namespace ScrapperWebAPI.Helpers.Mappers;

public static class GoSportMapper
{
    public static List<ProductToListDto> Map(List<GoSportProduct> products)
    {
        if (products == null || products.Count == 0)
        {
            Console.WriteLine("MAPPER: Mehsul listi bosdur");
            return new List<ProductToListDto>();
        }

        var list = new List<ProductToListDto>();

        foreach (var item in products)
        {
            try
            {
                if (item == null)
                {
                    Console.WriteLine("MAPPER: Null mehsul atildi");
                    continue;
                }

                var product = new ProductToListDto()
                {
                    Name = item.Name ?? "Ad yoxdur",
                    Price = item.Price,
                    Description = item.ShortDescription ?? "",
                    Brand = item.Brand ?? "",
                    ProductUrl = item.ProductUrl ?? "",
                    DiscountedPrice = item.DiscountedPrice,
                    ImageUrl = item.AdditionalImages ?? new List<string>(),
                    Colors = new List<Color>(),
                    Sizes = new List<Sizes>()
                };

                if (item.Sizes != null)
                {
                    foreach (var size in item.Sizes)
                    {
                        if (size != null)
                        {
                            product.Sizes.Add(new Sizes
                            {
                                SizeName = size.SizeName ?? "",
                                OnStock = size.IsAvailable
                            });
                        }
                    }
                }

                list.Add(product);
            }
            catch (Exception ex)
            {
                Console.WriteLine("MAPPER XETA: Mehsul map xetasi - " + ex.Message);
                continue;
            }
        }

        Console.WriteLine("MAPPER: " + list.Count + "/" + products.Count + " mehsul ugurla map edildi");
        return list;
    }

    public static async Task<List<ProductToListDto>> MapAsync(List<GoSportProduct> products)
    {
        return Map(products);
    }
}
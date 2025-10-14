using ScrapperWebAPI.Models.BershkaModels;
using ScrapperWebAPI.Models.ProductDtos;

namespace ScrapperWebAPI.Helpers.Mappers
{
    public static class BershkaProductMapper
    {
        public static List<ProductToListDto> Map(Root root)
        {
            var mappedProducts = new List<ProductToListDto>();

            if (root?.products == null)
            {
                Console.WriteLine("BERSHKA MAPPER: Root və ya products null-dur");
                return mappedProducts;
            }

            foreach (var product in root.products)
            {
                try
                {
                    // Null check
                    if (product?.bundleProductSummaries == null ||
                        product.bundleProductSummaries.Count == 0 ||
                        product.bundleProductSummaries[0]?.detail == null)
                    {
                        Console.WriteLine($"BERSHKA MAPPER: {product?.name ?? "Unknown"} - Detail yoxdur");
                        continue;
                    }

                    var detail = product.bundleProductSummaries[0].detail;

                    if (detail.colors == null || detail.colors.Count == 0)
                    {
                        Console.WriteLine($"BERSHKA MAPPER: {product.name} - Rəng yoxdur");
                        continue;
                    }

                    var firstColor = detail.colors[0];

                    if (firstColor.sizes == null || firstColor.sizes.Count == 0)
                    {
                        Console.WriteLine($"BERSHKA MAPPER: {product.name} - Ölçü yoxdur");
                        continue;
                    }

                    var mappedProduct = new ProductToListDto
                    {
                        Name = product.name ?? "Unknown",
                        Brand = "Bershka",
                        Price = 0, // Default
                        Sizes = new List<Sizes>(),
                        Colors = new List<ScrapperWebAPI.Models.ProductDtos.Color>(),
                        Description = detail.description ?? "",
                        ProductUrl = "https://www.bershka.com/az/" +
                                   (product.bundleProductSummaries[0].productUrl ?? "") +
                                   ".html?colorId=800",
                        ImageUrl = new List<string>()
                    };

                    // Qiymət
                    try
                    {
                        if (!string.IsNullOrEmpty(firstColor.sizes[0].price))
                        {
                            mappedProduct.Price = decimal.Parse(firstColor.sizes[0].price);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"BERSHKA MAPPER: Qiymət xətası {product.name} - {ex.Message}");
                    }

                    // Rəng
                    if (!string.IsNullOrEmpty(firstColor.name))
                    {
                        mappedProduct.Colors.Add(new ScrapperWebAPI.Models.ProductDtos.Color
                        {
                            Name = firstColor.name,
                            Hex = ""
                        });
                    }

                    // Şəkillər
                    try
                    {
                        if (detail.xmedia != null &&
                            detail.xmedia.Count > 0 &&
                            detail.xmedia[0].xmediaItems != null &&
                            detail.xmedia[0].xmediaItems.Count > 0)
                        {
                            var medias = detail.xmedia[0].xmediaItems[0].medias;

                            if (medias != null)
                            {
                                foreach (var media in medias)
                                {
                                    if (media?.extraInfo?.deliveryUrl != null)
                                    {
                                        mappedProduct.ImageUrl.Add(media.extraInfo.deliveryUrl);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"BERSHKA MAPPER: Şəkil xətası {product.name} - {ex.Message}");
                    }

                    // Ölçülər
                    foreach (var size in firstColor.sizes)
                    {
                        if (size != null)
                        {
                            mappedProduct.Sizes.Add(new Sizes
                            {
                                SizeName = size.name ?? "",
                                OnStock = size.isBuyable
                            });
                        }
                    }

                    mappedProducts.Add(mappedProduct);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"BERSHKA MAPPER XƏTA: {product?.name ?? "Unknown"} - {ex.Message}");
                    continue;
                }
            }

            Console.WriteLine($"BERSHKA MAPPER: {mappedProducts.Count}/{root.products.Count} məhsul map edildi");
            return mappedProducts;
        }
    }
}
using ScrapperWebAPI.Models.MassimoModels;
using ScrapperWebAPI.Models.ProductDtos;

namespace ScrapperWebAPI.Helpers.Mappers;

public static class PullAndBearProductMapper
{
    public static List<ProductToListDto> Map(Root root)
    {
        var mappedProducts = new List<ProductToListDto>();

        if (root?.products == null)
        {
            Console.WriteLine("Pull And Bear MAPPER: Root və ya products null-dur");
            return mappedProducts;
        }

        foreach (var product in root.products)
        {
            try
            {
                if (product?.bundleProductSummaries == null ||
                    product.bundleProductSummaries.Count == 0 ||
                    product.bundleProductSummaries[0]?.detail == null)
                {
                    Console.WriteLine($"Pull And Bear MAPPER: {product?.name ?? "Unknown"} - Detail yoxdur");
                    continue;
                }

                var detail = product.bundleProductSummaries[0].detail;

                if (detail.colors == null || detail.colors.Count == 0)
                {
                    Console.WriteLine($"Pull And Bear MAPPER: {product.name} - Rəng yoxdur");
                    continue;
                }

                // ✅ HƏR RƏNG ÜÇÜN 1 DƏFƏ (index ilə)
                for (int i = 0; i < detail.colors.Count; i++)
                {
                    var color = detail.colors[i];

                    if (color?.sizes == null || color.sizes.Count == 0)
                    {
                        Console.WriteLine($"Pull And Bear MAPPER: {product.name} - {color?.name} üçün ölçü yoxdur");
                        continue;
                    }

                    // ✅ BU RƏNGƏ AID colorCode tap
                    string colorCode = "800"; // default

                    // xmedia və colors eyni sırada olmalıdır
                    if (detail.xmedia != null && i < detail.xmedia.Count)
                    {
                        colorCode = detail.xmedia[i].colorCode ?? colorCode;
                    }

                    var mappedProduct = new ProductToListDto
                    {
                        Name = product.name ?? "Unknown",
                        Brand = "PullAndBear",
                        Price = 0,
                        Sizes = new List<Sizes>(),
                        Colors = new List<ScrapperWebAPI.Models.ProductDtos.Color>(),
                        Description = detail.longDescription ?? "",

                        // ✅ HƏR RƏNGƏ AID FƏRQLI URL
                        ProductUrl = "https://www.pullandbear.com/az/" +
                                   (product.bundleProductSummaries[0].productUrl ?? "") +
                                   $"?colorId={colorCode}",

                        ImageUrl = new List<string>()
                    };

                    // Qiymət
                    try
                    {
                        if (!string.IsNullOrEmpty(color.sizes[0].price))
                        {
                            mappedProduct.Price = decimal.Parse(color.sizes[0].price);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Pull And Bear MAPPER: Qiymət xətası {product.name} - {ex.Message}");
                    }

                    // ✅ YALNIZ BU RƏNGI əlavə et
                    if (!string.IsNullOrEmpty(color.name))
                    {
                        mappedProduct.Colors.Add(new ScrapperWebAPI.Models.ProductDtos.Color
                        {
                            Name = color.name,
                            Hex = ""
                        });
                    }

                    // ✅ BU RƏNGƏ AID şəkillər
                    try
                    {
                        if (detail.xmedia != null && i < detail.xmedia.Count)
                        {
                            var colorXmedia = detail.xmedia[i];

                            if (colorXmedia?.xmediaItems != null &&
                                colorXmedia.xmediaItems.Count > 0 &&
                                colorXmedia.xmediaItems[0].medias != null)
                            {
                                var medias = colorXmedia.xmediaItems[0].medias;

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
                        Console.WriteLine($"Pull And Bear MAPPER: Şəkil xətası {product.name} - {color.name} - {ex.Message}");
                    }

                    // Ölçülər
                    foreach (var size in color.sizes)
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

                    if (!mappedProducts.Any(x => x.ProductUrl == mappedProduct.ProductUrl))
                    {
                        mappedProduct.Colors = new List<Models.ProductDtos.Color>();

                        mappedProducts.Add(mappedProduct);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pull And Bear MAPPER XƏTA: {product?.name ?? "Unknown"} - {ex.Message}");
                continue;
            }
        }

        Console.WriteLine($"Pull And Bear MAPPER: {mappedProducts.Count} məhsul map edildi");
        return mappedProducts;
    }
}
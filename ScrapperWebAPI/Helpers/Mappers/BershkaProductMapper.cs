using ScrapperWebAPI.Models.BershkaModels;
using ScrapperWebAPI.Models.ProductDtos;

namespace ScrapperWebAPI.Helpers.Mappers
{
    public static class BershkaProductMapper
    {
        public static List<ProductToListDto> Map(Root root)
        {
            var mappedProducts = new List<ProductToListDto>();
            foreach (var product in root.products)
            {
                if (product.bundleProductSummaries is null) continue;

                var mappedProduct = new ProductToListDto
                {
                    Name = product.name,
                    Brand = "Bershka",
                    Price = decimal.Parse(product.bundleProductSummaries[0].detail.colors[0].sizes[0].price),
                    Sizes = new List<Sizes>(),
                    Colors = new List<ScrapperWebAPI.Models.ProductDtos.Color>() { new Models.ProductDtos.Color() { Name = product.bundleProductSummaries[0].detail.colors[0].name, Hex = "" } },
                    Description = product.bundleProductSummaries[0].detail.description,
                    ProductUrl = "https://www.bershka.com/az/" + product.bundleProductSummaries[0].productUrl + ".html?colorId=800",
                };



                List<string> images = new List<string>();
                foreach (var item in product.bundleProductSummaries[0].detail.xmedia[0].xmediaItems[0].medias)
                {
                    images.Add(item.extraInfo.deliveryUrl);
                }
                mappedProduct.ImageUrl = images;
                foreach (var item in product.bundleProductSummaries[0].detail.colors[0].sizes)
                {
                    mappedProduct.Sizes.Add(new Sizes
                    {
                        SizeName = item.name,
                        OnStock = item.isBuyable
                    });
                }
                mappedProducts.Add(mappedProduct);
            }
            return mappedProducts;
        }
    }
}

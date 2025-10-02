using ScrapperWebAPI.Helpers.Product;
using ScrapperWebAPI.Models.ProductDtos;

namespace ScrapperWebAPI.Helpers.Mappers;

public static class OliviaProductMapper
{
    public static ProductToListDto MapToDto(ProductDto oliviaProduct)
    {
        if (oliviaProduct == null)
            return null;

        // Sizes-dan yalnız həcm/ölçü məlumatlarını filtr et
        var validSizes = FilterValidSizes(oliviaProduct.Sizes);

        return new ProductToListDto
        {
            Name = oliviaProduct.Title,
            Brand = ExtractBrand(oliviaProduct.Title),
            Description = "",
            Price = oliviaProduct.Price,
            DiscountedPrice = oliviaProduct.DiscountedPrice,
            ProductUrl = oliviaProduct.Url,
            Colors = new List<Color>(),
            Sizes = validSizes.Select(s => new Sizes
            {
                SizeName = s,
                OnStock = true
            }).ToList(),
            ImageUrl = oliviaProduct.Images
        };
    }

    private static List<string> FilterValidSizes(List<string> sizes)
    {
        if (sizes == null || !sizes.Any())
            return new List<string>();

        var validSizes = new List<string>();

        foreach (var size in sizes)
        {
            var trimmedSize = size.Trim();

            // "Нет информации", "Россия" və s. kimi yanlış məlumatları filtr et
            if (string.IsNullOrWhiteSpace(trimmedSize) ||
                trimmedSize.Equals("Нет информации", StringComparison.OrdinalIgnoreCase) ||
                trimmedSize.Equals("Россия", StringComparison.OrdinalIgnoreCase) ||
                trimmedSize.Equals("Турция", StringComparison.OrdinalIgnoreCase) ||
                trimmedSize.Length > 50) // Çox uzun mətnləri də keç
            {
                continue;
            }

            // Həcm/ölçü formatını yoxla (ml, l, qr, kq, sm və s.)
            if (IsValidSizeFormat(trimmedSize))
            {
                validSizes.Add(trimmedSize);
            }
        }

        return validSizes;
    }

    private static bool IsValidSizeFormat(string size)
    {
        // Həcm/ölçü vahidlərini yoxla
        var validUnits = new[] { "мл", "л", "гр", "кг", "г", "ml", "l", "g", "kg", "см", "cm", "мм", "mm" };

        return validUnits.Any(unit => size.ToLower().Contains(unit));
    }

    private static string ExtractBrand(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "";

        // Məsələn: "Üz Kremi Elfa Pharm Vis Plantis..." -> "Elfa Pharm"
        var parts = title.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // İlk iki sözü brand kimi götür (əksər hallarda düzgün işləyir)
        if (parts.Length >= 3)
        {
            return $"{parts[2]} {(parts.Length > 3 ? parts[3] : "")}".Trim();
        }

        return "";
    }

    public static List<ProductToListDto> MapToDtoList(List<ProductDto> oliviaProducts)
    {
        if (oliviaProducts == null || !oliviaProducts.Any())
            return new List<ProductToListDto>();

        return oliviaProducts
            .Select(MapToDto)
            .Where(p => p != null)
            .ToList();
    }
}
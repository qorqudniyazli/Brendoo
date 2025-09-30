namespace ScrapperWebAPI.Models.ProductDtos
{
    public class ProductToListDto
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string ProductUrl { get; set; }
        public string Description { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public List<string> ImageUrl { get; set; }
        public List<Color> Colors { get; set; }
        public List<Sizes> Sizes { get; set; }
    }
    public class Sizes
    {
        public string SizeName { get; set; }
        public bool OnStock { get; set; }
    }
    public class Color
    {
        public string Name { get; set; }
        public string Hex { get; set; }
    }
}

namespace ScrapperWebAPI.Models.GoSport;

public class GoSportProduct
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public string ImageUrl { get; set; }
    public string ProductUrl { get; set; }
    public string ShortDescription { get; set; }
    public bool HasDiscount { get; set; }
    public List<GoSportProductSize> Sizes { get; set; } = new List<GoSportProductSize>();
    public List<string> Categories { get; set; } = new List<string>();
    public List<string> AdditionalImages { get; set; } = new List<string>();
}

public class GoSportProductSize
{
    public string SizeName { get; set; }
    public bool IsAvailable { get; set; }
}
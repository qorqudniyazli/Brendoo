namespace ZaraScraperWebApi.Models;

public class ZaraRootSeo
{
    public List<ProductGroupSeo> ProductGroups { get; set; }
}

public class ProductGroupSeo
{
    public List<ElementSeo> Elements { get; set; }
}

public class ElementSeo
{
    // Seo burada DEYİL! CommercialComponents-də var
    public List<CommercialComponentSeo> CommercialComponents { get; set; }
}

public class CommercialComponentSeo
{
    public Seo Seo { get; set; } // Seo burada!
}

public class Seo
{
    public string Keyword { get; set; }
    public string SeoProductId { get; set; }
    public long DiscernProductId { get; set; }
}
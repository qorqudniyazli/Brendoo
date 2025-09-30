using Newtonsoft.Json;
namespace ScrapperWebAPI.Models.Zara
{
    public class ZaraCategoryRoot
    {
        [JsonProperty("categories")]
        public List<Category> Categories { get; set; }
    }

    public class Category
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("sectionName")]
        public string SectionName { get; set; }
        [JsonProperty("subcategories")]
        public List<Subcategory> Subcategories { get; set; }
    }

    public class Subcategory
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("sectionName")]
        public string SectionName { get; set; }

        // Bu əlavə edilməlidir - nested subcategories üçün
        [JsonProperty("subcategories")]
        public List<Subcategory> Subcategories { get; set; }
    }

    // Response üçün istifadə etdiyin modellər
    public class CategoryWithSubcategories
    {
        public string SectionName { get; set; }
        public List<SubcategoryResponse> Subcategories { get; set; }
    }

    public class SubcategoryResponse
    {
        public long SubcategoryId { get; set; }
        public string Name { get; set; }
        public string link { get; set; }
    }
}
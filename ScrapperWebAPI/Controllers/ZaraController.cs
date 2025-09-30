using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ScrapperWebAPI.Models.Zara;
using ScrapperWebAPI.Models.Zara.Product;
using System.Xml.Linq;
using ZaraScraperWebApi.Models;

namespace ZaraScraperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZaraController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ZaraController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("getCategories")]
        public async Task<IActionResult> Get()
        {
            var result = await GetCategories();

            return Ok(result);
        }

        [HttpGet("getProduct")]
        public async Task<IActionResult> GetSeo(int id)
        {
            var subcategory = (await GetCategories())
                .SelectMany(x => x.Subcategories)
                .FirstOrDefault(sc => sc.SubcategoryId == id);

            var response = await _httpClient.GetAsync(subcategory.link);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Xəta baş verdi.");

            string json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ZaraRootSeo>(json);

           

            HashSet<string> links = new HashSet<string>();
            foreach (var productGroup in result.ProductGroups)
            {
                foreach (var element in productGroup.Elements)
                {
                    if (element.CommercialComponents != null)
                    {
                        foreach (var component in element.CommercialComponents)
                        {
                            if (component.Seo != null &&
                                !string.IsNullOrEmpty(component.Seo.Keyword) &&
                                !string.IsNullOrEmpty(component.Seo.SeoProductId))
                            {
                                var link = $"https://www.zara.com/az/ru/{component.Seo.Keyword}-p{component.Seo.SeoProductId}.html?ajax=true";
                                links.Add(link);
                            }
                        }
                    }
                }
            }

            List<Root> productDetails = new List<Root>();

            foreach (var link in links)
            {
                try
                {
                    var linkResponse = await _httpClient.GetAsync(link);
                    if (linkResponse.IsSuccessStatusCode)
                    {
                        string linkJson = await linkResponse.Content.ReadAsStringAsync();
                        var productDetail = JsonConvert.DeserializeObject<Root>(linkJson);
                        if (productDetail != null)
                        {
                            productDetails.Add(productDetail);
                        }
                    }
                    else
                    {
                        // Log the failed request or handle as needed
                        Console.WriteLine($"Failed to fetch: {link} - Status: {linkResponse.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception or handle as needed
                    Console.WriteLine($"Error processing link {link}: {ex.Message}");
                }
            }

            return Ok(productDetails);
        }

        // Alternative approach with parallel processing for better performance
        //[HttpGet("seo-parallel")]
        //public async Task<IActionResult> GetSeoParallel(string url)
        //{
        //    var response = await _httpClient.GetAsync(url);
        //    if (!response.IsSuccessStatusCode)
        //        return StatusCode((int)response.StatusCode, "Xəta baş verdi.");

        //    string json = await response.Content.ReadAsStringAsync();
        //    var result = JsonConvert.DeserializeObject<ZaraRootSeo>(json);
        //    var seoList = result?.ProductGroups
        //        .SelectMany(pg => pg.Elements)
        //        .SelectMany(e => e.CommercialComponents)
        //        .Select(c => c.Seo);

        //    HashSet<string> links = new HashSet<string>();
        //    foreach (var item in seoList)
        //    {
        //        links.Add("https://www.zara.com/az/ru/" + item.Keyword + "-p" + item.SeoProductId + ".html?ajax=true");
        //    }

        //    // Process links in parallel with concurrency control
        //    var semaphore = new SemaphoreSlim(5, 5); // Limit to 5 concurrent requests
        //    var tasks = links.Select(async link =>
        //    {
        //        await semaphore.WaitAsync();
        //        try
        //        {
        //            var linkResponse = await _httpClient.GetAsync(link);
        //            if (linkResponse.IsSuccessStatusCode)
        //            {
        //                string linkJson = await linkResponse.Content.ReadAsStringAsync();
        //                return JsonConvert.DeserializeObject<Root>(linkJson);
        //            }
        //            return null;
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error processing link {link}: {ex.Message}");
        //            return null;
        //        }
        //        finally
        //        {
        //            semaphore.Release();
        //        }
        //    }).ToArray();

        //    var results = await Task.WhenAll(tasks);
        //    var productDetails = results.Where(r => r != null).ToList();

        //    return Ok(productDetails);
        //}

        // If you want to return both the links and the processed data
        //[HttpGet("seo-detailed")]
        //public async Task<IActionResult> GetSeoDetailed(string url)
        //{
        //    var response = await _httpClient.GetAsync(url);
        //    if (!response.IsSuccessStatusCode)
        //        return StatusCode((int)response.StatusCode, "Xəta baş verdi.");

        //    string json = await response.Content.ReadAsStringAsync();
        //    var result = JsonConvert.DeserializeObject<ZaraRootSeo>(json);
        //    var seoList = result?.ProductGroups
        //        .SelectMany(pg => pg.Elements)
        //        .SelectMany(e => e.CommercialComponents)
        //        .Select(c => c.Seo);

        //    HashSet<string> links = new HashSet<string>();
        //    foreach (var item in seoList)
        //    {
        //        links.Add("https://www.zara.com/az/ru/" + item.Keyword + "-p" + item.SeoProductId + ".html?ajax=true");
        //    }

        //    // Process each link and create a detailed response
        //    var detailedResults = new List<object>();

        //    foreach (var link in links)
        //    {
        //        try
        //        {
        //            var linkResponse = await _httpClient.GetAsync(link);
        //            if (linkResponse.IsSuccessStatusCode)
        //            {
        //                string linkJson = await linkResponse.Content.ReadAsStringAsync();
        //                var productDetail = JsonConvert.DeserializeObject<Root>(linkJson);

        //                detailedResults.Add(new
        //                {
        //                    Link = link,
        //                    Success = true,
        //                    Data = productDetail
        //                });
        //            }
        //            else
        //            {
        //                detailedResults.Add(new
        //                {
        //                    Link = link,
        //                    Success = false,
        //                    Error = $"HTTP {linkResponse.StatusCode}",
        //                    Data = (Root)null
        //                });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            detailedResults.Add(new
        //            {
        //                Link = link,
        //                Success = false,
        //                Error = ex.Message,
        //                Data = (Root)null
        //            });
        //        }
        //    }

        //    return Ok(new
        //    {
        //        TotalLinks = links.Count,
        //        SuccessfulRequests = detailedResults.Count(r => (bool)r.GetType().GetProperty("Success").GetValue(r)),
        //        Results = detailedResults
        //    });
        //}

        private string CreateLink(long id)
        {
            return $"https://www.zara.com/az/ru/category/{id}/products?ajax=true";
        }
        private async Task<List<CategoryWithSubcategories>> GetCategories()
        {
            string url = "https://www.zara.com/az/ru/categories?categoryId=2536906&categorySeoId=640&ajax=true";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception();
            }
            string json = await response.Content.ReadAsStringAsync();
            ZaraCategoryRoot data = JsonConvert.DeserializeObject<ZaraCategoryRoot>(json);
            var result = new List<CategoryWithSubcategories>();

            foreach (var cat in data.Categories)
            {
                if (cat.Subcategories != null && cat.Subcategories.Count > 0)
                {
                    var subList = new List<SubcategoryResponse>();

                    foreach (var sub in cat.Subcategories)
                    {
                        // Əgər subcategory-nin özünün subcategories-i varsa, ilk subcategory-nin ID-sini götür
                        long subcategoryId = sub.Id;
                        if (sub.Subcategories != null && sub.Subcategories.Count > 0)
                        {
                            subcategoryId = sub.Subcategories[0].Id; // İlk nested subcategory-nin ID-si
                        }

                        subList.Add(new SubcategoryResponse
                        {
                            SubcategoryId = subcategoryId,
                            Name = sub.Name,
                            link = CreateLink(subcategoryId)
                        });
                    }

                    result.Add(new CategoryWithSubcategories
                    {
                        SectionName = cat.SectionName,
                        Subcategories = subList,
                    });
                }
            }
            result = result
    .Where(x => x.SectionName != "TRAVEL" && x.SectionName != "ANNIVERSARY")
    .ToList();

            return result;
        }
    }
}

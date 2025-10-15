using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScrapperWebAPI.Helpers;
using ScrapperWebAPI.Helpers.Product;
using System.Threading.Tasks;

namespace ScrapperWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
       

        [HttpGet("CategoryIds")]
        public async Task<IActionResult> catid()
        {
            var categoryNames = await GetBreshkaCategories.GetAllCategoryLinks();

            var data = await GetBershkaProducts.FetchProductsAsync(categoryNames);

            return Ok(data);
        }


    }
}

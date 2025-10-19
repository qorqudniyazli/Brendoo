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
            var list = new List<int>();
            list.Add(2088776);
            var categoryNames = await GetStradivariusCategories.GetAllProductLinks();


            return Ok(categoryNames);
        }


    }
}

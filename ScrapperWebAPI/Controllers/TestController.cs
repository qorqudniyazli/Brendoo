using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScrapperWebAPI.Helpers;
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
            var data = await GetBreshkaCategories.GetAllCategoryLinks();
            return Ok(data);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Warehouse.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult Boom()
        {
            throw new Exception("TEST EXCEPTION - Middleware should catch this");

        }

    }
}

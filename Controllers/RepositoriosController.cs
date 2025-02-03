using Microsoft.AspNetCore.Mvc;

namespace TakeNetInterConnector.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RepositoriosController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API funcionando!");
        }
    }
}
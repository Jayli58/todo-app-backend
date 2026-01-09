using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Controllers
{
    [ApiController]
    public class HealthController : ControllerBase
    {
        [AllowAnonymous]
        [HttpGet("/health")]
        public IActionResult Health() => Ok("ok");
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MyApp.Controllers
{
    [Authorize]
    [Route("api/me")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // GET: api/me
        [HttpGet]
        public IActionResult GetUserInfo()
        {
            var user = HttpContext.User; // parsed token claims

            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var name = user.FindFirst("name")?.Value;
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;            

            return Ok(new { email, name, sub });
        }
    }
}

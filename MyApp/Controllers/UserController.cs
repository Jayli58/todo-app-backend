using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Services;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MyApp.Controllers
{
    [Authorize]
    [Route("me")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ICurrentUser _currentUser;
        public UserController(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        // GET: api/me
        [HttpGet]
        public IActionResult GetUserInfo()
        {
            var email = _currentUser.Email;
            var name = _currentUser.Name;
            var sub = _currentUser.UserId;            

            return Ok(new { email, name, sub });
        }
    }
}

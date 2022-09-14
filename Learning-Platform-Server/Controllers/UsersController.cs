using Learning_Platform_Server.Models;
using Learning_Platform_Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Learning_Platform_Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpPost("signin")]
        public ContentResult SignIn([FromBody] SignInRequest signInRequest)
        {
            return UserService.GetUser(signInRequest);
        }
    }
}

using Learning_Platform_Server.Models.Users;
using Learning_Platform_Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Learning_Platform_Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("signin")]
        public ActionResult<UserResponse> SignIn([FromBody] SignInRequest signInRequest)
        {
            UserResponse userResponse = _userService.SignInUser(signInRequest);

            return Ok(userResponse);
        }

        [HttpPost]
        public ActionResult Create([FromBody] CreateUserRequest createUserRequest)
        {
            _userService.Create(createUserRequest);
            return StatusCode(StatusCodes.Status201Created);
        }
    }
}

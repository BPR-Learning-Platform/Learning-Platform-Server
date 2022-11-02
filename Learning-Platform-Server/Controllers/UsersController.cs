using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Users;
using Learning_Platform_Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;

namespace Learning_Platform_Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICacheHandler _cacheHelper;

        public UsersController(IUserService userService, ICacheHandler cacheHelper)
        {
            _userService = userService;
            _cacheHelper = cacheHelper;
        }

        [HttpPost("signin")]
        public ActionResult<UserResponse> SignIn([FromBody] SignInRequest signInRequest)
        {
            UserResponse userResponse = _userService.SignInUser(signInRequest);

            _cacheHelper.EnsureCaching(userResponse);

            return Ok(userResponse);
        }

        [HttpPost]
        public OkResult Create([FromBody] CreateUserRequest createUserRequest)
        {
            _userService.Create(createUserRequest);

            return Ok();
        }
    }
}

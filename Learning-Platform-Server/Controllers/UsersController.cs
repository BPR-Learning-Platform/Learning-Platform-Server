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
        private readonly ICacheService _cacheHelper;

        private ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ICacheService cacheHelper, ILogger<UsersController> logger)
        {
            _userService = userService;
            _cacheHelper = cacheHelper;

            _logger = logger;
        }

        [HttpPost("signin")]
        public ContentResult SignIn([FromBody] SignInRequest signInRequest)
        {
            UserResponse userResponse = _userService.SignInUser(signInRequest);

            _cacheHelper.EnsureCaching(userResponse);

            return new ContentResult() { StatusCode = StatusCodes.Status200OK, Content = userResponse.ToJson() };
        }
    }
}

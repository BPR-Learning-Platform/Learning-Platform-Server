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
        private IMemoryCache _cache;
        private ILogger<UsersController> _logger;

        public UsersController(IUserService userService, IMemoryCache cache, ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("signin")]
        public ContentResult SignIn([FromBody] SignInRequest signInRequest)
        {
            KeyValuePair<ContentResult, UserResponse?> keyValuePair = _userService.SignInUser(signInRequest);

            UserResponse? userResponse = keyValuePair.Value;
            if (userResponse is null)
            {
                ContentResult? contentResult = keyValuePair.Key;
                return contentResult;
            }

            _logger.Log(LogLevel.Information, "Trying to fetch the list of users from cache.");

            if (_cache.TryGetValue(CacheHelper.UserListCacheKey, out IEnumerable<UserResponse> users))
            {
                _logger.Log(LogLevel.Information, "User list found in cache.");

                if (!users.Any(x => x.Email is not null && x.Email.Equals(userResponse.Email)))
                {
                    Console.WriteLine("User email " + userResponse.Email + " not found in user list");
                    CacheHelper.AddToCachedUserList(_cache, users, userResponse);
                }
            }
            else
            {
                _logger.Log(LogLevel.Information, "User list not found in cache.");
                CacheHelper.ResetCachedUserList(_cache, userResponse);
            }

            return new ContentResult() { StatusCode = StatusCodes.Status200OK, Content = userResponse.ToJson() };
        }

    }
}

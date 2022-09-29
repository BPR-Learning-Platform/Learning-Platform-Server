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
        private const string studentListCacheKey = "userList";
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

            if (keyValuePair.Value is null)
                return keyValuePair.Key;

            UserResponse userResponse = keyValuePair.Value;

            _logger.Log(LogLevel.Information, "Trying to fetch the list of users from cache.");

            if (_cache.TryGetValue(studentListCacheKey, out IEnumerable<UserResponse> users))
            {
                _logger.Log(LogLevel.Information, "User list found in cache.");

                if (users.FirstOrDefault(x => x.Email is not null && x.Email.Equals(userResponse.Email)) is null)
                {
                    Console.WriteLine("User email " + userResponse.Email + " not found in user list");

                    List<UserResponse> userListNew = users.ToList();
                    userListNew.Add(userResponse);

                    SetUserCache(userListNew);
                }

                users.ToList().ForEach(x => Console.WriteLine(x.ToString()));
            }
            else
            {
                _logger.Log(LogLevel.Information, "User list not found in cache.");

                List<UserResponse> userListNew = new()
                {
                    userResponse
                };

                SetUserCache(userListNew);
            }

            return new ContentResult() { StatusCode = StatusCodes.Status200OK, Content = userResponse.ToJson() };
        }



        // helper methods

        private void SetUserCache(List<UserResponse> newUserList)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(1024);
            _cache.Set(studentListCacheKey, newUserList, cacheEntryOptions);
        }
    }
}

using Learning_Platform_Server.Models.Users;
using Microsoft.Extensions.Caching.Memory;

namespace Learning_Platform_Server.Services
{
    public interface ICacheService
    {
        void AddToCachedUserList(IEnumerable<UserResponse> users, UserResponse userResponse);
        void ResetCachedUserList(UserResponse userResponse);
        UserResponse? GetUserResponseFromCache(string userId);
        void EnsureCaching(UserResponse userResponse);
    }

    public class CacheService : ICacheService
    {
        public const string UserListCacheKey = "userList";

        public readonly MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(60))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(3600))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(1024);

        private readonly IUserService _userService;

        private IMemoryCache _cache;
        private ILogger<CacheService> _logger;

        public CacheService(IUserService userService, IMemoryCache cache, ILogger<CacheService> logger)
        {
            _userService = userService;
            _cache = cache;
            _logger = logger;
        }

        public void AddToCachedUserList(IEnumerable<UserResponse> users, UserResponse userResponse)
        {
            Console.WriteLine("Adding user to cached userlist");
            users = users.Append(userResponse);
            users = _cache.Set(UserListCacheKey, users, CacheEntryOptions);
            users.ToList().ForEach(x => Console.WriteLine(x));
        }

        public void ResetCachedUserList(UserResponse userResponse)
        {
            Console.WriteLine("Resetting cached user list and adding user");
            List<UserResponse> newUserList = new() { userResponse };
            newUserList = _cache.Set(UserListCacheKey, newUserList, CacheEntryOptions);
            newUserList.ToList().ForEach(x => Console.WriteLine(x));
        }

        public UserResponse? GetUserResponseFromCache(string userId)
        {
            UserResponse? userResponse;

            if (_cache.TryGetValue(UserListCacheKey, out IEnumerable<UserResponse> users))
            {
                _logger.Log(LogLevel.Information, "User list found in cache.");
                userResponse = users.FirstOrDefault(x => x.UserId is not null && x.UserId.Equals(userId));

                if (userResponse is null)
                {
                    _logger.Log(LogLevel.Information, "User id not found in user list");

                    userResponse = _userService.GetById(userId);
                    if (userResponse is null)
                        return null;

                    Console.WriteLine("Requested user from database, since no cache entry was found for userid " + userId + ". " +
                        "This could be due to expiration of the cache entry, " +
                        "or it could be caused by a user requesting the tasks of another user (that has not been cached). " +
                        "The tasks where requested for userid " + userId + " " + "which is associated with the user: " + userResponse + ". ");

                    AddToCachedUserList(users, userResponse);
                }
            }
            else
            {
                _logger.Log(LogLevel.Information, "User list not found in cache.");

                userResponse = _userService.GetById(userId);
                if (userResponse is null)
                    return null;

                ResetCachedUserList(userResponse);
            }

            return userResponse;
        }

        public void EnsureCaching(UserResponse userResponse)
        {
            _logger.Log(LogLevel.Information, "Trying to fetch the list of users from cache.");

            if (_cache.TryGetValue(UserListCacheKey, out IEnumerable<UserResponse> users))
            {
                _logger.Log(LogLevel.Information, "User list found in cache.");

                if (!users.Any(x => x.Email is not null && x.Email.Equals(userResponse.Email)))
                {
                    Console.WriteLine("User email " + userResponse.Email + " not found in user list");
                    AddToCachedUserList(users, userResponse);
                }
            }
            else
            {
                _logger.Log(LogLevel.Information, "User list not found in cache.");
                ResetCachedUserList(userResponse);
            }
        }
    }
}

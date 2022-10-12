using Learning_Platform_Server.Models.Grades;
using Learning_Platform_Server.Models.Users;
using Learning_Platform_Server.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Learning_Platform_Server.Helpers
{
    public interface ICacheService
    {
        void AddToCachedUserList(IEnumerable<UserResponse> users, UserResponse userResponse);
        void ResetCachedUserList(UserResponse userResponse);
        UserResponse GetUserResponseFromCache(string userId);
        void EnsureCaching(UserResponse userResponse);
        List<GradeResponse> GetGradeResponseListFromCache();
    }

    public class CacheHandler : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IUserService _userService;
        private readonly IGradeService _gradeService;
        private readonly ILogger<CacheHandler> _logger;

        public const string UserListCacheKey = "userList";
        public const string GradeListCacheKey = "gradeList";

        public readonly MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(60))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(3600))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(1024);

        public CacheHandler(IUserService userService, IGradeService gradeService, IMemoryCache cache, ILogger<CacheHandler> logger)
        {
            _userService = userService;
            _gradeService = gradeService;
            _cache = cache;
            _logger = logger;
        }



        // USERS

        public void AddToCachedUserList(IEnumerable<UserResponse> users, UserResponse userResponse)
        {
            Console.WriteLine("Adding user to cached userlist");
            users = users.Append(userResponse);
            SetCachedUserList(users);
        }

        public void ResetCachedUserList(UserResponse userResponse)
        {
            Console.WriteLine("Resetting cached user list and adding user");
            List<UserResponse> userList = new() { userResponse };
            SetCachedUserList(userList);
        }

        private void SetCachedUserList(IEnumerable<UserResponse> users)
        {
            users = _cache.Set(UserListCacheKey, users, CacheEntryOptions);
            users.ToList().ForEach(x => Console.WriteLine(x));
        }

        public UserResponse GetUserResponseFromCache(string userId)
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
                        throw new KeyNotFoundException("Could not find user with id " + userId);

                    AddToCachedUserList(users, userResponse);
                }
            }
            else
            {
                _logger.Log(LogLevel.Information, "User list not found in cache.");

                userResponse = _userService.GetById(userId);
                if (userResponse is null)
                    throw new KeyNotFoundException("Could not find user with id " + userId);

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



        // GRADES

        public List<GradeResponse> GetGradeResponseListFromCache()
        {
            List<GradeResponse>? gradeResponseList;

            if (_cache.TryGetValue(GradeListCacheKey, out IEnumerable<GradeResponse> grades))
            {
                _logger.Log(LogLevel.Information, "Grade list found in cache.");
                gradeResponseList = grades.ToList();
            }
            else
            {
                _logger.Log(LogLevel.Information, "Grade list not found in cache.");

                gradeResponseList = _gradeService.GetAll();
                if (gradeResponseList is null)
                    throw new Exception("Could not find grade list");

                ResetCachedGradeList(gradeResponseList);
            }

            return gradeResponseList;
        }

        private void ResetCachedGradeList(List<GradeResponse> gradeResponseList)
        {
            Console.WriteLine("Resetting cached grade list");
            List<GradeResponse>? newGradeResponseList = _cache.Set(GradeListCacheKey, gradeResponseList, CacheEntryOptions);
            newGradeResponseList.ForEach(x => Console.WriteLine(x));
        }
    }
}

using Learning_Platform_Server.Models.Grades;
using Learning_Platform_Server.Models.Users;
using Learning_Platform_Server.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Learning_Platform_Server.Helpers
{
    public interface ICacheHandler
    {
        // USERS
        void AddToCachedUserList(IEnumerable<UserResponse> users, UserResponse userResponse);
        void ResetCachedUserList(UserResponse userResponse);
        UserResponse GetUserResponseFromCache(string userId);
        void EnsureCaching(UserResponse userResponse);

        // GRADES
        List<GradeResponse> GetGradeResponseListFromCache();
        int GetStep(string userid);
    }

    public class CacheHandler : ICacheHandler
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
                _logger.Log(LogLevel.Information, "Cached UserList found.");
                userResponse = users.FirstOrDefault(x => x.UserId is not null && x.UserId.Equals(userId));

                if (userResponse is null)
                {
                    _logger.Log(LogLevel.Information, "User id not found in Cached UserList. Calling UserService and adding user to cache.");

                    userResponse = _userService.GetById(userId);
                    if (userResponse is null)
                        throw new KeyNotFoundException("Could not find user with id " + userId);

                    AddToCachedUserList(users, userResponse);
                }
            }
            else
            {
                _logger.Log(LogLevel.Information, "Cached UserList not found. Calling UserService and resetting cache.");

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
                _logger.Log(LogLevel.Information, "Cached UserList found.");

                if (!users.Any(x => x.Email is not null && x.Email.Equals(userResponse.Email)))
                {
                    Console.WriteLine("User email " + userResponse.Email + " not found in user list");
                    AddToCachedUserList(users, userResponse);
                }
            }
            else
            {
                _logger.Log(LogLevel.Information, "Cached UserList not found. Resetting cache and adding user.");
                ResetCachedUserList(userResponse);
            }
        }



        // GRADES

        public List<GradeResponse> GetGradeResponseListFromCache()
        {
            List<GradeResponse>? gradeResponseList;

            if (_cache.TryGetValue(GradeListCacheKey, out IEnumerable<GradeResponse> grades))
            {
                _logger.Log(LogLevel.Information, "Cached GradeList found.");
                gradeResponseList = grades.ToList();
            }
            else
            {
                _logger.Log(LogLevel.Information, "Cached GradeList not found. Calling GradeService and resetting cache.");

                gradeResponseList = _gradeService.GetAll();
                if (gradeResponseList is null)
                    throw new Exception("Could not find grade list");

                ResetCachedGradeList(gradeResponseList);
            }

            return gradeResponseList;
        }

        private void ResetCachedGradeList(List<GradeResponse> gradeResponseList)
        {
            Console.WriteLine("Resetting cached GradeList");
            List<GradeResponse>? newGradeResponseList = _cache.Set(GradeListCacheKey, gradeResponseList, CacheEntryOptions);
            newGradeResponseList.ForEach(x => Console.WriteLine(x));
        }

        public int GetStep(string userid)
        {
            UserResponse userResponse = GetUserResponseFromCache(userid);

            if (userResponse.AssignedGradeIds is null || userResponse.AssignedGradeIds.Count == 0)
                throw new Exception("No assignedgradeids were found for user with userid " + userid);

            List<GradeResponse> gradeResponseList = GetGradeResponseListFromCache();

            int assignedGradeId = userResponse.AssignedGradeIds[0];

            GradeResponse? gradeResponse = gradeResponseList.FirstOrDefault(x => x.GradeId is not null && x.GradeId.Equals(assignedGradeId + ""));
            if (gradeResponse is null)
                throw new Exception("No grade was found for gradeid " + assignedGradeId);

            if (gradeResponse.Step is null)
                throw new NullReferenceException("No step was found for grade with gradeid " + assignedGradeId);

            return (int)gradeResponse.Step;
        }
    }
}

using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Users;
using Microsoft.Extensions.Caching.Memory;

namespace Learning_Platform_Server.Services
{
    public class UserServiceWithCache : IUserService
    {
        private readonly UserService _userService;
        private readonly IMemoryCache _cache;
        public const string UserListCacheKey = "userList";

        public readonly MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(60))
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(3600))
        .SetPriority(CacheItemPriority.Normal)
        .SetSize(1024);

        private readonly ILogger<UserServiceWithCache> _logger;

        public UserServiceWithCache(UserService userService, IMemoryCache cache, ILogger<UserServiceWithCache> logger)
        {
            _userService = userService;
            _cache = cache;
            _logger = logger;
        }

        public List<UserResponse> GetByGradeId(int gradeId)
            => _userService.GetByGradeId(gradeId);

        public UserResponse? GetById(string id)
            => _userService.GetById(id);

        public UserResponse SignInUser(SignInRequest signInRequest)
        {
            UserResponse userResponse = _userService.SignInUser(signInRequest);

            EnsureCaching(userResponse);

            return userResponse;
        }

        public void Create(CreateUserRequest createUserRequest)
            => _userService.Create(createUserRequest);

        public UserResponse UpdateUserScore(UserResponse userResponse, CorrectInfo correctInfo)
        {
            if (userResponse.Score is null)
                throw new Exception("Score was null");

            ScoreResponse previousScore = userResponse.Score;

            // calling service
            UserResponse updatedUserResponse = _userService.UpdateUserScore(userResponse, correctInfo);

            UpdateCachedUser(updatedUserResponse);
            Console.WriteLine("The cached user with id " + userResponse.UserId + " was updated from " + previousScore + " to " + updatedUserResponse.Score);

            return updatedUserResponse;
        }



        // helper methods

        public void UpdateCachedUser(UserResponse updatedUserResponse)
        {
            UserResponse? userResponse;

            if (_cache.TryGetValue(UserListCacheKey, out IEnumerable<UserResponse> users))
            {
                userResponse = users.FirstOrDefault(x => x.UserId is not null && x.UserId.Equals(updatedUserResponse.UserId));

                if (userResponse is null)
                    throw new Exception("User id not found in cached UserList.");

                userResponse.Score = updatedUserResponse.Score;
            }
            else
                throw new Exception("Cached UserList not found.");
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
                    _logger.Log(LogLevel.Information, "User id not found in cached UserList. Calling UserService and adding user to cache.");

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

        private void AddToCachedUserList(IEnumerable<UserResponse> users, UserResponse userResponse)
        {
            Console.WriteLine("Adding user to cached userlist");
            users = users.Append(userResponse);
            SetCachedUserList(users);
        }
        private void ResetCachedUserList(UserResponse userResponse)
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
    }
}

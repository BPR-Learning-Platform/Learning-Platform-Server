using Learning_Platform_Server.Models.Users;
using Microsoft.Extensions.Caching.Memory;

namespace Learning_Platform_Server.Controllers
{
    public class CacheHelper
    {
        public const string UserListCacheKey = "userList";

        public static readonly MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(60))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(3600))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(1024);

        public static void AddToCachedUserList(IMemoryCache cache, IEnumerable<UserResponse> users, UserResponse userResponse)
        {
            Console.WriteLine("Adding user to cached userlist");
            users = users.Append(userResponse);
            users = cache.Set(CacheHelper.UserListCacheKey, users, CacheHelper.CacheEntryOptions);
            users.ToList().ForEach(x => Console.WriteLine(x));
        }

        public static void ResetCachedUserList(IMemoryCache cache, UserResponse userResponse)
        {
            Console.WriteLine("Resetting cached user list and adding user");
            List<UserResponse> newUserList = new() { userResponse };
            newUserList = cache.Set(CacheHelper.UserListCacheKey, newUserList, CacheHelper.CacheEntryOptions);
            newUserList.ToList().ForEach(x => Console.WriteLine(x));
        }
    }
}

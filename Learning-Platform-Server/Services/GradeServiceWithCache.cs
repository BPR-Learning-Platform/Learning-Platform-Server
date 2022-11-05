using Learning_Platform_Server.Models.Grades;
using Learning_Platform_Server.Models.Users;
using Microsoft.Extensions.Caching.Memory;

namespace Learning_Platform_Server.Services
{
    public class GradeServiceWithCache : IGradeService
    {
        private readonly GradeService _gradeService;
        private readonly IMemoryCache _cache;
        public const string GradeListCacheKey = "gradeList";

        public readonly MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(60))
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(3600))
        .SetPriority(CacheItemPriority.Normal)
        .SetSize(1024);

        private readonly ILogger<GradeServiceWithCache> _logger;

        public GradeServiceWithCache(GradeService gradeService, IMemoryCache cache, ILogger<GradeServiceWithCache> logger)
        {
            _gradeService = gradeService;
            _cache = cache;
            _logger = logger;
        }

        public List<GradeResponse> GetAll()
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

                // calling service
                gradeResponseList = _gradeService.GetAll();

                if (gradeResponseList is null)
                    throw new Exception("Could not find grade list");

                ResetCachedGradeList(gradeResponseList);
            }

            return gradeResponseList;
        }

        public List<GradeResponseToTeacher> GetAllToTeacher(string teacherId)
            => _gradeService.GetAllToTeacher(teacherId);

        public GradeResponse? GetById(int id)
            => _gradeService.GetById(id);

        public int GetStep(UserResponse userResponse)
            => _gradeService.GetStep(userResponse);



        // helper methods

        private void ResetCachedGradeList(List<GradeResponse> gradeResponseList)
        {
            Console.WriteLine("Resetting cached GradeList");
            List<GradeResponse>? newGradeResponseList = _cache.Set(GradeListCacheKey, gradeResponseList, CacheEntryOptions);
            newGradeResponseList.ForEach(x => Console.WriteLine(x));
        }
    }
}

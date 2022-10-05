using Learning_Platform_Server.Models.Grades;
using Learning_Platform_Server.Models.Tasks;
using Learning_Platform_Server.Models.Users;
using Learning_Platform_Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace Learning_Platform_Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;
        private readonly IGradeService _gradeService;

        private IMemoryCache _cache;
        private ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, IUserService userService, IGradeService gradeService, IMemoryCache cache, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _userService = userService;
            _gradeService = gradeService;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TaskResponse>> GetBatch([FromQuery] string userid, [FromQuery] int correct)
        {
            UserResponse? userResponse;

            if (_cache.TryGetValue(CacheHelper.UserListCacheKey, out IEnumerable<UserResponse> users))
            {
                _logger.Log(LogLevel.Information, "User list found in cache.");
                userResponse = users.FirstOrDefault(x => x.UserId is not null && x.UserId.Equals(userid));

                if (userResponse is null)
                {
                    _logger.Log(LogLevel.Information, "User id not found in user list");

                    userResponse = _userService.GetById(userid);
                    if (userResponse is null)
                        return StatusCode(StatusCodes.Status404NotFound, "No user was found with userid " + userid);

                    Console.WriteLine("Requesting user from database, since no cache entry was found for userid " + userid + ". " +
                        "This could be due to expiration of the cache entry, " +
                        "or it could be caused by a user requesting the tasks of another user (that has not been cached). " +
                        "The tasks where requested for userid " + userid + " " + "which is associated with the user: " + userResponse + ". ");

                    CacheHelper.AddToCachedUserList(_cache, users, userResponse);


                }
            }
            else
            {
                _logger.Log(LogLevel.Information, "User list not found in cache.");

                userResponse = _userService.GetById(userid);
                if (userResponse is null)
                    return StatusCode(StatusCodes.Status404NotFound, "No user was found with userid " + userid);

                CacheHelper.ResetCachedUserList(_cache, userResponse);
            }

            if (userResponse.AssignedGradeIds is null || userResponse.AssignedGradeIds.Count == 0)
                return StatusCode(StatusCodes.Status500InternalServerError, "No assignedgradeids were found for user with userid " + userid);




            // TEMPORARY VERSION //TODO: add caching of grades

            List<GradeResponse>? gradeResponseList = _gradeService.GetAll();
            if (gradeResponseList is null || gradeResponseList.Count == 0)
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to get grades from database");

            int assignedGradeId = userResponse.AssignedGradeIds[0];

            GradeResponse? gradeResponse = gradeResponseList.FirstOrDefault(x => x.GradeId is not null && x.GradeId.Equals(assignedGradeId + ""));
            if (gradeResponse is null)
                return StatusCode(StatusCodes.Status500InternalServerError, "No grade was found for gradeid " + assignedGradeId);

            if (gradeResponse.Step is null)
                return StatusCode(StatusCodes.Status500InternalServerError, "No step was found for grade with gradeid " + assignedGradeId);





            List<TaskResponse>? taskResponseList = _taskService.GetAll((int)gradeResponse.Step);
            return Ok(taskResponseList);
        }
    }
}

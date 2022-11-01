using Learning_Platform_Server.Helpers;
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
        private readonly ICacheHandler _cacheHelper;

        public TasksController(ITaskService taskService, IUserService userService, ICacheHandler cacheHelper)
        {
            _taskService = taskService;
            _userService = userService;
            _cacheHelper = cacheHelper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TaskResponse>> GetBatch([FromQuery] string userid, [FromQuery] int correct)
        {
            int step = _cacheHelper.GetStep(userid);
            List<TaskResponse> taskResponseList = _taskService.GetAll(step);

            List<TaskResponse> taskResponseBatchList = new();

            Random random = new();
            for (int i = 0; i < Util.BatchSize; i++)
            {
                taskResponseBatchList.Add(taskResponseList[random.Next(0, taskResponseList.Count)]);
            }

            UserResponse userResponse = _cacheHelper.GetUserResponseFromCache(userid);

            // updating the users score asynchronously each time a new batch request is received
            Task.Run(() => UpdateUserScore(userResponse, correct));

            // for debugging
            //Console.WriteLine("taskResponseBatchList: \n\t" + (string.Join(",\n\t", taskResponseBatchList))); // expected to complete before "UpdateUserScore" has completed

            return Ok(taskResponseBatchList);
        }

        private void UpdateUserScore(UserResponse userResponse, int correct)
        {
            float? previousScore = (float?)userResponse.Score;
            UserResponse updatedUserResponse = _userService.UpdateUserScore(userResponse, correct);

            _cacheHelper.UpdateCachedUser(updatedUserResponse);
            Console.WriteLine("The cached user with id " + userResponse.UserId + " was updated from " + previousScore + " to " + updatedUserResponse.Score);
        }
    }
}

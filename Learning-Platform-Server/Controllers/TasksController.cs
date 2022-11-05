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
        private readonly IGradeService _gradeService;

        public TasksController(ITaskService taskService, IUserService userService, IGradeService gradeService)
        {
            _taskService = taskService;
            _userService = userService;
            _gradeService = gradeService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TaskResponse>> GetBatch([FromQuery] string userid, [FromQuery] int correct, [FromQuery] string? taskids)
        {
            // The query parameter for task ids looks like this: "taskids=58,7,42"
            List<string> previousTaskIds = taskids is not null ? taskids.Split(',').ToList() : new();

            List<TaskResponse> taskResponseBatchList = _taskService.GetBatch(userid, correct, previousTaskIds);

            return Ok(taskResponseBatchList);
        }
    }
}

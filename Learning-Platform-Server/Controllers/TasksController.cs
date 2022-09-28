using Learning_Platform_Server.Models.Grades;
using Learning_Platform_Server.Models.Tasks;
using Learning_Platform_Server.Models.Users;
using Learning_Platform_Server.Services;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult<IEnumerable<TaskResponse>> GetBatch([FromQuery] string userid, [FromQuery] int correct)
        {
            List<TaskResponse>? taskResponseList = _taskService.GetAll(1); // TODO: remove hardcode

            return Ok(taskResponseList);
        }
    }
}

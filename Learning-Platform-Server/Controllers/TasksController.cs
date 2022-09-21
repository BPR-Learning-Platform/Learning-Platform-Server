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

        public TasksController(ITaskService taskService, IUserService userService)
        {
            _taskService = taskService;
            _userService = userService;
        }

        [HttpGet("{userid:int}")]
        public ActionResult<IEnumerable<TaskResponse>> GetAll(int userid)
        {
            // TODO: UserModel userModel = _userService.GetById(userid);

            // TODO: GradeModel gradeModel = _gradeService.GetById(userModel.AssignedGradeIdList[0]);
            // TODO: IEnumerable<TaskResponse> tasks = _taskService.GetAll(gradeModel.Step);
            // return Ok(tasks);

            List<TaskResponse> taskResponseList = _taskService.GetAll(1); // TODO: remove hardcode

            return Ok(taskResponseList);
        }
    }
}

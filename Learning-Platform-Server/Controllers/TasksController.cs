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
        public ActionResult<IEnumerable<TaskResponse>> GetAll([FromQuery] string userid)
        {

            // TODO // Test when database endpoints are ready
            /*
            // USER
            UserResponse? userResponse = _userService.GetById(userid);
            if (userResponse is null)
                return StatusCode(StatusCodes.Status404NotFound, "No user was found with userid " + userid);

            // GRADE
            List<int>? gradeIdList = userResponse.AssignedGradeIds;
            if (gradeIdList is null)
                return StatusCode(StatusCodes.Status500InternalServerError, "No assignedgradeids were found for user with userid " + userid);

            int gradeId = gradeIdList[0];
            GradeResponse? gradeResponse = _gradeService.GetById(gradeId);

            if (gradeResponse is null)
                return StatusCode(StatusCodes.Status500InternalServerError, "No grade was found for gradeid " + gradeId);

            // STEP
            if (gradeResponse.Step is null)
                return StatusCode(StatusCodes.Status500InternalServerError, "No step was found for grade with gradeid " + gradeId);

            // TASK
            int step = (int)gradeResponse.Step;
            List<TaskResponse>? taskResponseList = _taskService.GetAll(step);

            if (taskResponseList is null)
                return StatusCode(StatusCodes.Status500InternalServerError, "No tasks were found for step " + step);
            */

            List<TaskResponse>? taskResponseList = _taskService.GetAll(1); // TODO: remove hardcode

            return Ok(taskResponseList);
        }
    }
}

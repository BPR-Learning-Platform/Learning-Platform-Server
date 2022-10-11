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
        private readonly IGradeService _gradeService;
        private readonly ICacheService _cacheHelper;

        private ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, IGradeService gradeService, ICacheService cacheHelper, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _gradeService = gradeService;
            _cacheHelper = cacheHelper;

            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TaskResponse>> GetBatch([FromQuery] string userid, [FromQuery] int correct)
        {
            UserResponse? userResponse = _cacheHelper.GetUserResponseFromCache(userid);
            if (userResponse is null)
                return StatusCode(StatusCodes.Status404NotFound, "No user was found with userid " + userid);

            if (userResponse.AssignedGradeIds is null || userResponse.AssignedGradeIds.Count == 0)
                return StatusCode(StatusCodes.Status500InternalServerError, "No assignedgradeids were found for user with userid " + userid);



            List<GradeResponse>? gradeResponseList = _cacheHelper.GetGradeResponseListFromCache();

            if (gradeResponseList is null || gradeResponseList.Count == 0)
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to get grades");

            int assignedGradeId = userResponse.AssignedGradeIds[0];

            GradeResponse? gradeResponse = gradeResponseList.FirstOrDefault(x => x.GradeId is not null && x.GradeId.Equals(assignedGradeId + ""));
            if (gradeResponse is null)
                return StatusCode(StatusCodes.Status500InternalServerError, "No grade was found for gradeid " + assignedGradeId);

            if (gradeResponse.Step is null)
                return StatusCode(StatusCodes.Status500InternalServerError, "No step was found for grade with gradeid " + assignedGradeId);





            List<TaskResponse>? taskResponseList = _taskService.GetAll((int)gradeResponse.Step);

            List<TaskResponse> taskResponseBatchList = new();

            Random random = new();

            if (taskResponseList is not null)
            {
                if (taskResponseList.Count == 0)
                    throw new Exception("Unable to read any tasks from task collection for step number " + gradeResponse.Step + " in the database.");

                for (int i = 0; i < 3; i++)
                {
                    taskResponseBatchList.Add(taskResponseList[random.Next(0, taskResponseList.Count())]);
                }
            }

            return Ok(taskResponseBatchList);
        }
    }
}

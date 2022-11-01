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
        private readonly ICacheHandler _cacheHelper;

        public TasksController(ITaskService taskService, ICacheHandler cacheHelper)
        {
            _taskService = taskService;
            _cacheHelper = cacheHelper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TaskResponse>> GetBatch([FromQuery] string userid, [FromQuery] int correct)
        {
            int step = _cacheHelper.GetStep(userid);
            List<TaskResponse> taskResponseList = _taskService.GetAll(step);

            List<TaskResponse> taskResponseBatchList = new();

            Random random = new();
            for (int i = 0; i < 3; i++)
            {
                taskResponseBatchList.Add(taskResponseList[random.Next(0, taskResponseList.Count)]);
            }

            return Ok(taskResponseBatchList);
        }
    }
}

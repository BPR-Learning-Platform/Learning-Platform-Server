using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Grades;
using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Tasks;
using Learning_Platform_Server.Models.Users;
using Learning_Platform_Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Net;
using System.Web;

namespace Learning_Platform_Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TaskResponse>> GetBatch([FromQuery] string userid, [FromQuery] string correct, [FromQuery] string? taskids)
        {
            // The query parameter for task ids looks like this: "taskids=58,7,42"
            List<string> previousTaskIds = taskids is not null ? taskids.Split(',').ToList() : new();

            StringWriter stringWriter = new();

            //The query parameter for correct looks like this: "correct={%22A%22:{%22count%22:0,%22percentage%22:null},%22M%22:{%22count%22:0,%22percentage%22:null},%22S%22:{%22count%22:0,%22percentage%22:null},%22D%22:{%22count%22:0,%22percentage%22:null}}"
            HttpUtility.HtmlDecode(correct, stringWriter);

            string decodedString = stringWriter.ToString();
            Console.WriteLine("Decoded correct string: " + decodedString);

            CorrectInfo? correctInfo = JsonConvert.DeserializeObject<CorrectInfo>(decodedString);

            Console.Write("Deserialized CorrectInfo: " + correctInfo);

            if (correctInfo is null)
                throw new Exception("Could not read the query parameter named 'correct'");

            List<TaskResponse> taskResponseBatchList = _taskService.GetBatch(userid, correctInfo, previousTaskIds);

            return Ok(taskResponseBatchList);
        }
    }
}

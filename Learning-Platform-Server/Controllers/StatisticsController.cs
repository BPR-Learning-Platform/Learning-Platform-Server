using Learning_Platform_Server.Models.Statistics;
using Learning_Platform_Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace Learning_Platform_Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticService _statisticService;

        public StatisticsController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<StatisticResponse>> GetAll([FromQuery] string? studentid, string? gradeId)
        {
            List<StatisticResponse> statisticResponseList = new();
            if (studentid is not null)
                statisticResponseList = _statisticService.GetAllByStudentId(int.Parse(studentid));
            else if (gradeId is not null)
                statisticResponseList = _statisticService.GetAllByGradeId(int.Parse(gradeId));

            //For debugging
            Console.WriteLine("This will be returned to frontend: " + string.Join(",\n\t", statisticResponseList));

            return Ok(statisticResponseList);
        }
    }
}
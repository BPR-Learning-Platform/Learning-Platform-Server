using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Statistics;
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
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticService _statisticService;
        private ILogger<StatisticsController> _logger;

        public StatisticsController(IStatisticService statisticService, ILogger<StatisticsController> logger)
        {
            _statisticService = statisticService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<StatisticResponse>> GetAll([FromQuery] string? studentid, string? gradeId)
        {
            List<StatisticResponse> statisticResponseList = new();
            if (studentid is not null)
                statisticResponseList = _statisticService.GetAllByStudentId(int.Parse(studentid));
            else if (gradeId is not null)
                statisticResponseList = _statisticService.GetAllByGradeId(int.Parse(gradeId));

            return Ok(statisticResponseList);
        }
    }
}
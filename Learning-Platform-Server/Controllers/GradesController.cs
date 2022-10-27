using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Grades;
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
    public class GradesController : ControllerBase
    {
        private readonly IGradeService _gradeService;
        private ILogger<GradesController> _logger;

        public GradesController(IGradeService gradeService, ILogger<GradesController> logger)
        {
            _gradeService = gradeService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<StatisticResponse>> GetAllToTeacher([FromQuery] string teacherid)
        {
            List<GradeResponseToTeacher>? gradeResponsesToTeacher = _gradeService.GetAllToTeacher(teacherid);
            return Ok(gradeResponsesToTeacher);
        }
    }
}
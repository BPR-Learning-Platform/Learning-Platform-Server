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

        public GradesController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<StatisticResponse>> GetAllToTeacher([FromQuery] string teacherid)
        {
            List<GradeResponseToTeacher>? gradeResponsesToTeacher = _gradeService.GetAllToTeacher(teacherid);
            return Ok(gradeResponsesToTeacher);
        }
    }
}
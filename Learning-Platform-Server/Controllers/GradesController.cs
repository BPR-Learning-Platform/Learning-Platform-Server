using Learning_Platform_Server.Models.Grades;
using Learning_Platform_Server.Services;
using Microsoft.AspNetCore.Mvc;

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
        public ActionResult<IEnumerable<GradeResponse>> GetAllToTeacher([FromQuery] string? teacherid)
        {
            if (teacherid is not null)
            {
                List<GradeResponseToTeacher> gradeResponsesToTeacher = _gradeService.GetAllToTeacher(teacherid);
                return Ok(gradeResponsesToTeacher);
            }
            else
            {
                List<GradeResponse> gradeResponses = _gradeService.GetAll();

                List<GradeResponseToTeacher> gradeResponsesToTeacher = new();
                gradeResponses.ForEach(x => gradeResponsesToTeacher.Add(new GradeResponseToTeacher() { GradeId = x.GradeId, GradeName = x.GradeName, Students = null }));

                return Ok(gradeResponsesToTeacher);
            }
        }
    }
}
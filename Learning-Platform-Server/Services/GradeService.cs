using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Models.Grades;
using Learning_Platform_Server.Models.Users;

namespace Learning_Platform_Server.Services
{
    public interface IGradeService
    {
        List<GradeResponse> GetAll();
        List<GradeResponseToTeacher> GetAllToTeacher(string teacherId);
        GradeResponse GetById(int id);
        int GetStep(UserResponse userResponse);
    }

    public class GradeService : IGradeService
    {
        private readonly IGradeDAO _gradeDAO;
        private readonly IUserService _userService;

        public GradeService(IGradeDAO gradeDAO, IUserService userService)
        {
            _gradeDAO = gradeDAO;
            _userService = userService;
        }

        public List<GradeResponse> GetAll()
            => _gradeDAO.GetAll();

        public List<GradeResponseToTeacher> GetAllToTeacher(string teacherId)
        {
            UserResponse teacher = _userService.GetById(teacherId);

            if (teacher.Type is null)
                throw new NullReferenceException("No type was found for user with id " + teacherId + ", so could not determine if the user is really a teacher. Details: " + teacher);

            if (!teacher.Type.Equals("T"))
                throw new BadHttpRequestException("The user with id " + teacherId + " has a type that does not represent a teacher. Details: " + teacher);

            if (teacher.AssignedGradeIds is null)
                throw new NullReferenceException("No assigned grade ids were found for the teacher: " + teacher);

            List<GradeResponse> gradeResponses = GetAll();

            List<GradeResponseToTeacher> gradeResponsesToTeacher = new();

            foreach (int gradeId in teacher.AssignedGradeIds)
            {
                GradeResponse? gradeResponse = gradeResponses.FirstOrDefault(x => x.GradeId is not null && x.GradeId.Equals(gradeId + ""));

                if (gradeResponse is null)
                    throw new KeyNotFoundException("Could not find grade with grade id " + gradeId);

                string? gradeName = gradeResponse.GradeName;

                List<UserResponse> userResponseList = _userService.GetByGradeId(gradeId);
                List<UserResponseToTeacher> studentsToTeacher = new();
                foreach (UserResponse user in userResponseList)
                {
                    if (user.Type is null)
                    {
                        Console.WriteLine("Could not read type from user: " + user + ", so the user is not included.");
                        break;
                    }

                    if (user.Type.Equals("S"))
                        studentsToTeacher.Add(new UserResponseToTeacher() { UserId = user.UserId, Name = user.Name });
                }


                gradeResponsesToTeacher.Add(new GradeResponseToTeacher() { GradeId = gradeId + "", GradeName = gradeName, Students = studentsToTeacher });
            }

            return gradeResponsesToTeacher;
        }

        public GradeResponse GetById(int id)
            => _gradeDAO.GetById(id);



        // helper methods

        public int GetStep(UserResponse userResponse)
        {
            if (userResponse.AssignedGradeIds is null || userResponse.AssignedGradeIds.Count == 0)
                throw new Exception("No assignedgradeids were found for user with userid " + userResponse.UserId);

            // calling DAO
            List<GradeResponse> gradeResponseList = _gradeDAO.GetAll();

            int assignedGradeId = userResponse.AssignedGradeIds[0];

            GradeResponse? gradeResponse = gradeResponseList.FirstOrDefault(x => x.GradeId is not null && x.GradeId.Equals(assignedGradeId + ""));
            if (gradeResponse is null)
                throw new Exception("No grade was found for gradeid " + assignedGradeId);

            if (gradeResponse.Step is null)
                throw new NullReferenceException("No step was found for grade with gradeid " + assignedGradeId);

            return (int)gradeResponse.Step;
        }
    }
}

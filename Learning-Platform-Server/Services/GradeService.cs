using Learning_Platform_Server.Daos;
using Learning_Platform_Server.Helpers.CustomExceptions;
using Learning_Platform_Server.Models.Grades;
using Learning_Platform_Server.Models.Users;

namespace Learning_Platform_Server.Services
{
    public interface IGradeService
    {
        List<GradeResponse> GetAll();
        List<GradeResponseToTeacher> GetAllToTeacher(string teacherId);
        GradeResponse GetById(int id);
        int GetStep(UserResponse student);
    }

    public class GradeService : IGradeService
    {
        private readonly IGradeDao _gradeDao;
        private readonly IUserService _userService;

        public GradeService(IGradeDao gradeDao, IUserService userService)
        {
            _gradeDao = gradeDao;
            _userService = userService;
        }

        public List<GradeResponse> GetAll()
            => _gradeDao.GetAll();

        public List<GradeResponseToTeacher> GetAllToTeacher(string teacherId)
        {
            UserResponse teacher = _userService.GetById(teacherId);

            if (teacher.Type != UserType.T)
                throw new BadHttpRequestException("The user with id " + teacherId + " has a type that does not represent a teacher. Details: " + teacher);

            // if no grades have been assigned to the teacher, return an empty list
            if (teacher.AssignedGradeIds is null)
                return new();

            List<GradeResponse> gradeResponses = GetAll();

            List<GradeResponseToTeacher> gradeResponsesToTeacher = new();
            foreach (int gradeId in teacher.AssignedGradeIds)
            {
                GradeResponse? gradeResponse = gradeResponses.FirstOrDefault(x => x.GradeId is not null && x.GradeId.Equals(gradeId + ""));

                if (gradeResponse is null)
                    throw new KeyNotFoundException("Could not find grade with grade id " + gradeId);

                string? gradeName = gradeResponse.GradeName;
                int? step = gradeResponse.Step;

                List<UserResponse> userResponseList = _userService.GetByGradeId(gradeId);
                List<UserResponseToTeacher> studentsToTeacher = new();
                foreach (UserResponse user in userResponseList)
                {
                    if (user.Type == UserType.S)
                        studentsToTeacher.Add(new UserResponseToTeacher() { UserId = user.UserId, Name = user.Name });
                }

                gradeResponsesToTeacher.Add(new GradeResponseToTeacher() { GradeId = gradeId + "", GradeName = gradeName, Step = step, Students = studentsToTeacher });
            }

            return gradeResponsesToTeacher;
        }

        public GradeResponse GetById(int id)
            => _gradeDao.GetById(id);



        // helper methods

        public int GetStep(UserResponse student)
        {
            if (student.AssignedGradeIds is null || student.AssignedGradeIds.Count == 0)
                throw new BusinessException("No assignedgradeids were found for user with userid " + student.UserId);

            // calling Dao
            List<GradeResponse> gradeResponseList = _gradeDao.GetAll();

            int assignedGradeId = student.AssignedGradeIds[0];

            GradeResponse? gradeResponse = gradeResponseList.FirstOrDefault(x => x.GradeId is not null && x.GradeId.Equals(assignedGradeId + ""));
            if (gradeResponse is null)
                throw new BusinessException("No grade was found for gradeid " + assignedGradeId);

            return gradeResponse.Step ?? throw new BusinessException($"No step was found for the students grade. Details: {gradeResponse}");
        }
    }
}

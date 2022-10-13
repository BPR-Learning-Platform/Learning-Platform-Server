using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Models.Users;

namespace Learning_Platform_Server.Models.Grades
{
    public class GradeResponse
    {
        public string? GradeId { get; set; }
        public int? Step { get; set; }
        public string? Name { get; set; }

        public override string ToString()
        {
            return "GradeResponse: gradeid: " + GradeId +
                ", step: " + Step +
                ", name: " + Name;
        }
    }

    public class GradeResponseToTeacher
    {
        public string? GradeId { get; set; }
        public string? GradeName { get; set; }
        public List<UserResponseToTeacher>? Students { get; set; }

        public override string ToString()
        {
            return "GradeResponse: gradeid: " + GradeId;
        }
    }
}

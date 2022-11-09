using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Models.Users;
using System.Text.Json.Serialization;

namespace Learning_Platform_Server.Models.Grades
{
    public class GradeResponse
    {
        public string? GradeId { get; set; }
        public int? Step { get; set; }
        public string? GradeName { get; set; }

        public override string ToString()
        {
            return "GradeResponse: gradeid: " + GradeId +
                ", step: " + Step +
                ", name: " + GradeName;
        }
    }

    public class GradeResponseToTeacher
    {
        public string? GradeId { get; set; }
        public string? GradeName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<UserResponseToTeacher>? Students { get; set; }

        public override string ToString()
        {
            return "GradeResponse: gradeid: " + GradeId +
                ", name: " + GradeName +
                ",\n\t" + "students: \n\t" + (Students is not null ? string.Join(",\n\t", Students) : "");
        }
    }
}

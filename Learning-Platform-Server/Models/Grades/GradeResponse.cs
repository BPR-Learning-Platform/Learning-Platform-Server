using Learning_Platform_Server.Entities;

namespace Learning_Platform_Server.Models.Grades
{
    public class GradeResponse
    {
        public string? GradeId { get; set; }
        public int? Step { get; set; }
        public string? Name { get; set; }

        public override string ToString()
        {
            return "MongoDbGrade: gradeid: " + GradeId +
                ", step: " + Step +
                ", name: " + Name;
        }
    }
}

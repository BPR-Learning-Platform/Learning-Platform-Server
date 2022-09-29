using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Learning_Platform_Server.Entities
{
    public class GradeRoot
    {
        [JsonProperty("_id")]
        public Id? Id { get; set; }
        public MongoDbGrade? Grade { get; set; }
    }

    public class MongoDbGrade
    {
        public string? GradeID { get; set; }
        public string? Step { get; set; }
        public string? GradeName { get; set; }

        public override string ToString()
        {
            return "MongoDbTask: gradeid: " + GradeID +
                ", step: " + Step +
                ", gradeName: " + GradeName;
        }
    }
}


using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Learning_Platform_Server.Entities
{
    public class MongoDbGradeRoot
    {
        [JsonProperty("_id")]
        public Id? Id { get; set; }
        public MongoDbGrade? Grade { get; set; }
        public GradeId? GradeId { get; set; }
        public override string ToString() => "MongoDbGradeRoot: Oid: " + Id + ", " + Grade + ", GradeId: " + GradeId;
    }
    public class GradeId
    {
        [JsonProperty("$numberLong")]
        public string? NumberLong { get; set; }
        public override string ToString() => NumberLong + "";
    }

    public class MongoDbGrade
    {
        public string? Step { get; set; }
        public string? GradeName { get; set; }

        public override string ToString()
        {
            return "MongoDbTask: step: " + Step +
                ", gradeName: " + GradeName;
        }
    }
}


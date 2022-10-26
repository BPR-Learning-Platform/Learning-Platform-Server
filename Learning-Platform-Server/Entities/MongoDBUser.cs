using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Learning_Platform_Server.Entities
{
    public class MongoDbUserRoot
    {
        [JsonProperty("_id")]
        public Id? Id { get; set; }
        public MongoDbUser? User { get; set; }
        public UserId? UserId { get; set; }
        public override string ToString() => "MongoDbUserRoot: Oid: " + Id + ", " + User + ", UserId: " + UserId;
    }

    public class Id
    {
        [JsonProperty("$oid")]
        public string? Oid { get; set; }
        public override string ToString() => Oid + "";
    }

    public class UserId
    {
        [JsonProperty("$numberLong")]
        public string? NumberLong { get; set; }
        public override string ToString() => NumberLong + "";
    }

    public class MongoDbUser
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? Score { get; set; }
        public List<int>? AssignedGradeIds;

        public override string ToString()
        {
            return "MongoDBUser: type: " + Type +
                ", name: " + Name +
                ", email: " + Email +
                ", score: " + Score +
                ", assignedGradeIds: " + (AssignedGradeIds is not null ? string.Join(",", AssignedGradeIds) : "");

        }
    }
}


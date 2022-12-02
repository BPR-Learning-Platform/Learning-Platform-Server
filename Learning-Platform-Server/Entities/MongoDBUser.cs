using Newtonsoft.Json;
using System.Text.Json.Serialization;

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

        private string? _email;
        public string? Email
        {
            get { return _email; }
            set { _email = ("" + value).ToLower(); }
        }
        public string? Password { get; set; }
        public MongoDbScore? Score { get; set; }
        [JsonPropertyName("assignedgradeids")]
        public List<int>? AssignedGradeIds { get; set; }

        public override string ToString()
        {
            return "MongoDbUser: type: " + Type +
                ", name: " + Name +
                ", email: " + Email +
                ", password: " + Password +
                ", " + Score +
                ",\n\t" + "assignedGradeIds: \n\t" + (AssignedGradeIds is not null ? string.Join(",\n\t", AssignedGradeIds) : "");

        }
    }
}


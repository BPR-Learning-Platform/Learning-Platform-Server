using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Learning_Platform_Server.Entities
{
    public class UserRoot
    {
        public Id? Id { get; set; }
        public MongoDBUser? User { get; set; }
    }

    public class Id
    {
        [JsonProperty("$oid")]
        public string? Oid { get; set; }
    }

    public class MongoDBUser
    {
        public string? UserId { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? Score { get; set; }
        public List<int>? AssignedGradeIds;

        public override string ToString()
        {
            return "MongoDBUser: userid: " + UserId +
                "\t, type: " + Type +
                "\t, name: " + Name +
                "\t, email: " + Email +
                "\t, password: " + Password +
                "\t, score: " + Score;
            //"\t, assignedGradeIds: " + AssignedGradeIds;

        }
    }
}


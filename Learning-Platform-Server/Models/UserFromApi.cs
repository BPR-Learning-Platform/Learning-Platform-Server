using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Learning_Platform_Server.Models
{
    public class Id
    {
        [JsonProperty("$oid")]
        public string? Oid { get; set; }
    }

    public class UserRoot
    {
        public Id? Id { get; set; }

        public Timestamp? TimeStamp { get; set; }

        public User? User { get; set; }
    }

    public class Timestamp
    {
        [JsonProperty("$date")]
        public long Date { get; set; }
    }

    public class User
    {
        public List<int>? assignedGradeIdList;
        public int? Score { get; set; }

        public string? Type { get; set; }
        public string? Email { get; set; }

        public string? Name { get; set; }
        public string? Password { get; set; }


        public string? UserId { get; set; }

        public override string ToString()
        {
            return "UserId: " + UserId + "\t pw: " + Password;
        }
    }

    public class MongoDBUser
    {
        public string? Name { get; set; }

        //public string userId { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; }
        public List<int>? AssignedGradeIds { get; set; }
        public string? Type { get; set; }
        public int? Score { get; set; }


        public MongoDBUser(User user)
        {
            //this.userId = user.userId;
            this.Name = user.Name;
            this.Email = user.Email;
            this.Password = user.Password;
            this.AssignedGradeIds = user.assignedGradeIdList;
            this.Type = user.Type;
            this.Score = (int)user.Score;
        }

        public override string ToString()
        {
            return "name: " + Name +
                ", \t pw: " + Password +
                ", \t assignedGradeIds: " + AssignedGradeIds +
                ", \t type: " + Type +
                ", \t score: " + Score;
        }
    }

    public class SignInRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}


using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Learning_Platform_Server.Models
{
    public class Id
    {
        [JsonProperty("$oid")]
        public string Oid { get; set; }
    }

    public class UserRoot
    {
        public Id _id { get; set; }

        public Timestamp? timeStamp { get; set; }

        public User User { get; set; }
    }

    public class Timestamp
    {
        [JsonProperty("$date")]
        public long Date { get; set; }
    }

    public class User
    {
        public List<int>? assignedGradeIdList;
        public int? score { get; set; }

        public string? type { get; set; }
        public string email { get; set; }

        public string? name { get; set; }
        public string password { get; set; }


        public string? userId { get; set; }

        public override string ToString()
        {
            return "UserId: " + userId + "\t pw: " + password;
        }
    }

    public class MongoDBUser
    {
        public string name { get; set; }

        //public string userId { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public List<int> assignedGradeIds { get; set; }
        public string type { get; set; }
        public int score { get; set; }


        public MongoDBUser(User user)
        {
            //this.userId = user.userId;
            this.name = user.name;
            this.email = user.email;
            this.password = user.password;
            this.assignedGradeIds = user.assignedGradeIdList;
            this.type = user.type;
            this.score = (int)user.score;
        }

        public override string ToString()
        {
            return "name: " + name +
                ", \t pw: " + password +
                ", \t assignedGradeIds: " + assignedGradeIds +
                ", \t type: " + type +
                ", \t score: " + score;
        }
    }

    public class SignInRequest
    {
        public string email { get; set; }
        public string password { get; set; }
    }




}


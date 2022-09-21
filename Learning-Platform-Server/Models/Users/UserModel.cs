using Learning_Platform_Server.Entities;

namespace Learning_Platform_Server.Models.Users
{
    public class UserModel
    {
        public string? UserId { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? Score { get; set; }
        public List<int>? AssignedGradeIdList { get; set; }


        public UserModel(MongoDbUser user)
        {
            UserId = user.UserId;
            Type = user.Type;
            Name = user.Name;
            Email = user.Email;
            Score = user.Score;
            AssignedGradeIdList = user.AssignedGradeIds;
        }

        public override string ToString()
        {
            return "UserModel: userid: " + UserId +
                "\t, type: " + Type +
                "\t, name: " + Name +
                "\t, email: " + Email +
                "\t, password: " + Password +
                "\t, score: " + Score;
            //"\t, assignedGradeIdList: " + AssignedGradeIdList;
        }
    }
}

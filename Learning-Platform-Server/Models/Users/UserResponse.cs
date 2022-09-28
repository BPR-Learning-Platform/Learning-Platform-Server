using Learning_Platform_Server.Entities;

namespace Learning_Platform_Server.Models.Users
{
    public class UserResponse
    {
        public string? UserId { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? Score { get; set; }
        public List<int>? AssignedGradeIds { get; set; }

        public override string ToString()
        {
            return "UserModel: userid: " + UserId +
                ", type: " + Type +
                ", name: " + Name +
                ", email: " + Email +
                ", password: " + Password +
                ", score: " + Score;
            //"\t, assignedGradeIds: " + AssignedGradeIds;
        }
    }
}

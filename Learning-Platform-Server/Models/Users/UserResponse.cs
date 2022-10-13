using Learning_Platform_Server.Entities;

namespace Learning_Platform_Server.Models.Users
{
    public class UserResponse
    {
        public string? UserId { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? Score { get; set; }
        public List<int>? AssignedGradeIds { get; set; }

        public override string ToString()
        {
            return "UserResponse: userid: " + UserId +
                ", type: " + Type +
                ", name: " + Name +
                ", email: " + Email +
                ", score: " + Score +
                ", assignedGradeIds: " + (AssignedGradeIds is not null ? string.Join(",", AssignedGradeIds) : "");
        }
    }

    public class UserResponseToTeacher
    {
        public string? UserId { get; set; }
        public string? Name { get; set; }

        public override string ToString()
        {
            return "UserResponse: userid: " + UserId +
                ", name: " + Name;
        }
    }
}

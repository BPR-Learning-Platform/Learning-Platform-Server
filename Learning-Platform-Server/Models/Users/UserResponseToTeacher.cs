using Learning_Platform_Server.Entities;

namespace Learning_Platform_Server.Models.Users
{
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

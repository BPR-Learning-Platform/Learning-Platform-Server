using Learning_Platform_Server.Entities;

namespace Learning_Platform_Server.Models.Users
{
    public class CreateUserRequest
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public List<int>? AssignedGradeIds { get; set; }

        public override string ToString()
        {
            return "CreateUserRequest: type: " + Type +
                ", name: " + Name +
                ", email: " + Email +
                ", password: " + Password +
                ",\n\t" + "assignedGradeIds: \n\t" + (AssignedGradeIds is not null ? string.Join(",\n\t", AssignedGradeIds) : "");
        }
    }
}

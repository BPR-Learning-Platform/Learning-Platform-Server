using Learning_Platform_Server.Models.Scores;

namespace Learning_Platform_Server.Models.Users
{
    public class UserResponse
    {
        public string? UserId { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }

        private string? _email;
        public string? Email
        {
            get { return _email; }
            set { _email = ("" + value).ToLower(); }
        }
        public ScoreResponse? Score { get; set; }
        public List<int>? AssignedGradeIds { get; set; }

        public override string ToString()
        {
            return "UserResponse: userid: " + UserId +
                ", type: " + Type +
                ", name: " + Name +
                ", email: " + Email +
                ", " + Score +
                ",\n\t" + "assignedGradeIds: \n\t" + (AssignedGradeIds is not null ? string.Join(",\n\t", AssignedGradeIds) : "");
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

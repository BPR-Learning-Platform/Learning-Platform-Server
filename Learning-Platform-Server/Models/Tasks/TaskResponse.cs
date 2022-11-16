namespace Learning_Platform_Server.Models.Tasks
{
    public class TaskResponse
    {
        public string? TaskId { get; set; }
        public int? Step { get; set; }
        public string? Type { get; set; }
        public int? Difficulty { get; set; }
        public string? Exercise { get; set; }
        public int? Answer { get; set; }

        public override string ToString()
        {
            return "TaskResponse: taskid: " + TaskId +
                ", step: " + Step +
                ", type: " + Type +
                ", difficulty: " + Difficulty +
                ", exercise: " + Exercise +
                ", answer: " + Answer;
        }
    }
}

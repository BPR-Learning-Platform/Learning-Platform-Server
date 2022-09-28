using Learning_Platform_Server.Entities;

namespace Learning_Platform_Server.Models.Tasks
{
    public class TaskResponse
    {
        public string? TaskId { get; set; }
        public int? Step { get; set; }
        public int? Difficulty { get; set; }
        public string? Exercise { get; set; }
        public int? Answer { get; set; }

        public override string ToString()
        {
            return "MongoDbTask: taskid: " + TaskId +
                ", step: " + Step +
                ", difficulty: " + Difficulty +
                ", exercise: " + Exercise +
                ", answer: " + Answer;
        }
    }
}

using Learning_Platform_Server.Entities;

namespace Learning_Platform_Server.Models.Tasks
{
    public class TaskResponse
    {
        private MongoDbTask? mongoDbTask;

        public string? TaskId { get; set; }
        public int? Step { get; set; }
        public int? Difficulty { get; set; }
        public string? Exercise { get; set; }
        public int? Answer { get; set; }

        public TaskResponse(MongoDbTask? mongoDbTask)
        {
            this.TaskId = mongoDbTask.TaskId;
            this.Step = mongoDbTask.Step;
            this.Difficulty = mongoDbTask.Difficulty;
            this.Exercise = mongoDbTask.Exercise;
            this.Answer = mongoDbTask.Answer;
        }

        public override string ToString()
        {
            return "MongoDbTask: taskid: " + TaskId +
                "\t, step: " + Step +
                "\t, difficulty: " + Difficulty +
                "\t, exercise: " + Exercise +
                "\t, answer: " + Answer;
        }
    }
}

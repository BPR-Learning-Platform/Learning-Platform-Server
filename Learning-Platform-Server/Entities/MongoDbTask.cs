using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Learning_Platform_Server.Entities
{
    public class TaskRoot
    {
        public Id? Id { get; set; }
        public MongoDbTask? Step { get; set; } //TODO: Should be called Task in MongoDb instead, and thereafter changed here as well
    }

    public class MongoDbTask
    {
        public string? TaskId { get; set; }
        public int? Step { get; set; }
        public int? Difficulty { get; set; }
        public string? Exercise { get; set; }
        public int? Answer { get; set; }

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


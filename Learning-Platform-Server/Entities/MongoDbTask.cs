using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Learning_Platform_Server.Entities
{
    public class TaskRoot
    {
        public Id? _id { get; set; }
        public MongoDbTask? Task { get; set; }
    }

    public class MongoDbTask
    {
        public string? TaskID { get; set; }
        public string? Step { get; set; }
        public string? Difficulty { get; set; }
        public string? Exercise { get; set; }
        public string? Answer { get; set; }

        public override string ToString()
        {
            return "MongoDbTask: taskid: " + TaskID +
                ", step: " + Step +
                ", difficulty: " + Difficulty +
                ", exercise: " + Exercise +
                ", answer: " + Answer;
        }
    }
}


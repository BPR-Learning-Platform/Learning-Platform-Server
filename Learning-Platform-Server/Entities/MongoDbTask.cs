using Newtonsoft.Json;

namespace Learning_Platform_Server.Entities
{
    public class MongoDbTaskRoot
    {
        [JsonProperty("_id")]
        public Id? Id { get; set; }
        public MongoDbTask? Task { get; set; }
        public TaskId? TaskId { get; set; }
        public override string ToString() => "MongoDbTaskRoot: Oid: " + Id + ", " + Task + ", TaskId: " + TaskId;
    }
    public class TaskId
    {
        [JsonProperty("$numberLong")]
        public string? NumberLong { get; set; }
        public override string ToString() => NumberLong + "";
    }

    public class MongoDbTask
    {
        public string? Step { get; set; }
        public string? Type { get; set; }
        public string? Difficulty { get; set; }
        public string? Exercise { get; set; }
        public string? Answer { get; set; }

        public override string ToString()
        {
            return "MongoDbTask: step: " + Step +
                ", type: " + Type +
                ", difficulty: " + Difficulty +
                ", exercise: " + Exercise +
                ", answer: " + Answer;
        }
    }
}


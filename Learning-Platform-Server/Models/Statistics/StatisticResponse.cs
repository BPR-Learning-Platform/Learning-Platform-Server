using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Models.Scores;
using System.Text.Json.Serialization;

namespace Learning_Platform_Server.Models.Statistics
{
    public class StatisticResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? StudentId { get; set; }
        public string? GradeId { get; set; }
        public MultipleScore? MultipleScore { get; set; }
        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return "StatisticResponse: student id: " + StudentId +
                ", grade id: " + GradeId +
                ", " + MultipleScore +
                ", timestamp: " + TimeStamp;
        }
    }
}

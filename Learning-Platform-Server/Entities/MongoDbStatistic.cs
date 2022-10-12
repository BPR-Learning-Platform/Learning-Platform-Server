using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Learning_Platform_Server.Entities
{
    public class MongoDbStatisticRoot
    {
        [JsonProperty("_id")]
        public Id? Id { get; set; }
        public MongoDbStatistic? Statistic { get; set; }
        public StatisticId? StatisticId { get; set; }
        public override string ToString() => "MongoDbStatisticRoot: Oid: " + Id + ", " + Statistic + ", StatisticId: " + StatisticId;
    }
    public class StatisticId
    {
        [JsonProperty("$numberLong")]
        public string? NumberLong { get; set; }
        public override string ToString() => NumberLong + "";
    }

    public class MongoDbStatistic
    {
        public string? StudentId { get; set; }
        public string? GradeId { get; set; }
        public int? Score { get; set; }
        public DateTime? TimeStamp { get; set; }

        public override string ToString()
        {
            return "MongoDbStatistic: student id: " + StudentId +
                ", grade id: " + GradeId +
                ", score: " + Score +
                ", timestamp: " + TimeStamp;
        }
    }
}


using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Learning_Platform_Server.Entities
{
    public class MongoDbStatisticRoot
    {
        [JsonProperty("_id")]
        public Id? Id { get; set; }
        public MongoDbStatistic? Statistic { get; set; }
        public TimeStamp? TimeStamp { get; set; }
        public override string ToString() => "MongoDbStatisticRoot: Oid: " + Id + ", " + Statistic;
    }
    public class StatisticId
    {
        [JsonProperty("$numberLong")]
        public string? NumberLong { get; set; }
        public override string ToString() => NumberLong + "";
    }

    public class TimeStamp
    {
        [JsonProperty("$date")]
        public MongoDBDate? DateTime { get; set; }
        public override string ToString() => DateTime + "";
    }

    public class MongoDBDate
    {
        [JsonProperty("$numberLong")]
        public long NumberLong { get; set; }
        public override string ToString() => NumberLong + "";
    }

    public class MongoDbStatistic
    {
        public string? StudentId { get; set; }
        public string? GradeId { get; set; }
        public int? Score { get; set; }

        public override string ToString()
        {
            return "MongoDbStatistic: student id: " + StudentId +
                ", grade id: " + GradeId +
                ", score: " + Score;
        }
    }
}


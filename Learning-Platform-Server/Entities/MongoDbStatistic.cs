using Newtonsoft.Json;

namespace Learning_Platform_Server.Entities
{
    public class MongoDbStatisticRoot
    {
        [JsonProperty("_id")]
        public Id? Id { get; set; }
        [JsonProperty("User")]
        public MongoDbStatistic? Statistic { get; set; }
        public UserId? UserId { get; set; }
        public TimeStamp? TimeStamp { get; set; }
        public override string ToString() => "MongoDbStatisticRoot: Oid: " + Id + ", " + Statistic + ", UserId: " + UserId;
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
        [JsonProperty("AssignedGradeIDs")]
        public string? GradeId { get; set; }
        public MongoDbScore? Score { get; set; }

        public override string ToString()
        {
            return "MongoDbStatistic: grade id: " + GradeId +
                ", MongoDbScore: " + Score;
        }
    }
}


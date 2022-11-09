using Newtonsoft.Json;

namespace Learning_Platform_Server.Models.Score
{

    public class CorrectInfo
    {
        [JsonProperty("A")]
        public ScorePoint A { get; set; }
        [JsonProperty("B")]
        public ScorePoint M { get; set; }
        [JsonProperty("C")]
        public ScorePoint S { get; set; }
        [JsonProperty("D")]
        public ScorePoint D { get; set; }

        public override string ToString()
        {
            return "ScoreThing: " +
                "A: " + A +
                "M: " + M +
                "S: " + S +
                "D: " + D;
        }
    }
    public class ScorePoint
    {
        public int count { get; set; }
        public object percentage { get; set; }
        public override string ToString()
        {
            return "ScorePoint: " +
                "count: " + count +
                "percentage: " + percentage;
        }
    }


}

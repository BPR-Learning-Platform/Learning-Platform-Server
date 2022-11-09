using Newtonsoft.Json;

namespace Learning_Platform_Server.Models.Scores
{

    public class CorrectInfo
    {
        public ScorePoint? A { get; set; }
        public ScorePoint? M { get; set; }
        public ScorePoint? S { get; set; }
        public ScorePoint? D { get; set; }

        public override string ToString()
        {
            return "CorrectInfo: A: " + A +
                ", M: " + M +
                ", S: " + S +
                ", D: " + D;
        }
    }
    public class ScorePoint
    {
        public int Count { get; set; }
        public int? Percentage { get; set; }
        public override string ToString()
        {
            return "ScorePoint: " +
                "count: " + Count +
                "percentage: " + Percentage;
        }
    }
}

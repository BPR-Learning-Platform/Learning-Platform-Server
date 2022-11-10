using Newtonsoft.Json;

namespace Learning_Platform_Server.Models.Scores
{
    public class ScoreResponse
    {
        public float? A { get; set; }
        public float? M { get; set; }
        public float? S { get; set; }
        public float? D { get; set; }
        public override string ToString()
        {
            return "ScoreResponse: A: " + A +
                ", M: " + M +
                ", S: " + S +
                ", D: " + D;
        }
    }
}

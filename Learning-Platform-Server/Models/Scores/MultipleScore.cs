using Newtonsoft.Json;

namespace Learning_Platform_Server.Models.Scores
{
    public class MultipleScore
    {
        public float? A { get; set; }
        public float? M { get; set; }
        public float? S { get; set; }
        public float? D { get; set; }
        public override string ToString()
        {
            return "MultipleScore: A: " + A +
                ", M: " + M +
                ", S: " + S +
                ", D: " + D;
        }
    }
}

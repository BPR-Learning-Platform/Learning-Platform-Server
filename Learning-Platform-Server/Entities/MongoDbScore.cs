namespace Learning_Platform_Server.Entities
{
    public class MongoDbScore
    {
        public string? A { get; set; }
        public string? M { get; set; }
        public string? S { get; set; }
        public string? D { get; set; }
        public override string ToString()
        {
            return "MongoDbScore: A: " + A +
                ", M: " + M +
                ", S: " + S +
                ", D: " + D;
        }
    }
}

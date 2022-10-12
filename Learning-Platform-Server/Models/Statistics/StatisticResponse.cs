using Learning_Platform_Server.Entities;

namespace Learning_Platform_Server.Models.Grades
{
    public class StatisticResponse
    {
        public string? StatisticId { get; set; }
        public string? StudentId { get; set; }
        public string? GradeId { get; set; }
        public int? Score { get; set; }
        public DateTime? TimeStamp { get; set; }

        public override string ToString()
        {
            return "StatisticResponse: statistic id: " + StatisticId +
                ", student id: " + StudentId +
                ", score: " + Score +
                ", timestamp: " + TimeStamp;
        }
    }
}

using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Statistics;

namespace Learning_Platform_Server.Services
{
    public interface IStatisticService
    {
        List<StatisticResponse> GetAllByStudentId(int studentId);
        List<StatisticResponse> GetAllByGradeId(int gradetId);
    }

    public class StatisticService : IStatisticService
    {
        private readonly IStatisticDAO _statisticDAO;

        public StatisticService(IStatisticDAO statisticDAO)
        {
            _statisticDAO = statisticDAO;
        }

        public List<StatisticResponse> GetAllByStudentId(int studentId)
            => _statisticDAO.GetAllByParameter(studentId, null);

        public List<StatisticResponse> GetAllByGradeId(int gradeId)
        {
            List<StatisticResponse>? statisticListForTheGrade = _statisticDAO.GetAllByParameter(null, gradeId);

            List<StatisticResponse> statisticListWithAvgScores = GetAvg(statisticListForTheGrade);

            return statisticListWithAvgScores;
        }



        // helper methods

        public static List<StatisticResponse> GetAvg(List<StatisticResponse> statisticListForTheGrade)
        {
            // Group by TimeStamp Date and calculate the average Score for each group
            List<StatisticResponse> statisticListWithAvgScores = statisticListForTheGrade.GroupBy(
                statisticResponse => statisticResponse.TimeStamp.Date,
                statisticResponse => statisticResponse,
                (key, val) =>

                    new StatisticResponse
                    {
                        GradeId = val.ToList()[0].GradeId,
                        Score = new ScoreResponse()
                        {
                            A = val.Average(x => x.Score is not null ? x.Score.A : null),
                            M = val.Average(x => x.Score is not null ? x.Score.M : null),
                            S = val.Average(x => x.Score is not null ? x.Score.S : null),
                            D = val.Average(x => x.Score is not null ? x.Score.D : null)
                        },
                        TimeStamp = key
                    })

                    .ToList();

            return statisticListWithAvgScores;
        }
    }
}

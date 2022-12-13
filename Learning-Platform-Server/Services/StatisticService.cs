using Learning_Platform_Server.Daos;
using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Statistics;

namespace Learning_Platform_Server.Services
{
    public interface IStatisticService
    {
        List<StatisticResponse> GetAllByStudentId(int studentId);
        List<StatisticResponse> GetAllByGradeId(string gradeId);
        List<StatisticResponse> GetAllByStep(int step, string gradeIdToExclude);
    }

    public class StatisticService : IStatisticService
    {
        private readonly IStatisticDao _statisticDAO;

        public StatisticService(IStatisticDao statisticDAO)
        {
            _statisticDAO = statisticDAO;
        }

        public List<StatisticResponse> GetAllByStudentId(int studentId)
            => _statisticDAO.GetAllByParameter(studentId, null, null);

        public List<StatisticResponse> GetAllByGradeId(string gradeId)
        {
            List<StatisticResponse>? statisticListForTheGrade = _statisticDAO.GetAllByParameter(null, gradeId, null);

            List<StatisticResponse> statisticListWithAvgScores = GetAvg(statisticListForTheGrade, gradeId);

            return statisticListWithAvgScores;
        }
        public List<StatisticResponse> GetAllByStep(int step, string gradeIdToExclude)
        {
            List<StatisticResponse>? statisticListForStep = _statisticDAO.GetAllByParameter(null, null, step);

            List<StatisticResponse> statisticListForStepWithoutGrade = statisticListForStep.Where(x => x.GradeId is not null && !x.GradeId.Equals(gradeIdToExclude)).ToList();
            Console.WriteLine("statisticListForStepWithoutGrade: " + string.Join(",\n\t", statisticListForStepWithoutGrade));

            List<StatisticResponse> statisticListWithAvgScores = GetAvg(statisticListForStepWithoutGrade, null);

            return statisticListWithAvgScores;
        }



        // helper methods

        public static List<StatisticResponse> GetAvg(List<StatisticResponse> statisticListForTheGrade, string? gradeId)
        {
            // Group by TimeStamp Date and calculate the average Score for each group
            List<StatisticResponse> statisticListWithAvgScores = statisticListForTheGrade.GroupBy(
                statisticResponse => statisticResponse.TimeStamp.Date,
                statisticResponse => statisticResponse,
                (key, val) =>

                    new StatisticResponse
                    {
                        GradeId = gradeId,
                        Score = new ScoreResponse()
                        {
                            A = val.Average(x => x.Score?.A),
                            M = val.Average(x => x.Score?.M),
                            S = val.Average(x => x.Score?.S),
                            D = val.Average(x => x.Score?.D)
                        },
                        TimeStamp = key
                    })

                    .ToList();

            return statisticListWithAvgScores;
        }
    }
}

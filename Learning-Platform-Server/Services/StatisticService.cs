using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Statistics;

namespace Learning_Platform_Server.Services
{
    public interface IStatisticService
    {
        List<StatisticResponse> GetAllByStudentId(int studentId);
        List<StatisticResponse> GetAllByGradeId(int gradeId);
        List<StatisticResponse> GetAllByStep(int step, string gradeIdToExclude);
    }

    public class StatisticService : IStatisticService
    {
        private readonly IStatisticDAO _statisticDAO;

        public StatisticService(IStatisticDAO statisticDAO)
        {
            _statisticDAO = statisticDAO;
        }

        public List<StatisticResponse> GetAllByStudentId(int studentId)
            => _statisticDAO.GetAllByParameter(studentId, null, null);

        public List<StatisticResponse> GetAllByGradeId(int gradeId)
        {
            List<StatisticResponse>? statisticListForTheGrade = _statisticDAO.GetAllByParameter(null, gradeId, null);

            List<StatisticResponse> statisticListWithAvgScores = GetAvg(statisticListForTheGrade);

            return statisticListWithAvgScores;
        }
        public List<StatisticResponse> GetAllByStep(int step, string gradeIdToExclude)
        {
            List<StatisticResponse>? statisticListForTheStep = _statisticDAO.GetAllByParameter(null, null, step);

            List<StatisticResponse> statisticListWithAvgScores = GetAvg(statisticListForTheStep, gradeIdToExclude);

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

        public static List<StatisticResponse> GetAvg(List<StatisticResponse> statisticListForTheGrade, string gradeIdToExcludeFromAverage)
        {
            // For debugging
            //statisticListForTheGrade = AddDummyData(new List<StatisticResponse>()); 
            //Console.WriteLine("Received from DAO: " + string.Join(",\n\t", statisticListForTheGrade));

            List<StatisticResponse> statisticListWithoutGrade = statisticListForTheGrade.Where(x => x.GradeId is not null && !x.GradeId.Equals(gradeIdToExcludeFromAverage)).ToList();
            Console.WriteLine("statisticListWithoutGrade: " + string.Join(",\n\t", statisticListWithoutGrade));

            // Group by TimeStamp Date and calculate the average Score for each group
            List<StatisticResponse> statisticListWithAvgScores = statisticListWithoutGrade.GroupBy(
                statisticResponse => statisticResponse.TimeStamp.Date,
                statisticResponse => statisticResponse,
                (key, val) =>

                    new StatisticResponse
                    {
                        //GradeId = val.ToList()[0].GradeId,
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

        /*private static List<StatisticResponse> AddDummyData(List<StatisticResponse> statisticListForTheGrade)
        {
            for (int i = 1; i <= 3; i++)
            {
                statisticListForTheGrade.Add(new()
                {
                    GradeId = "1",
                    StudentId = "1",
                    Score = new() { A = 1, M = 1, S = 1, D = 1 },
                    TimeStamp = DateTime.Now.AddDays(7 * i)
                });
            }

            for (int i = 1; i <= 3; i++)
            {
                statisticListForTheGrade.Add(new()
                {
                    GradeId = "2",
                    StudentId = "1",
                    Score = new() { A = 4, M = 4, S = 4, D = 4 },
                    TimeStamp = DateTime.Now.AddDays(7 * i)
                });
            }

            for (int i = 1; i <= 3; i++)
            {
                statisticListForTheGrade.Add(new()
                {
                    GradeId = "3",
                    StudentId = "1",
                    Score = new() { A = 3, M = 3, S = 3, D = 3 },
                    TimeStamp = DateTime.Now.AddDays(7 * i)
                });
            }

            return statisticListForTheGrade;
        }*/
    }
}

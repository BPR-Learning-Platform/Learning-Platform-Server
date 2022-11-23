using FluentAssertions;
using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Statistics;
using Learning_Platform_Server.Services;
using Xunit.Abstractions;

namespace Learning_Platform_Server.Tests
{
    public class StatisticServiceTests
    {
        private readonly ITestOutputHelper _output;

        public StatisticServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GetAvg_calculates_correct_average_scores()
        {
            // Arrange
            string gradeId = "1";

            var lastWeek = DateTime.Now.AddDays(-7);
            var today = DateTime.Now;

            List<StatisticResponse> statisticListForTheGrade = new()
            {
                // last week
                new() { GradeId = gradeId, StudentId = "1", TimeStamp = lastWeek, Score = new() { A = 1.1f, M = 1.1f, S = 1.1f, D = 1.1f } },
                new() { GradeId = gradeId, StudentId = "2", TimeStamp = lastWeek, Score = new() { A = 1.9f, M = 1.9f, S = 1.9f, D = 1.9f } },

                // today
                new() { GradeId = gradeId, StudentId = "1", TimeStamp = today, Score = new() { A = 1.0f, M = 1.0f, S = 1.0f, D = 1.1f } },
                new() { GradeId = gradeId, StudentId = "2", TimeStamp = today, Score = new() { A = 2.0f, M = 3.0f, S = 1.0f, D = 3.9f } },
            };

            List<ScoreResponse> expectedAvgScores = new()
            {   
                // last week
                new ScoreResponse { A = 1.5f, M = 1.5f, S = 1.5f, D = 1.5f },

                // today
                new ScoreResponse { A = 1.5f, M = 2.0f, S = 1.0f, D = 2.5f },
            };


            // Act
            List<StatisticResponse> foundAvgStatistics = StatisticService.GetAvg(statisticListForTheGrade, gradeId);
            List<ScoreResponse?>? foundAvgScores = foundAvgStatistics.Select(x => x.Score).ToList(); //projecting the ScoreResponse, in order to be able to compare with expectedAvgScores

            _output.WriteLine("Expected AvgScores: \n\t" + string.Join(",\t", expectedAvgScores));
            _output.WriteLine("Found AvgScores: \n\t" + string.Join(",\t", foundAvgScores));


            // Assert
            foundAvgScores.Should().BeEquivalentTo(expectedAvgScores);
        }
    }
}

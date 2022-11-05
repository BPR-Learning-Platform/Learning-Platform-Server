using FluentAssertions;
using Learning_Platform_Server.Models.Grades;
using Learning_Platform_Server.Models.Statistics;
using Learning_Platform_Server.Models.Tasks;
using Learning_Platform_Server.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Learning_Platform_Server.Tests
{
    public class StatisticsControllerTests
    {
        private const string StatisticsUrl = "/statistics";
        private readonly ITestOutputHelper _output;

        public StatisticsControllerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task GetAllByStudentId_receives_200_OK_with_multiple_objects_of_expected_type()
        {
            string studentId = "65";
            List<StatisticResponse>? statisticResponseList = await GetStatisticResponseListAsync("?studentid=" + studentId);

            statisticResponseList.Should().NotBeNullOrEmpty();
            if (statisticResponseList is not null)
                TestStatisticList(statisticResponseList, false);
        }

        [Fact]
        public async Task GetAllByGradeId_receives_200_OK_with_multiple_objects_of_expected_type_and_correct_average_scores()
        {
            int gradeId = 2;
            List<StatisticResponse>? statisticListWithAvgScores = await GetStatisticResponseListAsync("?gradeid=" + gradeId);

            statisticListWithAvgScores.Should().NotBeNullOrEmpty();
            if (statisticListWithAvgScores is not null)
            {
                TestStatisticList(statisticListWithAvgScores, true);

                // Score: The average score should be correct
                List<StatisticResponse> statisticListForTheGrade = StatisticService.GetAllByParameter(null, gradeId);
                EnsureCorrectAvgScoreCalculation(statisticListWithAvgScores, statisticListForTheGrade);
            }
        }



        // helper methods

        private static async Task<List<StatisticResponse>?> GetStatisticResponseListAsync(string queryString)
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            HttpResponseMessage? httpResponseMessage = await client.GetAsync(StatisticsUrl + queryString);
            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

            string? responseContentString = httpResponseMessage.Content.ReadAsStringAsync().Result;
            responseContentString.Should().NotBeNullOrEmpty();
            List<StatisticResponse>? statisticResponseList = JsonConvert.DeserializeObject<List<StatisticResponse>>(responseContentString);

            return statisticResponseList;
        }

        private void TestStatisticList(List<StatisticResponse> statisticResponseList, bool byGradeId)
        {
            statisticResponseList.Count.Should().BeGreaterThanOrEqualTo(3);
            _output.WriteLine("Number of statistics deserialized: " + statisticResponseList.Count);

            foreach (StatisticResponse? statisticResponse in statisticResponseList)
            {

                // Check for rules that apply to any query parameter
                _output.WriteLine("Found a statistic: " + statisticResponse);

                // GradeId
                statisticResponse.GradeId.Should().NotBeNull();
                if (statisticResponse.GradeId is not null)
                    int.Parse(statisticResponse.GradeId).Should().BeGreaterThan(0);

                // TimeStamp
                statisticResponse.TimeStamp.Should().BeAfter(new DateTime(2022, 01, 01));

                // Score
                statisticResponse.Score.Should().NotBeNull();
                if (statisticResponse.Score is not null)
                {
                    statisticResponse.Score.Should().BeGreaterOrEqualTo(0);
                    statisticResponse.Score.Should().BeLessThanOrEqualTo(10);
                }


                // Check for rules that depend on the query parameter

                // Student: should be null when returning statistics by grade (since the scores for grades are average values)
                if (byGradeId)
                    statisticResponse.StudentId.Should().BeNull();
                else
                    statisticResponse.StudentId.Should().NotBeNull();
            }
        }

        private void EnsureCorrectAvgScoreCalculation(List<StatisticResponse> statisticListWithAvgScores, List<StatisticResponse> statisticList)
        {
            foreach (StatisticResponse? statisticResponse in statisticListWithAvgScores)
            {
                DateTime timeStampToCheck = statisticResponse.TimeStamp;
                string? gradeIdToCheck = statisticResponse.GradeId;
                float? scoreToCheck = statisticResponse.Score;

                float totalScoreFound = 0;
                List<StatisticResponse> statisticListToCheck = statisticList.Where(x => x.GradeId is not null && x.GradeId.Equals(gradeIdToCheck) && x.TimeStamp.Date.Equals(timeStampToCheck.Date)).ToList();
                _output.WriteLine("statisticListToCheck: " + string.Join(",", statisticListToCheck));

                foreach (StatisticResponse? statisticToCheck in statisticListToCheck)
                {
                    if (statisticToCheck.Score is not null)
                        totalScoreFound += (float)statisticToCheck.Score;
                }

                float avgScoreFound = totalScoreFound / statisticListToCheck.Count;

                _output.WriteLine("expected avg score: " + scoreToCheck);
                _output.WriteLine("found avg score: " + avgScoreFound);

                scoreToCheck.Should().BeApproximately(avgScoreFound, 0.001f);
            }
        }
    }
}

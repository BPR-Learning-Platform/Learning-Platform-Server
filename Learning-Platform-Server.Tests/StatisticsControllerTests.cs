using FluentAssertions;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Statistics;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
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
            string studentId = "124";
            List<StatisticResponse>? statisticResponseList = await GetStatisticResponseListAsync("?studentid=" + studentId);

            statisticResponseList.Should().NotBeNullOrEmpty();
            TestStatisticList(statisticResponseList!, false);
        }

        [Fact]
        public async Task GetAllByGradeId_receives_200_OK_with_multiple_objects_of_expected_type()
        {
            int gradeId = 2;
            List<StatisticResponse>? statisticListWithAvgScores = await GetStatisticResponseListAsync("?gradeid=" + gradeId);

            statisticListWithAvgScores.Should().NotBeNullOrEmpty();
            TestStatisticList(statisticListWithAvgScores!, true);
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
            int expectedNumber = 3;
            statisticResponseList.Count.Should().BeGreaterThanOrEqualTo(expectedNumber);
            _output.WriteLine("Number of statistics deserialized: " + statisticResponseList.Count);

            foreach (StatisticResponse? statisticResponse in statisticResponseList)
            {

                // Check for rules that apply to any query parameter
                _output.WriteLine("Found a statistic: " + statisticResponse);

                // GradeId
                statisticResponse.GradeId.Should().NotBeNullOrEmpty();
                int.Parse(statisticResponse.GradeId!).Should().BeGreaterThan(0);

                // TimeStamp
                statisticResponse.TimeStamp.Should()
                    .BeAfter(new DateTime(2022, 01, 01))
                    .And.BeBefore(DateTime.Now);

                // Score
                statisticResponse.Score.Should().NotBeNull();
                TestSubScore(statisticResponse.Score!.A);
                TestSubScore(statisticResponse.Score.M);
                TestSubScore(statisticResponse.Score.S);
                TestSubScore(statisticResponse.Score.D);

                // Check for rules that depend on the query parameter

                // Student: should be null when returning statistics by grade (since the scores for grades are average values)
                if (byGradeId)
                    statisticResponse.StudentId.Should().BeNull();
                else
                    statisticResponse.StudentId.Should().NotBeNullOrEmpty();
            }

            static void TestSubScore(float? subScore) => subScore.Should()
                .BeInRange(Util.MinimumScore, Util.MaximumScore);
        }
    }
}
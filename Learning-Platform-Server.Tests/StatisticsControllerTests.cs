using FluentAssertions;
using Learning_Platform_Server.Models.Statistics;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NSubstitute;
using System.Net;
using Xunit.Abstractions;

namespace Learning_Platform_Server.Tests
{
    public class StatisticsControllerTests
    {
        private const string StatisticsUrl = "/statistics";
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;

        private const float MinimumScore = 1.0f;
        private const float MaximumScore = 3.9f;

        public StatisticsControllerTests(ITestOutputHelper output)
        {
            // More information on how this works: https://www.thecodebuzz.com/read-appsettings-json-in-net-core-test-project-xunit-mstest/
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, false)
                //.AddEnvironmentVariables()
                .Build();

            _output = output;
        }

        [Fact]
        public async Task GetAllByStudentId_receives_200_OK_with_multiple_objects_of_expected_type()
        {
            string studentId = "124";
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
                EnsureCorrectAvgScoreCalculation(statisticListWithAvgScores, gradeId);
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
            int expectedNumber = 1; //TODO Change to 3, when there are more statistics in the database
            statisticResponseList.Count.Should().BeGreaterThanOrEqualTo(expectedNumber);
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
                    TestScore(statisticResponse.Score.A);
                    TestScore(statisticResponse.Score.M);
                    TestScore(statisticResponse.Score.S);
                    TestScore(statisticResponse.Score.D);
                }


                // Check for rules that depend on the query parameter

                // Student: should be null when returning statistics by grade (since the scores for grades are average values)
                if (byGradeId)
                    statisticResponse.StudentId.Should().BeNull();
                else
                    statisticResponse.StudentId.Should().NotBeNull();
            }

            void TestScore(float? score) => score.Should().BeGreaterOrEqualTo(MinimumScore);
        }

        private void EnsureCorrectAvgScoreCalculation(List<StatisticResponse> statisticListWithAvgScores, int? gradeId) //TODO: Redo and reactivate
        {
            /* IHttpClientFactory? httpClientFactoryMock = GetHttpClientFactoryMock();
             StatisticDAO? statisticDAO = new(httpClientFactoryMock);
             List<StatisticResponse> statisticListForTheGrade = statisticDAO.GetAllByParameter(null, gradeId);

             foreach (StatisticResponse? statisticResponse in statisticListWithAvgScores)
             {
                 DateTime timeStampToCheck = statisticResponse.TimeStamp;
                 string? gradeIdToCheck = statisticResponse.GradeId;
                 float? scoreToCheck = statisticResponse.Score;

                 float totalScoreFound = 0;
                 List<StatisticResponse> statisticListToCheck = statisticListForTheGrade.Where(x => x.GradeId is not null && x.GradeId.Equals(gradeIdToCheck) && x.TimeStamp.Date.Equals(timeStampToCheck.Date)).ToList();
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
             } */
        }

        private IHttpClientFactory GetHttpClientFactoryMock()
        {
            IHttpClientFactory? httpClientFactoryMock = Substitute.For<IHttpClientFactory>();

            string? mongoDbBaseUrl = _configuration.GetConnectionString("MongoDbBaseUrl");

            HttpClient httpClient = new()
            {
                BaseAddress = new Uri(mongoDbBaseUrl)
            };

            httpClientFactoryMock.CreateClient("MongoDB").Returns(httpClient);

            return httpClientFactoryMock;
        }
    }
}
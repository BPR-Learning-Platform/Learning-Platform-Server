using FluentAssertions;
using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Users;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit.Abstractions;

namespace Learning_Platform_Server.Tests
{
    public class UserDAOTests
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;

        public UserDAOTests(ITestOutputHelper output)
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
        public void UpdateUser_updates_user_score_correctly()
        {
            string studentId = "124";
            IHttpClientFactory? httpClientFactoryMock = GetHttpClientFactoryMock();
            UserDAO? userDAO = new(httpClientFactoryMock);

            UserResponse? user = userDAO.GetById(studentId);
            user.Should().NotBeNull();
            if (user is not null)
            {
                user.Score.Should().NotBeNull();
                if (user.Score is not null)
                {
                    ScoreResponse newScore = new()
                    {
                        A = Change(user.Score.A),
                        M = Change(user.Score.M),
                        S = Change(user.Score.S),
                        D = Change(user.Score.D)
                    };


                    // Update
                    user.Score = newScore;
                    userDAO.UpdateUser(user);


                    // Get
                    UserResponse? userAfterUpdate = userDAO.GetById(studentId);

                    userAfterUpdate.Should().NotBeNull();
                    if (userAfterUpdate is not null)
                    {
                        userAfterUpdate.Score.Should().NotBeNull();
                        if (userAfterUpdate.Score is not null)
                            // Should have equally named properties with the same value
                            userAfterUpdate.Score.Should().BeEquivalentTo(newScore);
                    }
                }
            }
        }



        // helper methods

        private static float? Change(float? subScore)
        {
            float changedScore;
            do
            {
                System.Random random = new();
                double val = (random.NextDouble() * (Util.MaximumScore - Util.MinimumScore) + Util.MinimumScore);
                float randomFloat = (float)val;
                changedScore = (float)Math.Round((randomFloat), 1);
            } while (changedScore == subScore);

            return changedScore;
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

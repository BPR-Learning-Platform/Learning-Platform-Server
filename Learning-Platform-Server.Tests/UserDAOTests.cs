using FluentAssertions;
using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Users;
using Xunit.Abstractions;

namespace Learning_Platform_Server.Tests
{
    [CollectionDefinition("Serial", DisableParallelization = true)] //To avoid TaskControllerTests from failing when running all tests together. More info: https://tsuyoshiushio.medium.com/controlling-the-serial-and-parallel-test-on-xunit-6174326da196
    [Collection("Serial")]
    public class UserDAOTests
    {
        private readonly ITestOutputHelper _output;

        public UserDAOTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void UpdateUser_updates_user_score_correctly()
        {
            string studentId = "124";
            IHttpClientFactory? httpClientFactoryMock = TestUtil.GetHttpClientFactoryMock();
            UserDAO? userDAO = new(httpClientFactoryMock);

            UserResponse user = userDAO.GetById(studentId);

            user.Score.Should().NotBeNull();
            ScoreResponse newScore = new()
            {
                A = Change(user.Score!.A),
                M = Change(user.Score.M),
                S = Change(user.Score.S),
                D = Change(user.Score.D)
            };


            // Update
            user.Score = newScore;
            userDAO.UpdateUser(user);


            // Get
            UserResponse userAfterUpdate = userDAO.GetById(studentId);

            userAfterUpdate.Score.Should().NotBeNull();
            // Should have equally named properties with the same value
            userAfterUpdate.Score?.Should().BeEquivalentTo(newScore);
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

    }
}

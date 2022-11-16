using FluentAssertions;
using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Models.Grades;
using Learning_Platform_Server.Models.Tasks;
using Learning_Platform_Server.Models.Users;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Learning_Platform_Server.Tests
{
    public class TasksControllerTests
    {
        private const string TasksUrl = "/tasks";
        private readonly ITestOutputHelper _output;

        public TasksControllerTests(ITestOutputHelper output)
        {
            _output = output;
        }


        // GET BATCH

        [Fact]
        public async Task Get_batch_for_signed_in_user_receives_200_OK_with_multiple_objects_of_expected_type()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            UserResponse userResponse = await GetUserResponseAsync(client);
            _output.WriteLine("Testing with user: " + userResponse);

            int? gradeId = userResponse.AssignedGradeIds?[0];
            gradeId.Should().NotBeNull();
            gradeId.Should().NotBe(0);

            if (gradeId is null) throw new Exception();

            GradeResponse gradeResponse = GetGradeResponseAsync(client, (int)gradeId);
            _output.WriteLine("Testing with grade: " + gradeResponse);

            string? userId = userResponse?.UserId; // "124";

            string correct = "{%22A%22:{%22count%22:2,%22percentage%22:50},%22M%22:{%22count%22:1,%22percentage%22:100},%22S%22:{%22count%22:0,%22percentage%22:100},%22D%22:{%22count%22:0,%22percentage%22:0}}";

            HttpResponseMessage? taskHttpResponseMessage = await client.GetAsync(TasksUrl + "?userid=" + userId + "&correct=" + correct + "&taskids=42,12,18");
            taskHttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

            string? taskResponseContentString = taskHttpResponseMessage.Content.ReadAsStringAsync().Result;
            taskResponseContentString.Should().NotBeNullOrEmpty();
            List<TaskResponse>? taskResponseList = JsonConvert.DeserializeObject<List<TaskResponse>>(taskResponseContentString);
            _output.WriteLine("taskResponseList: \n\t" + (taskResponseList is not null ? string.Join(",\n\t", taskResponseList) : ""));

            taskResponseList.Should().NotBeNullOrEmpty();
            if (taskResponseList is not null)
            {
                taskResponseList.Count.Should().BeGreaterThan(1);
                _output.WriteLine("Number of tasks deserialized: " + taskResponseList.Count);

                foreach (var taskResponse in taskResponseList)
                {
                    // Id
                    taskResponse.TaskId.Should().NotBeNull();
                    if (taskResponse.TaskId is not null)
                        int.Parse(taskResponse.TaskId).Should().BeGreaterThan(0);

                    // Exercise
                    taskResponse.Exercise.Should().NotBeNull();
                    if (taskResponse.Exercise is not null)
                        taskResponse.Exercise.Length.Should().BeGreaterThan(0);

                    // Step: should match the step associated with the users grade
                    if (gradeResponse.Step is null) throw new Exception();
                    _output.WriteLine("gradeResponse.Step: " + gradeResponse.Step + ". taskResponse.Step: " + taskResponse.Step);
                    taskResponse.Step.Should().Be((int)gradeResponse.Step);

                    // Type
                    List<string> list = new() { "A", "S", "M", "D" };
                    list.Should().Contain(taskResponse.Type);

                    // Difficulty
                    taskResponse.Difficulty.Should().BeGreaterThanOrEqualTo(1);

                    userResponse.Should().NotBeNull();
                    userResponse?.Score.Should().NotBeNull();

                    if (userResponse?.Score is not null)
                    {
                        float subScore = 0;
                        switch (taskResponse.Type)
                        {
                            case "A":
                                userResponse.Score.A.Should().NotBeNull();
                                if (userResponse.Score.A is not null)
                                    subScore = (float)userResponse.Score.A;
                                break;
                            case "S":
                                userResponse.Score.S.Should().NotBeNull();
                                if (userResponse.Score.S is not null)
                                    subScore = (float)userResponse.Score.S;
                                break;
                            case "M":
                                userResponse.Score.M.Should().NotBeNull();
                                if (userResponse.Score.M is not null)
                                    subScore = (float)userResponse.Score.M;
                                break;
                            case "D":
                                userResponse.Score.D.Should().NotBeNull();
                                if (userResponse.Score.D is not null)
                                    subScore = (float)userResponse.Score.D;
                                break;
                        }
                        taskResponse.Difficulty.Should().Be((int)subScore);
                    }

                    // Answer
                    // taskResponse.Answer
                }
            }
        }



        // helper methods

        private static GradeResponse GetGradeResponseAsync(HttpClient client, int gradeId)
        {
            IHttpClientFactory? httpClientFactoryMock = TestUtil.GetHttpClientFactoryMock();
            GradeDAO? gradeDAO = new(httpClientFactoryMock);

            return gradeDAO.GetById(gradeId);
        }

        private async Task<UserResponse> GetUserResponseAsync(HttpClient client)
        {
            JsonContent content = JsonContent.Create(new { email = "student1@student.com", password = "12345678" });

            HttpResponseMessage? userHttpResponseMessage = await client.PostAsync(UsersControllerTests.SignInUrl, content);
            _output.WriteLine("Statuscode:" + userHttpResponseMessage.StatusCode);
            _output.WriteLine(content.ReadAsStringAsync().Result);

            userHttpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

            string? userResponseContentString = userHttpResponseMessage.Content.ReadAsStringAsync().Result;
            userResponseContentString.Should().NotBeNullOrEmpty();

            UserResponse? userResponse = JsonConvert.DeserializeObject<UserResponse>(userResponseContentString);

            userResponse.Should().NotBeNull();
            return userResponse ?? throw new Exception();
        }
    }
}

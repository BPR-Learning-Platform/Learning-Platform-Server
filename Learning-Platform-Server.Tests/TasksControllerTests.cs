using FluentAssertions;
using Learning_Platform_Server.Models.Tasks;
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
    public class TasksControllerTests
    {
        private const string TasksUrl = "/tasks?";
        private readonly ITestOutputHelper _output;

        public TasksControllerTests(ITestOutputHelper output)
        {
            this._output = output;
        }


        // GET ALL

        [Fact]
        public async Task Get_batch_receives_200_OK_with_multiple_objects_of_expected_type()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            string userId = "1";

            HttpResponseMessage? httpResponseMessage = await client.GetAsync(TasksUrl + "userid=" + userId + "correct=" + 1);
            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

            string? responseContentString = httpResponseMessage.Content.ReadAsStringAsync().Result;
            responseContentString.Should().NotBeNullOrEmpty();
            List<TaskResponse>? taskResponseList = JsonConvert.DeserializeObject<List<TaskResponse>>(responseContentString);

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

                    // Step
                    taskResponse.Step.Should().BeGreaterThan(0);

                    // Difficulty
                    taskResponse.Difficulty.Should().BeGreaterThan(0);

                    // Answer
                    // taskResponse.Answer
                }
            }
        }
    }
}

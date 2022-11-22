using FluentAssertions;
using Learning_Platform_Server.Models.Tasks;
using Learning_Platform_Server.Models.Users;
using Learning_Platform_Server.Services;
using Xunit.Abstractions;

namespace Learning_Platform_Server.Tests
{
    public class TaskServiceTests
    {
        private readonly ITestOutputHelper _output;

        public TaskServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void HasAppropiateDifficulty_determines_expected_truth_value()
        {
            GetTruthValue(taskType: TaskType.A, difficulty: 2, subScore: 2.5f).Should().BeTrue();
            GetTruthValue(taskType: TaskType.A, difficulty: 1, subScore: 2.5f).Should().BeFalse();

            GetTruthValue(taskType: TaskType.M, difficulty: 3, subScore: 3.1f).Should().BeTrue();
            GetTruthValue(taskType: TaskType.M, difficulty: 1, subScore: 2.1f).Should().BeFalse();

            GetTruthValue(taskType: TaskType.S, difficulty: 3, subScore: 3.9f).Should().BeTrue();
            GetTruthValue(taskType: TaskType.S, difficulty: 2, subScore: 1.1f).Should().BeFalse();

            GetTruthValue(taskType: TaskType.D, difficulty: 1, subScore: 1.0f).Should().BeTrue();
            GetTruthValue(taskType: TaskType.D, difficulty: 1, subScore: 2.0f).Should().BeFalse();

        }

        private static bool GetTruthValue(TaskType taskType, int difficulty, float subScore)
        {
            TaskResponse taskResponse = new() { TaskId = "1", Step = 1, Type = taskType, Difficulty = difficulty, Exercise = "1+1", Answer = 2 };
            UserResponse userResponse = new() { UserId = "1", Type = UserType.S, Name = "Sponge Bob", Email = "sponge@bob.dk", Score = new() { A = 1.1f, M = 1.1f, S = 1.1f, D = 1.1f }, AssignedGradeIds = new() { 3 }, };

            switch (taskType)
            {
                case TaskType.A:
                    userResponse.Score.A = subScore;
                    break;
                case TaskType.M:
                    userResponse.Score.M = subScore;
                    break;
                case TaskType.S:
                    userResponse.Score.S = subScore;
                    break;
                case TaskType.D:
                    userResponse.Score.D = subScore;
                    break;
            }

            return TaskService.TaskHasAppropiateDifficulty(taskResponse, userResponse);
        }
    }
}

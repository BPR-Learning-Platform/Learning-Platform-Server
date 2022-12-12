using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Tasks;
using Learning_Platform_Server.Models.Users;

namespace Learning_Platform_Server.Services
{
    public interface ITaskService
    {
        List<TaskResponse> GetBatch(string studentId, CorrectInfo correctInfo, List<string> previousTaskIds);
    }

    public class TaskService : ITaskService
    {
        private readonly ITaskDAO _taskDAO;
        private readonly IUserService _userService;
        private readonly IGradeService _gradeService;

        public TaskService(ITaskDAO taskDAO, IUserService userService, IGradeService gradeService)
        {
            _userService = userService;
            _taskDAO = taskDAO;
            _gradeService = gradeService;
        }

        public List<TaskResponse> GetBatch(string studentId, CorrectInfo correctInfo, List<string> previousTaskIds)
        {
            UserResponse student = _userService.GetById(studentId);

            int step = _gradeService.GetStep(student);

            List<TaskResponse> taskResponseList = _taskDAO.GetAll(step).Where(taskResponse => TaskHasAppropiateDifficulty(taskResponse, student)).ToList();

            List<TaskResponse> taskResponseBatchList = new();
            Random random = new();
            for (int i = 0; i < Util.BatchSize; i++)
            {
                bool usable = false;
                TaskResponse? taskToAdd = null;
                do
                {
                    taskToAdd = taskResponseList[random.Next(0, taskResponseList.Count)];

                    // not sending tasks that were sent in the previous batch
                    if (taskToAdd?.TaskId is not null
                        && !previousTaskIds.Contains(taskToAdd.TaskId)
                        // sending unique tasks only
                        && !taskResponseBatchList.Any(x => x.TaskId is not null
                        && x.TaskId.Equals(taskToAdd.TaskId)))
                    {
                        taskResponseBatchList.Add(taskToAdd);
                        usable = true;
                    }
                } while (!usable);
            }

            // updating the users score asynchronously each time a new batch request is received
            Task.Run(() => _userService.UpdateUserScore(student, correctInfo));

            return taskResponseBatchList;
        }

        public static bool TaskHasAppropiateDifficulty(TaskResponse taskResponse, UserResponse userResponse)
        {
            if (userResponse.Score is null)
                throw new ArgumentException($"No score was found for the user parameter {nameof(userResponse)}. Details: {userResponse}");
            ScoreResponse score = userResponse.Score;

            float subScore = taskResponse.Type switch
            {
                TaskType.A => score.A ?? throw new ArgumentException(nameof(score.A)),
                TaskType.S => score.S ?? throw new ArgumentException(nameof(score.S)),
                TaskType.M => score.M ?? throw new ArgumentException(nameof(score.M)),
                TaskType.D => score.D ?? throw new ArgumentException(nameof(score.D)),
                _ => throw new ArgumentException("Invalid enum value for the Type attribute of the TaskResponse object", nameof(taskResponse))
            };

            // Difficulty (integer) should be equal to the integral part of the subScore (decimal number)
            // Example: Difficulty should be 2 if subScore is 2.7
            return taskResponse.Difficulty == (int)subScore;
        }


    }
}

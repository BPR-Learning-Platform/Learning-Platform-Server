using Learning_Platform_Server.Daos;
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
        private readonly ITaskDao _taskDao;
        private readonly IUserService _userService;
        private readonly IGradeService _gradeService;

        public TaskService(ITaskDao taskDao, IUserService userService, IGradeService gradeService)
        {
            _userService = userService;
            _taskDao = taskDao;
            _gradeService = gradeService;
        }

        public List<TaskResponse> GetBatch(string studentId, CorrectInfo correctInfo, List<string> previousTaskIds)
        {
            UserResponse student = _userService.GetById(studentId);

            int step = _gradeService.GetStep(student);

            List<TaskResponse> taskResponseList = _taskDao.GetAll(step).Where(taskResponse => TaskHasAppropiateDifficulty(taskResponse, student)).ToList();

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
                throw new ArgumentException($"No score was found for the user parameter. Details: {userResponse}", nameof(userResponse));
            ScoreResponse score = userResponse.Score;

            float subScore = taskResponse.Type switch
            {
                TaskType.A => score.A ?? throw new InvalidOperationException($"{nameof(score.A)} was null"),
                TaskType.S => score.S ?? throw new InvalidOperationException($"{nameof(score.S)} was null"),
                TaskType.M => score.M ?? throw new InvalidOperationException($"{nameof(score.M)} was null"),
                TaskType.D => score.D ?? throw new InvalidOperationException($"{nameof(score.D)} was null"),
                _ => throw new ArgumentException("Invalid enum value for the Type attribute of the TaskResponse object", nameof(taskResponse))
            };

            // Difficulty (integer) should be equal to the integral part of the subScore (decimal number)
            // Example: Difficulty should be 2 if subScore is 2.7
            return taskResponse.Difficulty == (int)subScore;
        }


    }
}

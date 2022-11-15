using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Tasks;
using Learning_Platform_Server.Models.Users;

namespace Learning_Platform_Server.Services
{
    public interface ITaskService
    {
        List<TaskResponse> GetBatch(string userid, CorrectInfo correctInfo, List<string> previousTaskIds);
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

        public List<TaskResponse> GetBatch(string userid, CorrectInfo correctInfo, List<string> previousTaskIds)
        {
            UserResponse userResponse = _userService.GetById(userid);

            int step = _gradeService.GetStep(userResponse);
            List<TaskResponse> taskResponseList = _taskDAO.GetAll(step);

            List<TaskResponse> taskResponseBatchList = new();
            Random random = new();
            for (int i = 0; i < Util.BatchSize; i++)
            {
                bool same = true;
                TaskResponse? taskToAdd = null;
                do
                {
                    taskToAdd = taskResponseList[random.Next(0, taskResponseList.Count)];

                    // not sending tasks that were sent in the previous batch
                    if (taskToAdd is not null && taskToAdd.TaskId is not null && !previousTaskIds.Contains(taskToAdd.TaskId)
                        // sending unique tasks only
                        && !taskResponseBatchList.Any(x => x.TaskId is not null && x.TaskId.Equals(taskToAdd.TaskId)))
                    {
                        taskResponseBatchList.Add(taskToAdd);
                        same = false;
                    }
                } while (same);
            }

            // updating the users score asynchronously each time a new batch request is received
            Task.Run(() => _userService.UpdateUserScore(userResponse, correctInfo));

            // for debugging
            //Console.WriteLine("taskResponseBatchList: \n\t" + (string.Join(",\n\t", taskResponseBatchList))); // expected to complete before "UpdateUserScore" has completed

            return taskResponseBatchList;
        }
    }
}

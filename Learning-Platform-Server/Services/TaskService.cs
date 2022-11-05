using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Tasks;
using Learning_Platform_Server.Models.Users;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Learning_Platform_Server.Services
{
    public interface ITaskService
    {
        List<TaskResponse> GetBatch(string userid, int correct, List<string> previousTaskIds);
    }

    public class TaskService : ITaskService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/task";
        private readonly IUserService _userService;
        private readonly IGradeService _gradeService;

        public TaskService(IUserService userService, IGradeService gradeService)
        {
            _userService = userService;
            _gradeService = gradeService;
        }

        public List<TaskResponse> GetBatch(string userid, int correct, List<string> previousTaskIds)
        {
            UserResponse? userResponse = _userService.GetById(userid);

            if (userResponse is null)
                throw new Exception("No user was found for userid " + userid);

            int step = _gradeService.GetStep(userResponse);
            List<TaskResponse> taskResponseList = GetAll(step);

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
            Task.Run(() => _userService.UpdateUserScore(userResponse, correct));

            // for debugging
            //Console.WriteLine("taskResponseBatchList: \n\t" + (string.Join(",\n\t", taskResponseBatchList))); // expected to complete before "UpdateUserScore" has completed

            return taskResponseBatchList;
        }

        private static List<TaskResponse> GetAll(int step)
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), Url + "?step=" + step);
            HttpResponseMessage httpResponseMessage = MongoDbHelper.GetHttpClient().SendAsync(httpRequestMessage).Result;

            List<TaskResponse> taskList = new();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new Exception("Database answered with statuscode " + httpResponseMessage.StatusCode + ".");

            BsonArray taskRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);

            foreach (BsonValue taskRootBsonValue in taskRootBsonArray)
            {
                MongoDbTaskRoot? mongoDbTaskRoot = MapToMongoDbTaskRoot(taskRootBsonValue);
                if (mongoDbTaskRoot is null)
                    break;

                TaskResponse? taskResponse = MapToTaskResponse(mongoDbTaskRoot);

                // only return valid tasks
                if (taskResponse is not null)
                    taskList.Add(taskResponse);

            }

            if (taskList.Count == 0)
                throw new Exception("Unable to read any tasks from task collection for step number " + step + " in the database.");

            return taskList;
        }



        // helper methods 

        private static MongoDbTaskRoot? MapToMongoDbTaskRoot(BsonValue taskRootBsonValue)
        {
            string? taskRootJson = MongoDbHelper.MapToJson(taskRootBsonValue);

            if (taskRootJson is null)
            {
                Console.WriteLine("taskRootJson is null, so mapping is not completed. ");
                return null;
            }


            MongoDbTaskRoot? mongoDbTaskRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<MongoDbTaskRoot>(taskRootJson);

            if (mongoDbTaskRoot is null)
            {
                Console.WriteLine("taskRoot is null, so mapping is not completed. ");
                return null;
            }

            if (mongoDbTaskRoot.Task is null)
            {
                Console.WriteLine("taskRoot.Task is null, so mapping is not completed. ");
                return null;
            }


            return mongoDbTaskRoot;
        }

        private static TaskResponse? MapToTaskResponse(MongoDbTaskRoot mongoDbTaskRoot)
        {
            // ignore MongoDbTask if one or more values are missing
            if (mongoDbTaskRoot is null
                || mongoDbTaskRoot.Task is null
                || mongoDbTaskRoot.TaskId is null
                || mongoDbTaskRoot.TaskId.NumberLong is null
                || String.IsNullOrEmpty(mongoDbTaskRoot.Task.Step)
                || String.IsNullOrEmpty(mongoDbTaskRoot.Task.Type)
                || String.IsNullOrEmpty(mongoDbTaskRoot.Task.Difficulty)
                || String.IsNullOrEmpty(mongoDbTaskRoot.Task.Exercise)
                || String.IsNullOrEmpty(mongoDbTaskRoot.Task.Answer))
            {
                Console.WriteLine("One or more properties of mongoDbTaskRoot is null or empty, so mapping to TaskResponse is not completed. " +
                    "Details: " + mongoDbTaskRoot);
                return null;
            }
            else
            {
                return new TaskResponse()
                {
                    TaskId = mongoDbTaskRoot.TaskId.NumberLong,
                    Step = int.Parse(mongoDbTaskRoot.Task.Step),
                    Type = mongoDbTaskRoot.Task.Type,
                    Difficulty = int.Parse(mongoDbTaskRoot.Task.Difficulty),
                    Exercise = mongoDbTaskRoot.Task.Exercise,
                    Answer = int.Parse(mongoDbTaskRoot.Task.Answer)
                };
            }
        }
    }
}

using Learning_Platform_Server.Entities;
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
        List<TaskResponse>? GetAll(int step);
    }

    public class TaskService : ITaskService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/task";

        public List<TaskResponse>? GetAll(int step)
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), Url + "?step=" + step);
            HttpResponseMessage httpResponseMessage = Util.GetHttpClient().SendAsync(httpRequestMessage).Result;

            List<TaskResponse> taskList = new();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                return null;

            BsonArray taskRootBsonArray = Util.MapToBsonArray(httpResponseMessage);

            foreach (BsonValue taskRootBsonValue in taskRootBsonArray)
            {
                MongoDbTask? mongoDbTask = MapToMongoDbTask(taskRootBsonValue);
                if (mongoDbTask is null)
                    break;

                TaskResponse? taskResponse = MapToTaskResponse(mongoDbTask);

                // only return valid tasks
                if (taskResponse is not null)
                    taskList.Add(taskResponse);

            }

            return taskList;
        }



        // helper methods 

        private static MongoDbTask? MapToMongoDbTask(BsonValue taskRootBsonValue)
        {
            string? taskRootJson = Util.MapToJson(taskRootBsonValue);

            if (taskRootJson is null)
            {
                Console.WriteLine("taskRootJson is null.");
                return null;
            }


            TaskRoot? taskRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskRoot>(taskRootJson);

            if (taskRoot is null)
            {
                Console.WriteLine("taskRoot is null.");
                return null;
            }

            if (taskRoot.Task is null)
            {
                Console.WriteLine("taskRoot.Task is null.");
                return null; ;
            }


            return taskRoot.Task;
        }

        private static TaskResponse? MapToTaskResponse(MongoDbTask mongoDbTask)
        {
            // ignore MongoDbTask if one or more values are missing
            if (String.IsNullOrEmpty(mongoDbTask.TaskID)
                || String.IsNullOrEmpty(mongoDbTask.Step)
                || String.IsNullOrEmpty(mongoDbTask.Difficulty)
                || String.IsNullOrEmpty(mongoDbTask.Exercise)
                || String.IsNullOrEmpty(mongoDbTask.Answer))
            {
                Console.WriteLine("One or more properties of mongoDbTask is null or empty, so mapping to TaskResponse is not completed. " +
                    "Details: " + mongoDbTask);
                return null;
            }
            else
            {
                return new TaskResponse()
                {
                    TaskId = mongoDbTask.TaskID,
                    Step = int.Parse(mongoDbTask.Step),
                    Difficulty = int.Parse(mongoDbTask.Difficulty),
                    Exercise = mongoDbTask.Exercise,
                    Answer = int.Parse(mongoDbTask.Answer)
                };
            }
        }
    }
}

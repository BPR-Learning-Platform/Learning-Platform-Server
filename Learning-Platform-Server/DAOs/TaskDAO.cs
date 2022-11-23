using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Tasks;
using MongoDB.Bson;
using System.Net;

namespace Learning_Platform_Server.DAOs
{
    public interface ITaskDAO
    {
        List<TaskResponse> GetAll(int step);
    }

    public class TaskDAO : ITaskDAO
    {
        private readonly HttpClient _httpClient;

        public TaskDAO(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MongoDB");
        }

        public List<TaskResponse> GetAll(int step)
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), "task?step=" + step);
            HttpResponseMessage httpResponseMessage = _httpClient.SendAsync(httpRequestMessage).Result;

            List<TaskResponse> taskList = new();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new Exception("Database answered with statuscode " + httpResponseMessage.StatusCode + ".");

            BsonArray taskRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);

            foreach (BsonValue taskRootBsonValue in taskRootBsonArray)
            {
                MongoDbTaskRoot mongoDbTaskRoot = MapToMongoDbTaskRoot(taskRootBsonValue);

                TaskResponse taskResponse = MapToTaskResponse(mongoDbTaskRoot);
                taskList.Add(taskResponse);
            }

            if (taskList.Count == 0)
                throw new Exception("Unable to read any tasks from task collection for step number " + step + " in the database.");

            return taskList;
        }



        // helper methods 

        private static MongoDbTaskRoot MapToMongoDbTaskRoot(BsonValue taskRootBsonValue)
        {
            string taskRootJson = MongoDbHelper.MapToJson(taskRootBsonValue);

            MongoDbTaskRoot mongoDbTaskRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<MongoDbTaskRoot>(taskRootJson) ?? throw new ArgumentNullException(nameof(taskRootJson));

            return mongoDbTaskRoot;
        }

        private static TaskResponse MapToTaskResponse(MongoDbTaskRoot mongoDbTaskRoot)
        {
            MongoDbTask task = mongoDbTaskRoot.Task ?? throw new ArgumentException(nameof(mongoDbTaskRoot.Task));
            TaskId taskId = mongoDbTaskRoot.TaskId ?? throw new ArgumentException(nameof(mongoDbTaskRoot.TaskId));

            return new TaskResponse()
            {
                TaskId = taskId.NumberLong ?? throw new ArgumentException(nameof(taskId.NumberLong)),
                Step = int.Parse(task.Step ?? throw new ArgumentException(nameof(task.Step))),
                Type = Enum.Parse<TaskType>(task.Type ?? throw new ArgumentException(nameof(task.Type))),
                Difficulty = int.Parse(task.Difficulty ?? throw new ArgumentException(nameof(task.Difficulty))),
                Exercise = task.Exercise ?? throw new ArgumentException(nameof(task.Exercise)),
                Answer = int.Parse(task.Answer ?? throw new ArgumentException(nameof(task.Answer)))
            };
        }
    }
}

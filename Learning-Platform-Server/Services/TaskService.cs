using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Models.Tasks;
using Learning_Platform_Server.Models.Users;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Learning_Platform_Server.Services
{
    public interface ITaskService
    {
        List<TaskResponse> GetAll(int step);
    }

    public class TaskService : ITaskService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/task";

        public List<TaskResponse> GetAll(int step)
        {
            HttpClient httpClient = new();

            HttpRequestMessage? request = new(new HttpMethod("GET"), Url + "?step=" + step);

            Task<HttpResponseMessage>? response = httpClient.SendAsync(request);

            JsonWriterSettings jsonWriterSettings = new() { OutputMode = JsonOutputMode.CanonicalExtendedJson };

            string? tasksBsonString = response.Result.Content.ReadAsStringAsync().Result;
            BsonArray taskBsonArray = BsonSerializer.Deserialize<BsonArray>(tasksBsonString);

            List<TaskResponse> taskList = new();

            foreach (var taskBson in taskBsonArray)
            {
                string? taskJson = taskBson.ToJson(jsonWriterSettings);
                TaskRoot taskRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskRoot>(taskJson);
                taskList.Add(new TaskResponse(taskRoot.Task));
            }

            return taskList;
        }
    }
}

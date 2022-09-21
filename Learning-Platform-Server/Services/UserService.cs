using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Models.Users;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Text;

namespace Learning_Platform_Server.Services
{
    public interface IUserService
    {
        ContentResult PostUser(SignInRequest signInRequest);
    }

    public class UserService : IUserService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/User";

        // POST USER
        public ContentResult PostUser(SignInRequest signInRequest)
        {
            HttpClient httpClient = new();

            HttpRequestMessage? request = new(new HttpMethod("POST"), Url + "/Signin");
            request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(signInRequest), Encoding.UTF8, "application/json");
            
            Task<HttpResponseMessage>? response = httpClient.SendAsync(request);

            JsonWriterSettings jsonWriterSettings = new() { OutputMode = JsonOutputMode.CanonicalExtendedJson };

            string? userRootBsonString = response.Result.Content.ReadAsStringAsync().Result;
            BsonArray userRootBsonArray = BsonSerializer.Deserialize<BsonArray>(userRootBsonString);

            MongoDbUser? mongoDbUser = null;

            StatusCodeResult statusCodeResult = new StatusCodeResult(401);

            if (userRootBsonArray.Count != 0)
            {
                statusCodeResult = new StatusCodeResult(200);

                BsonValue? userRootBson = userRootBsonArray[0];
                string? userRootJson = userRootBson.ToJson(jsonWriterSettings);
                UserRoot? userRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<UserRoot>(userRootJson);
                mongoDbUser = userRoot?.User;

                if (mongoDbUser != null)
                {
                    statusCodeResult = new StatusCodeResult(200);
                }

            }

            string? msg = statusCodeResult.StatusCode == 200 ? mongoDbUser?.Name : "No user was found with the given credentials";

            Console.WriteLine(msg);

            return new ContentResult() { Content = msg.ToJson(), StatusCode = statusCodeResult.StatusCode };
        }
    }
}
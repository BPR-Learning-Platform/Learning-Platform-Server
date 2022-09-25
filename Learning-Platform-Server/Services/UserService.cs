using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Models.Users;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Net;
using System.Text;

namespace Learning_Platform_Server.Services
{
    public interface IUserService
    {
        ContentResult PostUser(SignInRequest signInRequest);
        UserResponse? GetById(string id);
    }

    public class UserService : IUserService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/user";

        // SIGN IN
        public ContentResult PostUser(SignInRequest signInRequest)
        {
            HttpClient httpClient = new();

            HttpRequestMessage? request = new(new HttpMethod("POST"), Url + "/signin");
            request.Content = JsonContent.Create(signInRequest);

            Task<HttpResponseMessage>? response = httpClient.SendAsync(request);

            string? userRootBsonString = response.Result.Content.ReadAsStringAsync().Result;
            BsonArray userRootBsonArray = BsonSerializer.Deserialize<BsonArray>(userRootBsonString);

            MongoDbUser? mongoDbUser = null;

            StatusCodeResult statusCodeResult = new StatusCodeResult(401);

            if (userRootBsonArray.Count != 0)
            {
                BsonValue? userRootBson = userRootBsonArray[0];
                string? userRootJson = Util.MapToJson(userRootBson);
                UserRoot? userRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<UserRoot>(userRootJson);
                mongoDbUser = userRoot?.User;

                if (mongoDbUser != null)
                {
                    statusCodeResult = new StatusCodeResult(200);
                }
            }

            string? msg = statusCodeResult.StatusCode == 200 ? mongoDbUser.ToJson() : "No user was found with the given credentials";

            Console.WriteLine(msg);

            return new ContentResult() { Content = msg, StatusCode = statusCodeResult.StatusCode };
        }

        // GET BY ID

        public UserResponse? GetById(string id)
        {
            HttpClient httpClient = new();
            HttpRequestMessage? httpRequestMessage = new(new HttpMethod("GET"), Url + "?id=" + id);
            HttpResponseMessage? httpResponseMessage = httpClient.SendAsync(httpRequestMessage).Result;

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                return null;

            BsonArray userRootBsonArray = Util.MapToBsonArray(httpResponseMessage);

            if (userRootBsonArray.Count != 0)
            {
                BsonValue? userRootBsonValue = userRootBsonArray[0];

                MongoDbUser? mongoDbUser = MapToMongoDbUser(userRootBsonValue);
                if (mongoDbUser is null)
                    return null;

                UserResponse? userResponse = MapToUserResponse(mongoDbUser);

                return userResponse;
            }
            return null;
        }

        private MongoDbUser? MapToMongoDbUser(BsonValue userRootBsonValue)
        {
            string? userRootJson = Util.MapToJson(userRootBsonValue);

            if (userRootJson is null)
            {
                Console.WriteLine("userRootJson is null.");
                return null;
            }


            UserRoot? userRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<UserRoot>(userRootJson);

            return userRoot?.User;
        }

        private static UserResponse? MapToUserResponse(MongoDbUser mongoDbUser)
        {
            return new UserResponse()
            {
                UserId = mongoDbUser.UserId,
                Type = mongoDbUser.Type,
                Name = mongoDbUser.Name,
                Email = mongoDbUser.Email,
                Score = mongoDbUser.Score,
                AssignedGradeIds = mongoDbUser.AssignedGradeIds
            };
        }
    }
}

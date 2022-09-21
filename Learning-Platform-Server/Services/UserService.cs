using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Models.Users;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Learning_Platform_Server.Services
{
    public interface IUserService
    {
        ContentResult GetUser(SignInRequest signInRequest);
        UserModel GetById(int userId);
    }

    public class UserService : IUserService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/user";

        // SIGN IN USER
        public ContentResult GetUser(SignInRequest signInRequest)
        {
            HttpClient httpClient = new();

            HttpRequestMessage? request = new(new HttpMethod("GET"), Url + "?Email=" + signInRequest.Email + "&Password=" + signInRequest.Password);

            Task<HttpResponseMessage>? response = httpClient.SendAsync(request);

            JsonWriterSettings jsonWriterSettings = new() { OutputMode = JsonOutputMode.CanonicalExtendedJson };

            string? userRootBsonString = response.Result.Content.ReadAsStringAsync().Result;
            BsonArray userRootBsonArray = BsonSerializer.Deserialize<BsonArray>(userRootBsonString);

            MongoDBUser? mongoDBUser = null;

            StatusCodeResult statusCodeResult = new StatusCodeResult(401);

            if (userRootBsonArray.Count != 0)
            {
                statusCodeResult = new StatusCodeResult(200);

                BsonValue? userRootBson = userRootBsonArray[0];
                string? userRootJson = userRootBson.ToJson(jsonWriterSettings);
                UserRoot? userRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<UserRoot>(userRootJson);
                mongoDBUser = userRoot?.User;

                if (mongoDBUser != null)
                {
                    statusCodeResult = new StatusCodeResult(200);
                }

            }

            string? msg = statusCodeResult.StatusCode == 200 ? mongoDBUser.Email : "No user was found with the given credentials";

            Console.WriteLine(msg);

            return new ContentResult() { Content = msg.ToJson(), StatusCode = statusCodeResult.StatusCode };
        }

        // GET BY ID

        public UserModel GetById(int userId)
        {
            throw new NotImplementedException(); // TODO
        }


    }
}
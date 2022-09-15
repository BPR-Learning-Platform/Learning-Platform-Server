using Learning_Platform_Server.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Learning_Platform_Server.Services
{
    public interface IUserService
    {
        ContentResult GetUser(SignInRequest signInRequest);
    }

    public class UserService : IUserService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/User";

        // GET USER
        public ContentResult GetUser(SignInRequest signInRequest)
        {
            HttpClient httpClient = new();

            HttpRequestMessage? request = new(new HttpMethod("GET"), Url + "?Email=" + signInRequest.Email + "&Password=" + signInRequest.Password);

            Task<HttpResponseMessage>? response = httpClient.SendAsync(request);

            JsonWriterSettings jsonWriterSettings = new() { OutputMode = JsonOutputMode.CanonicalExtendedJson };

            string? userRootBsonString = response.Result.Content.ReadAsStringAsync().Result;
            BsonArray userRootBsonArray = BsonSerializer.Deserialize<BsonArray>(userRootBsonString);

            User? user = null;

            StatusCodeResult statusCodeResult = new StatusCodeResult(401);

            if (userRootBsonArray.Count != 0)
            {
                statusCodeResult = new StatusCodeResult(200);

                BsonValue? userRootBson = userRootBsonArray[0];
                string? userRootJson = userRootBson.ToJson(jsonWriterSettings);
                UserRoot? userRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<UserRoot>(userRootJson);
                user = userRoot?.User;

                if (user != null)
                {
                    statusCodeResult = new StatusCodeResult(200);
                }

            }

            string msg = statusCodeResult.StatusCode == 200 ? user.Email : "No user was found with the given credentials";

            Console.WriteLine(msg);

            return new ContentResult() { Content = msg.ToJson(), StatusCode = statusCodeResult.StatusCode };
        }
    }
}
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
        KeyValuePair<ContentResult, UserResponse?> SignInUser(SignInRequest signInRequest);
        UserResponse? GetById(string id);
    }

    public class UserService : IUserService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/user";

        // SIGN IN
        public KeyValuePair<ContentResult, UserResponse?> SignInUser(SignInRequest signInRequest)
        {
            HttpRequestMessage request = new(new HttpMethod("POST"), Url + "/signin")
            {
                Content = JsonContent.Create(signInRequest)
            };

            HttpResponseMessage httpResponseMessage = Util.GetHttpClient().SendAsync(request).Result;
            BsonArray userRootBsonArray = Util.MapToBsonArray(httpResponseMessage);
            if (userRootBsonArray.Count == 0)
            {
                return new KeyValuePair<ContentResult, UserResponse?>(new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "No user was found with the given credentials"
                }, null);
            }

            BsonValue userRootBson = userRootBsonArray[0];
            string? userRootJson = Util.MapToJson(userRootBson);
            if (userRootJson is null)
            {
                return new KeyValuePair<ContentResult, UserResponse?>(new ContentResult()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Content = "Could not read user information from BsonValue"
                }, null);
            }

            UserRoot? userRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<UserRoot>(userRootJson);
            MongoDbUser? mongoDbUser = userRoot?.User;
            if (mongoDbUser is null)
            {
                return new KeyValuePair<ContentResult, UserResponse?>(new ContentResult()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Content = "Could not read user from userRoot"
                }, null);
            }

            UserResponse userResponse = MapToUserResponse(mongoDbUser);

            return new KeyValuePair<ContentResult, UserResponse?>(new ContentResult()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Content = ""
            }, userResponse);
        }

        // GET BY ID

        public UserResponse? GetById(string id)
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), Url + "?id=" + id);
            HttpResponseMessage httpResponseMessage = Util.GetHttpClient().SendAsync(httpRequestMessage).Result;

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                return null;

            BsonArray userRootBsonArray = Util.MapToBsonArray(httpResponseMessage);

            if (userRootBsonArray.Count != 0)
            {
                BsonValue userRootBsonValue = userRootBsonArray[0];

                MongoDbUser? mongoDbUser = MapToMongoDbUser(userRootBsonValue);
                if (mongoDbUser is null)
                    return null;

                UserResponse? userResponse = MapToUserResponse(mongoDbUser);

                return userResponse;
            }
            return null;
        }


        // helper methods

        private static MongoDbUser? MapToMongoDbUser(BsonValue userRootBsonValue)
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

        private static UserResponse MapToUserResponse(MongoDbUser mongoDbUser)
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

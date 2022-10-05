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

            HttpResponseMessage httpResponseMessage = MongoDbHelper.GetHttpClient().SendAsync(request).Result;
            BsonArray userRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);
            if (userRootBsonArray.Count == 0)
            {
                return new KeyValuePair<ContentResult, UserResponse?>(new ContentResult()
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Content = "No user was found with the given credentials"
                }, null);
            }

            BsonValue userRootBsonValue = userRootBsonArray[0];

            MongoDbUserRoot? mongoDbUserRoot = MapToMongoDbUserRoot(userRootBsonValue);
            if (mongoDbUserRoot is null)
            {
                return new KeyValuePair<ContentResult, UserResponse?>(new ContentResult()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Content = "Could not read user information from BsonValue"
                }, null);
            }

            if (mongoDbUserRoot is null)
            {
                return new KeyValuePair<ContentResult, UserResponse?>(new ContentResult()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Content = "Could not read mongoDbUserRoot from userRootJson"
                }, null);
            }

            UserResponse? userResponse = MapToUserResponse(mongoDbUserRoot);

            return new KeyValuePair<ContentResult, UserResponse?>(new ContentResult()
            {
                StatusCode = StatusCodes.Status200OK,
                Content = ""
            }, userResponse);
        }

        // GET BY ID

        public UserResponse? GetById(string id)
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), Url + "?userid=" + id);
            HttpResponseMessage httpResponseMessage = MongoDbHelper.GetHttpClient().SendAsync(httpRequestMessage).Result;

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new Exception("Database answered with statuscode " + httpResponseMessage.StatusCode + ".");

            BsonArray userRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);

            if (userRootBsonArray.Count != 0)
            {
                BsonValue userRootBsonValue = userRootBsonArray[0];

                MongoDbUserRoot? mongoDbUserRoot = MapToMongoDbUserRoot(userRootBsonValue);
                if (mongoDbUserRoot is null)
                {
                    Console.WriteLine("UserRoot was not found");
                    return null;
                }

                UserResponse? userResponse = MapToUserResponse(mongoDbUserRoot);

                return userResponse;
            }
            return null;
        }


        // helper methods

        private static MongoDbUserRoot? MapToMongoDbUserRoot(BsonValue userRootBsonValue)
        {
            string? userRootJson = MongoDbHelper.MapToJson(userRootBsonValue);

            if (userRootJson is null)
            {
                Console.WriteLine("userRootJson is null");
                return null;
            }

            MongoDbUserRoot? mongoDbUserRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<MongoDbUserRoot>(userRootJson);

            Console.WriteLine("Mapping complete, found: " + mongoDbUserRoot);

            return mongoDbUserRoot;
        }

        private static UserResponse? MapToUserResponse(MongoDbUserRoot mongoDbUserRoot)
        {
            if (mongoDbUserRoot.User is null)
            {
                Console.WriteLine("User was not found in mongoDbUserRoot");
                return null;
            }

            if (mongoDbUserRoot.UserId is null)
            {
                Console.WriteLine("UserId was not found in mongoDbUserRoot");
                return null;
            }

            return new UserResponse()
            {
                UserId = mongoDbUserRoot.UserId.NumberLong,
                Type = mongoDbUserRoot.User.Type,
                Name = mongoDbUserRoot.User.Name,
                Email = mongoDbUserRoot.User.Email,
                Score = mongoDbUserRoot.User.Score,
                AssignedGradeIds = mongoDbUserRoot.User.AssignedGradeIds
            };
        }
    }
}

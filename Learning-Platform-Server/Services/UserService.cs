using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Helpers;
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
        UserResponse SignInUser(SignInRequest signInRequest);
        UserResponse? GetById(string id);
        List<UserResponse> GetByGradeId(int gradeId);
    }

    public class UserService : IUserService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/user";

        // SIGN IN
        public UserResponse SignInUser(SignInRequest signInRequest)
        {
            HttpRequestMessage request = new(new HttpMethod("POST"), Url + "/signin")
            {
                Content = JsonContent.Create(signInRequest)
            };

            HttpResponseMessage httpResponseMessage = MongoDbHelper.GetHttpClient().SendAsync(request).Result;
            BsonArray userRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);
            if (userRootBsonArray.Count == 0)
                throw new UnauthorizedAccessException("No user was found with the given credentials");

            BsonValue userRootBsonValue = userRootBsonArray[0];

            MongoDbUserRoot? mongoDbUserRoot = MapToMongoDbUserRoot(userRootBsonValue);
            if (mongoDbUserRoot is null)
                throw new Exception("Could not read user information from BsonValue");

            UserResponse userResponse = MapToUserResponse(mongoDbUserRoot);

            return userResponse;
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
                    throw new Exception("Could not read user information from BsonValue");

                UserResponse userResponse = MapToUserResponse(mongoDbUserRoot);

                return userResponse;
            }
            return null;
        }

        public List<UserResponse> GetByGradeId(int gradeId)
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/wholegrade" + "?assignedgradeid=" + gradeId);
            HttpResponseMessage httpResponseMessage = MongoDbHelper.GetHttpClient().SendAsync(httpRequestMessage).Result;

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new Exception("Database answered with statuscode " + httpResponseMessage.StatusCode + ".");

            List<UserResponse> userList = new();

            BsonArray userRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);

            foreach (BsonValue userRootBsonValue in userRootBsonArray)
            {
                MongoDbUserRoot? mongoDbUserRoot = MapToMongoDbUserRoot(userRootBsonValue);
                if (mongoDbUserRoot is null)
                    break;

                UserResponse? userResponse = MapToUserResponse(mongoDbUserRoot);

                // only return valid tasks
                if (userResponse is not null)
                    userList.Add(userResponse);

            }

            return userList;
        }


        // helper methods

        private static MongoDbUserRoot? MapToMongoDbUserRoot(BsonValue userRootBsonValue)
        {
            string? userRootJson = MongoDbHelper.MapToJson(userRootBsonValue);

            if (userRootJson is null)
            {
                Console.WriteLine("userRootJson is null, so mapping to UserRoot is not completed. ");
                return null;
            }

            MongoDbUserRoot? mongoDbUserRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<MongoDbUserRoot>(userRootJson);

            Console.WriteLine("Mapping complete, found: " + mongoDbUserRoot);

            return mongoDbUserRoot;
        }

        private static UserResponse MapToUserResponse(MongoDbUserRoot mongoDbUserRoot)
        {
            if (mongoDbUserRoot.User is null)
                throw new NullReferenceException("User was not found in mongoDbUserRoot");

            if (mongoDbUserRoot.UserId is null)
                throw new NullReferenceException("UserId was not found in mongoDbUserRoot");

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

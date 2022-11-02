using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Users;
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
        UserResponse SignInUser(SignInRequest signInRequest);
        UserResponse? GetById(string id);
        List<UserResponse> GetByGradeId(int gradeId);
        UserResponse UpdateUserScore(UserResponse userResponse, int correct);
        float CalculateNewScore(float score, int correct);
    }

    public class UserService : IUserService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/user";

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

                // only return valid users
                if (userResponse is not null)
                    userList.Add(userResponse);

            }

            return userList;
        }

        public UserResponse UpdateUserScore(UserResponse userResponse, int correct)
        {
            if (userResponse.Score is null)
                throw new NullReferenceException("No score was found for the user: " + userResponse);

            userResponse.Score = CalculateNewScore((float)userResponse.Score, correct);

            UpdateUser(userResponse, correct);

            // returning an updated version of the same user-object, since the database does not return the updated user
            return userResponse;
        }



        // helper methods

        public float CalculateNewScore(float score, int correct)
        {
            float correctNumber = 0;
            if (correct > 0)
                correctNumber = correct / 100 * Util.BatchSize;

            float incorrectNumber = Util.BatchSize - correctNumber;

            // increase/decrease score with 0.1 points for each correct/incorrect answer
            float change = (correctNumber - incorrectNumber) / 10;
            float newScore = score + change;

            //rounding the result
            newScore = (float)Math.Round((newScore), 2);

            return newScore;
        }

        private static HttpResponseMessage UpdateUser(UserResponse userResponse, int correct)
        {
            MongoDbUser mongoDbUser = MapToMongoDbUser(userResponse);
            HttpRequestMessage request = new(new HttpMethod("PUT"), Url)
            {
                Content = JsonContent.Create(mongoDbUser)
            };

            HttpResponseMessage httpResponseMessage = MongoDbHelper.GetHttpClient().SendAsync(request).Result;

            // throws an exception if the PUT request went wrong
            ValidateMongoDbPutRequestResponse(mongoDbUser, httpResponseMessage);

            return httpResponseMessage;
        }

        private static void ValidateMongoDbPutRequestResponse(MongoDbUser mongoDbUser, HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new Exception("Database answered with statuscode " + httpResponseMessage.StatusCode);

            string? responseString = httpResponseMessage.Content.ReadAsStringAsync().Result;
            JObject? responseJobject = (JObject?)Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);

            if (responseJobject is null)
                throw new Exception();

            JToken? upsertedIdJToken = responseJobject["upsertedId"];
            JToken? matchedCountJToken = responseJobject["matchedCount"];
            JToken? modifiedCountJToken = responseJobject["modifiedCount"];

            if (upsertedIdJToken is not null)
                throw new Exception("Database inserted a new user instead of updating an existing user. Details: " + mongoDbUser);

            if (matchedCountJToken is null || modifiedCountJToken is null)
                throw new Exception("Database did not return expected details. The user might not have been updated. Details: " + mongoDbUser);

            int matchedCount = int.Parse(matchedCountJToken.ToString());
            int modifiedCount = int.Parse(modifiedCountJToken.ToString());

            if (matchedCount != 1 || modifiedCount != 1)
                throw new Exception("Database did not behave as expected Details: matchedCount was " + matchedCount + " and modifiedCount was " + modifiedCount + " for the user " + mongoDbUser);
        }

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
                AssignedGradeIds = mongoDbUserRoot.User.assignedgradeids
            };
        }
        private static MongoDbUser MapToMongoDbUser(UserResponse userResponse)
        {
            return new MongoDbUser()
            {
                Type = userResponse.Type,
                Name = userResponse.Name,
                Email = userResponse.Email,
                Score = userResponse.Score,
                assignedgradeids = userResponse.AssignedGradeIds
            };
        }
    }
}

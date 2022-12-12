using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Users;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;

namespace Learning_Platform_Server.DAOs
{
    public interface IUserDAO
    {
        UserResponse SignInUser(SignInRequest signInRequest);
        UserResponse GetById(string id);
        List<UserResponse> GetByGradeId(int gradeId);
        void Create(CreateUserRequest createUserRequest);
        void UpdateUser(UserResponse userResponse);
    }

    public class UserDAO : IUserDAO
    {
        private readonly HttpClient _httpClient;

        public UserDAO(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MongoDB");
        }

        public UserResponse SignInUser(SignInRequest signInRequest)
        {
            HttpRequestMessage request = new(new HttpMethod("POST"), "user/signin")
            {
                Content = JsonContent.Create(signInRequest)
            };

            HttpResponseMessage httpResponseMessage = _httpClient.SendAsync(request).Result;
            BsonArray userRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);
            if (userRootBsonArray.Count == 0)
                throw new UnauthorizedAccessException("No user was found with the given credentials");

            BsonValue userRootBsonValue = userRootBsonArray[0];
            MongoDbUserRoot mongoDbUserRoot = MapToMongoDbUserRoot(userRootBsonValue);

            UserResponse userResponse = MapToUserResponse(mongoDbUserRoot);
            return userResponse;
        }

        public UserResponse GetById(string id)
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), "user?userid=" + id);
            HttpResponseMessage httpResponseMessage = _httpClient.SendAsync(httpRequestMessage).Result;

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new MongoDbException("Database answered with statuscode " + httpResponseMessage.StatusCode + ".");

            BsonArray userRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);

            if (userRootBsonArray.Count != 0)
            {
                BsonValue userRootBsonValue = userRootBsonArray[0];
                MongoDbUserRoot mongoDbUserRoot = MapToMongoDbUserRoot(userRootBsonValue);

                UserResponse userResponse = MapToUserResponse(mongoDbUserRoot);
                return userResponse;
            }
            throw new KeyNotFoundException("Could not find any user with id " + id);
        }

        public List<UserResponse> GetByGradeId(int gradeId)
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), "wholegrade?assignedgradeid=" + gradeId);
            HttpResponseMessage httpResponseMessage = _httpClient.SendAsync(httpRequestMessage).Result;

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new MongoDbException("Database answered with statuscode " + httpResponseMessage.StatusCode + ".");

            List<UserResponse> userList = new();

            BsonArray userRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);

            foreach (BsonValue userRootBsonValue in userRootBsonArray)
            {
                MongoDbUserRoot mongoDbUserRoot = MapToMongoDbUserRoot(userRootBsonValue);

                UserResponse userResponse = MapToUserResponse(mongoDbUserRoot);
                userList.Add(userResponse);
            }

            return userList;
        }

        public void Create(CreateUserRequest createUserRequest)
        {
            MongoDbUser mongoDbUser = MapToMongoDbUser(createUserRequest);
            HttpRequestMessage request = new(new HttpMethod("POST"), "user")
            {
                Content = JsonContent.Create(mongoDbUser)
            };

            HttpResponseMessage httpResponseMessage = _httpClient.SendAsync(request).Result;

            // throws an exception if the PUT request went wrong
            ValidateMongoDbPostRequestResponse(mongoDbUser, httpResponseMessage);
        }

        public void UpdateUser(UserResponse userResponse)
        {
            MongoDbUser mongoDbUser = MapToMongoDbUser(userResponse);
            HttpRequestMessage request = new(new HttpMethod("PUT"), "user")
            {
                Content = JsonContent.Create(mongoDbUser)
            };

            HttpResponseMessage httpResponseMessage = _httpClient.SendAsync(request).Result;

            // throws an exception if the PUT request went wrong
            ValidateMongoDbPutRequestResponse(mongoDbUser, httpResponseMessage);
        }



        // helper methods

        private static void ValidateMongoDbPostRequestResponse(MongoDbUser mongoDbUser, HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new MongoDbException("Database answered with statuscode " + httpResponseMessage.StatusCode);

            string? responseString = httpResponseMessage.Content.ReadAsStringAsync().Result;
            JObject? responseJobject = (JObject?)Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);

            if (responseJobject is null)
                throw new MongoDbException("responseJobject was not found");

            if (responseJobject[MongoDbHelper.UpsertedId] is null)
                throw new UpsertedIdNotFoundException("Database did not create a new user. Maybe the email already existed. Details: " + mongoDbUser);
        }

        private static void ValidateMongoDbPutRequestResponse(MongoDbUser mongoDbUser, HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new MongoDbException("Database answered with statuscode " + httpResponseMessage.StatusCode);

            string? responseString = httpResponseMessage.Content.ReadAsStringAsync().Result;
            JObject? responseJobject = (JObject?)Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);

            if (responseJobject is null)
                throw new MongoDbException("responseJobject was not found");

            JToken? upsertedIdJToken = responseJobject[MongoDbHelper.UpsertedId];
            JToken? matchedCountJToken = responseJobject[MongoDbHelper.MatchedCount];
            JToken? modifiedCountJToken = responseJobject[MongoDbHelper.ModifiedCount];

            if (upsertedIdJToken is not null)
                throw new MongoDbException("Database inserted a new user instead of updating an existing user. Details: " + mongoDbUser);

            if (matchedCountJToken is null || modifiedCountJToken is null)
                throw new MongoDbException("Database did not return expected details. The user might not have been updated. Details: " + mongoDbUser);

            int matchedCount = int.Parse(matchedCountJToken.ToString());
            int modifiedCount = int.Parse(modifiedCountJToken.ToString());

            if (matchedCount != 1 || modifiedCount != 1)
                throw new MongoDbException("Database did not behave as expected. Details: matchedCount was " + matchedCount + " and modifiedCount was " + modifiedCount + " for the user " + mongoDbUser);
        }

        private static MongoDbUserRoot MapToMongoDbUserRoot(BsonValue userRootBsonValue)
        {
            string userRootJson = MongoDbHelper.MapToJson(userRootBsonValue);

            MongoDbUserRoot mongoDbUserRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<MongoDbUserRoot>(userRootJson) ?? throw new ArgumentNullException(nameof(userRootJson));

            Console.WriteLine("Mapping complete, found: " + mongoDbUserRoot);

            return mongoDbUserRoot;
        }

        private static UserResponse MapToUserResponse(MongoDbUserRoot mongoDbUserRoot)
        {
            MongoDbUser user = mongoDbUserRoot.User ?? throw new ArgumentException(nameof(mongoDbUserRoot.User));
            UserId userId = mongoDbUserRoot.UserId ?? throw new ArgumentException(nameof(mongoDbUserRoot.UserId));

            return new UserResponse()
            {
                UserId = userId.NumberLong,
                Type = Enum.Parse<UserType>(user.Type ?? throw new ArgumentException(nameof(user.Type))),
                Name = mongoDbUserRoot.User.Name ?? throw new ArgumentException(nameof(user.Name)),
                Email = mongoDbUserRoot.User.Email ?? throw new ArgumentException(nameof(user.Email)),
                Score = MapToScoreResponse(user.Score ?? throw new ArgumentException(nameof(user.Score))),
                AssignedGradeIds = user.AssignedGradeIds ?? throw new ArgumentException(nameof(user.AssignedGradeIds))
            };
        }

        public static ScoreResponse MapToScoreResponse(MongoDbScore mongoDbScore)
        {
            return new ScoreResponse()
            {
                A = parseStringToFloat(mongoDbScore.A),
                S = parseStringToFloat(mongoDbScore.S),
                M = parseStringToFloat(mongoDbScore.M),
                D = parseStringToFloat(mongoDbScore.D),
            };

            static float? parseStringToFloat(string? str)
                => str is not null ? float.Parse(str, CultureInfo.InvariantCulture) : null;
        }

        private static MongoDbScore MapToMongoDbScore(ScoreResponse score)
        {
            return new MongoDbScore()
            {
                A = score.A + "",
                S = score.S + "",
                M = score.M + "",
                D = score.D + "",
            };
        }

        private static MongoDbUser MapToMongoDbUser(UserResponse userResponse)
        {
            return new MongoDbUser()
            {
                Type = userResponse.Type.ToString(),
                Name = userResponse.Name ?? throw new ArgumentException(nameof(userResponse.Name)),
                Email = userResponse.Email ?? throw new ArgumentException(nameof(userResponse.Email)),
                Score = MapToMongoDbScore(userResponse.Score ?? throw new ArgumentException(nameof(userResponse.Score))),
                AssignedGradeIds = userResponse.AssignedGradeIds ?? throw new ArgumentException(nameof(userResponse.AssignedGradeIds))
            };
        }
        private static MongoDbUser MapToMongoDbUser(CreateUserRequest createUserRequest)
        {
            return new MongoDbUser()
            {
                Type = createUserRequest.Type.ToString(),
                Name = createUserRequest.Name ?? throw new ArgumentException(nameof(createUserRequest.Name)),
                Email = createUserRequest.Email ?? throw new ArgumentException(nameof(createUserRequest.Email)),
                Password = createUserRequest.Password ?? throw new ArgumentException(nameof(createUserRequest.Password)),
                Score = new MongoDbScore() { A = "1", M = "1", D = "1", S = "1" },
                AssignedGradeIds = createUserRequest.AssignedGradeIds ?? throw new ArgumentException(nameof(createUserRequest.AssignedGradeIds))
            };
        }
    }
}

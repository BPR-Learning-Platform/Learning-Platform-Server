using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Statistics;
using Learning_Platform_Server.Models.Tasks;
using Learning_Platform_Server.Models.Users;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Learning_Platform_Server.DAOs
{
    public interface IStatisticDAO
    {
        List<StatisticResponse> GetAllByParameter(int? studentId, int? gradeId);
    }

    public class StatisticDAO : IStatisticDAO
    {
        private readonly HttpClient _httpClient;

        public StatisticDAO(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MongoDB");
        }

        public List<StatisticResponse> GetAllByParameter(int? studentId, int? gradeId)
        {
            string parameterString = "";

            if (studentId.HasValue)
                parameterString = "?studentid=" + studentId;
            else if (gradeId.HasValue)
                parameterString = "?gradeid=" + gradeId;

            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), "statistic" + parameterString);
            HttpResponseMessage httpResponseMessage = _httpClient.SendAsync(httpRequestMessage).Result;

            List<StatisticResponse> statisticList = new();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new Exception("Database answered with statuscode " + httpResponseMessage.StatusCode + ".");

            BsonArray statisticRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);

            foreach (BsonValue statisticRootBsonValue in statisticRootBsonArray)
            {
                MongoDbStatisticRoot? mongoDbStatisticRoot = MapToMongoDbStatisticRoot(statisticRootBsonValue);
                if (mongoDbStatisticRoot is null)
                    break;

                StatisticResponse? statisticResponse = MapToStatisticResponse(mongoDbStatisticRoot);

                // only return valid statistics
                if (statisticResponse is not null)
                    statisticList.Add(statisticResponse);

            }

            return statisticList;
        }



        // helper methods

        private static MongoDbStatisticRoot? MapToMongoDbStatisticRoot(BsonValue statisticRootBsonValue)
        {
            string? statisticRootJson = MongoDbHelper.MapToJson(statisticRootBsonValue);

            if (statisticRootJson is null)
            {
                Console.WriteLine("statisticRootJson is null, so mapping to StatisticRoot is not completed. ");
                return null;
            }

            MongoDbStatisticRoot? mongoDbStatisticRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<MongoDbStatisticRoot>(statisticRootJson);

            return mongoDbStatisticRoot;
        }

        private static StatisticResponse? MapToStatisticResponse(MongoDbStatisticRoot mongoDbStatisticRoot)
        {
            if (mongoDbStatisticRoot.Statistic is null)
            {
                Console.WriteLine("Statistic was not found in mongoDbStatisticRoot");
                return null;
            }

            if (mongoDbStatisticRoot.Statistic.Score is null)
            {
                Console.WriteLine("Score was not found in mongoDbStatisticRoot.Statistic");
                return null;
            }

            if (mongoDbStatisticRoot.UserId is null)
            {
                Console.WriteLine("UserId was not found in mongoDbStatisticRoot");
                return null;
            }

            if (mongoDbStatisticRoot.TimeStamp is null)
            {
                Console.WriteLine("TimeStamp was not found in mongoDbStatisticRoot");
                return null;
            }

            if (mongoDbStatisticRoot.TimeStamp.DateTime is null)
            {
                Console.WriteLine("DateTime was not found in TimeStamp");
                return null;
            }


            var date = mongoDbStatisticRoot.TimeStamp.DateTime.NumberLong;

            return new StatisticResponse()
            {
                StudentId = mongoDbStatisticRoot.UserId.NumberLong,
                GradeId = mongoDbStatisticRoot.Statistic.GradeId,
                MultipleScore = UserDAO.MapToMultipleScore(mongoDbStatisticRoot.Statistic.Score),
                TimeStamp = MongoDbHelper.MapToDateTime(date)
            };
        }
    }
}

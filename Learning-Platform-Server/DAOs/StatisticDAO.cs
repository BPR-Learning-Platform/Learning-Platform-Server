using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Statistics;
using MongoDB.Bson;
using System.Net;

namespace Learning_Platform_Server.DAOs
{
    public interface IStatisticDAO
    {
        List<StatisticResponse> GetAllByParameter(int? studentId, string? gradeId, int? step);
    }

    public class StatisticDAO : IStatisticDAO
    {
        private readonly HttpClient _httpClient;

        public StatisticDAO(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MongoDB");
        }

        public List<StatisticResponse> GetAllByParameter(int? studentId, string? gradeId, int? step)
        {
            string parameterString = "";

            if (studentId.HasValue)
                parameterString = "?studentid=" + studentId;
            else if (gradeId is not null)
                parameterString = "?gradeid=" + gradeId;
            else if (step.HasValue)
                parameterString = "?step=" + step;

            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), "statistic" + parameterString);
            HttpResponseMessage httpResponseMessage = _httpClient.SendAsync(httpRequestMessage).Result;

            List<StatisticResponse> statisticList = new();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new Exception("Database answered with statuscode " + httpResponseMessage.StatusCode + ".");

            BsonArray statisticRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);

            foreach (BsonValue statisticRootBsonValue in statisticRootBsonArray)
            {
                MongoDbStatisticRoot mongoDbStatisticRoot = MapToMongoDbStatisticRoot(statisticRootBsonValue);

                StatisticResponse statisticResponse = MapToStatisticResponse(mongoDbStatisticRoot);
                statisticList.Add(statisticResponse);

            }

            return statisticList;
        }



        // helper methods

        private static MongoDbStatisticRoot MapToMongoDbStatisticRoot(BsonValue statisticRootBsonValue)
        {
            string statisticRootJson = MongoDbHelper.MapToJson(statisticRootBsonValue);

            MongoDbStatisticRoot mongoDbStatisticRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<MongoDbStatisticRoot>(statisticRootJson) ?? throw new ArgumentNullException(nameof(statisticRootJson));

            return mongoDbStatisticRoot;
        }

        private static StatisticResponse MapToStatisticResponse(MongoDbStatisticRoot mongoDbStatisticRoot)
        {
            MongoDbStatistic statistic = mongoDbStatisticRoot.Statistic ?? throw new ArgumentException(nameof(mongoDbStatisticRoot.Statistic));
            UserId userId = mongoDbStatisticRoot.UserId ?? throw new ArgumentException(nameof(mongoDbStatisticRoot.UserId));

            TimeStamp timeStamp = mongoDbStatisticRoot.TimeStamp ?? throw new ArgumentException(nameof(mongoDbStatisticRoot.UserId));
            MongoDBDate dateTime = timeStamp.DateTime ?? throw new ArgumentException(nameof(timeStamp.DateTime));
            long date = dateTime.NumberLong;

            return new StatisticResponse()
            {
                StudentId = userId.NumberLong ?? throw new ArgumentException(nameof(userId.NumberLong)),
                GradeId = statistic.GradeId ?? throw new ArgumentException(nameof(statistic.Score)),
                Score = UserDAO.MapToScoreResponse(statistic.Score ?? throw new ArgumentException(nameof(statistic.Score))),
                TimeStamp = MongoDbHelper.MapToDateTime(date)
            };
        }
    }
}

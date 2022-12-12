using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Grades;
using MongoDB.Bson;
using System.Net;

namespace Learning_Platform_Server.DAOs
{
    public interface IGradeDAO
    {
        List<GradeResponse> GetAll();
        GradeResponse GetById(int id);
    }

    public class GradeDAO : IGradeDAO
    {
        private readonly HttpClient _httpClient;

        public GradeDAO(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MongoDB");
        }

        public List<GradeResponse> GetAll()
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), "grade");
            HttpResponseMessage httpResponseMessage = _httpClient.SendAsync(httpRequestMessage).Result;

            List<GradeResponse> gradeList = new();

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new MongoDbException("Database answered with statuscode " + httpResponseMessage.StatusCode + ".");

            BsonArray gradeRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);

            foreach (BsonValue gradeRootBsonValue in gradeRootBsonArray)
            {
                MongoDbGradeRoot? mongoDbGradeRoot = MapToMongoDbGradeRoot(gradeRootBsonValue);
                if (mongoDbGradeRoot is null)
                    break;

                GradeResponse? gradeResponse = MapToGradeResponse(mongoDbGradeRoot);

                // only return valid grades
                if (gradeResponse is not null)
                    gradeList.Add(gradeResponse);

            }

            return gradeList;
        }

        public GradeResponse GetById(int id)
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), "grade?gradeid=" + id);
            HttpResponseMessage httpResponseMessage = _httpClient.SendAsync(httpRequestMessage).Result;

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new MongoDbException("Database answered with statuscode " + httpResponseMessage.StatusCode + ".");

            BsonArray gradeRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);

            if (gradeRootBsonArray.Count != 0)
            {
                BsonValue gradeRootBsonValue = gradeRootBsonArray[0];
                MongoDbGradeRoot mongoDbGradeRoot = MapToMongoDbGradeRoot(gradeRootBsonValue);

                GradeResponse gradeResponse = MapToGradeResponse(mongoDbGradeRoot);
                return gradeResponse;
            }

            throw new KeyNotFoundException("Could not find any grade with id " + id);
        }



        // helper methods

        private static MongoDbGradeRoot MapToMongoDbGradeRoot(BsonValue gradeRootBsonValue)
        {
            string gradeRootJson = MongoDbHelper.MapToJson(gradeRootBsonValue);

            MongoDbGradeRoot mongoDbGradeRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<MongoDbGradeRoot>(gradeRootJson) ?? throw new ArgumentException(nameof(gradeRootBsonValue));

            return mongoDbGradeRoot;
        }

        private static GradeResponse MapToGradeResponse(MongoDbGradeRoot mongoDbGradeRoot)
        {
            MongoDbGrade grade = mongoDbGradeRoot.Grade ?? throw new ArgumentException(nameof(mongoDbGradeRoot.Grade));
            GradeId gradeId = mongoDbGradeRoot.GradeId ?? throw new ArgumentException(nameof(mongoDbGradeRoot.GradeId));

            return new GradeResponse()
            {
                GradeId = mongoDbGradeRoot.GradeId.NumberLong ?? throw new ArgumentException(nameof(gradeId.NumberLong)),
                Step = int.Parse(grade.Step ?? throw new ArgumentException(nameof(grade.Step))),
                GradeName = grade.GradeName ?? throw new ArgumentException(nameof(grade.GradeName))
            };
        }
    }
}

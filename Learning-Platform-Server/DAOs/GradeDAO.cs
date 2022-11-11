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
        GradeResponse? GetById(int id);
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
                throw new Exception("Database answered with statuscode " + httpResponseMessage.StatusCode + ".");

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

        public GradeResponse? GetById(int id)
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), "grade?id=" + id);
            HttpResponseMessage httpResponseMessage = _httpClient.SendAsync(httpRequestMessage).Result;

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                throw new Exception("Database answered with statuscode " + httpResponseMessage.StatusCode + ".");

            BsonArray gradeRootBsonArray = MongoDbHelper.MapToBsonArray(httpResponseMessage);

            if (gradeRootBsonArray.Count != 0)
            {
                BsonValue gradeRootBsonValue = gradeRootBsonArray[0];

                MongoDbGradeRoot? mongoDbGradeRoot = MapToMongoDbGradeRoot(gradeRootBsonValue);
                if (mongoDbGradeRoot is null)
                {
                    Console.WriteLine("GradeRoot was not found");
                    return null;
                }

                GradeResponse? gradeResponse = MapToGradeResponse(mongoDbGradeRoot);

                return gradeResponse;
            }
            return null;
        }



        // helper methods

        private static MongoDbGradeRoot? MapToMongoDbGradeRoot(BsonValue gradeRootBsonValue)
        {
            string? gradeRootJson = MongoDbHelper.MapToJson(gradeRootBsonValue);

            if (gradeRootJson is null)
            {
                Console.WriteLine("gradeRootJson is null, so mapping to GradeRoot is not completed. ");
                return null;
            }

            MongoDbGradeRoot? mongoDbGradeRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<MongoDbGradeRoot>(gradeRootJson);

            return mongoDbGradeRoot;
        }

        private static GradeResponse? MapToGradeResponse(MongoDbGradeRoot mongoDbGradeRoot)
        {
            if (mongoDbGradeRoot.Grade is null)
            {
                Console.WriteLine("Grade was not found in mongoDbGradeRoot");
                return null;
            }

            if (mongoDbGradeRoot.GradeId is null)
            {
                Console.WriteLine("GradeId was not found in mongoDbGradeRoot");
                return null;
            }

            return new GradeResponse()
            {
                GradeId = mongoDbGradeRoot.GradeId.NumberLong,
                Step = mongoDbGradeRoot.Grade.Step is not null ? int.Parse(mongoDbGradeRoot.Grade.Step) : -1,
                GradeName = mongoDbGradeRoot.Grade.GradeName
            };
        }
    }
}

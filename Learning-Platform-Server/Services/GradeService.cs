using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Models.Grades;
using Learning_Platform_Server.Models.Tasks;
using Learning_Platform_Server.Models.Users;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Learning_Platform_Server.Services
{
    public interface IGradeService
    {
        GradeResponse? GetById(int id);
    }

    public class GradeService : IGradeService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/grade";


        // GET BY ID

        public GradeResponse? GetById(int id)
        {
            HttpClient httpClient = new();
            HttpRequestMessage? httpRequestMessage = new(new HttpMethod("GET"), Url + "?id=" + id);
            HttpResponseMessage? httpResponseMessage = httpClient.SendAsync(httpRequestMessage).Result;

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                return null;

            BsonArray gradeRootBsonArray = Util.MapToBsonArray(httpResponseMessage);

            if (gradeRootBsonArray.Count != 0)
            {
                BsonValue? gradeRootBsonValue = gradeRootBsonArray[0];

                MongoDbGrade? mongoDbGrade = MapToMongoDbGrade(gradeRootBsonValue);
                if (mongoDbGrade is null)
                    return null;

                GradeResponse? gradeResponse = MapToGradeResponse(mongoDbGrade);

                return gradeResponse;
            }
            return null;
        }

        private MongoDbGrade? MapToMongoDbGrade(BsonValue gradeRootBsonValue)
        {
            string? gradeRootJson = Util.MapToJson(gradeRootBsonValue);

            if (gradeRootJson is null)
            {
                Console.WriteLine("gradeRootJson is null.");
                return null;
            }

            GradeRoot? gradeRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<GradeRoot>(gradeRootJson);

            return gradeRoot?.Grade;
        }

        private static GradeResponse? MapToGradeResponse(MongoDbGrade mongoDbGrade)
        {
            return new GradeResponse()
            {
                GradeId = mongoDbGrade.GradeID,
                Step = mongoDbGrade.Step is not null ? int.Parse(mongoDbGrade.Step) : -1,
                Name = mongoDbGrade.GradeName
            };
        }


    }
}

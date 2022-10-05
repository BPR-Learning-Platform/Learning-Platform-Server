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
        List<GradeResponse>? GetAll();
        GradeResponse? GetById(int id);
    }

    public class GradeService : IGradeService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/grade";

        public List<GradeResponse>? GetAll()
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), Url);
            HttpResponseMessage httpResponseMessage = MongoDbHelper.GetHttpClient().SendAsync(httpRequestMessage).Result;

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

                // only return valid tasks
                if (gradeResponse is not null)
                    gradeList.Add(gradeResponse);

            }

            return gradeList;
        }

        public GradeResponse? GetById(int id)
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), Url + "?id=" + id);
            HttpResponseMessage httpResponseMessage = MongoDbHelper.GetHttpClient().SendAsync(httpRequestMessage).Result;

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
                Console.WriteLine("gradeRootJson is null.");
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
                Name = mongoDbGradeRoot.Grade.GradeName
            };
        }


    }
}

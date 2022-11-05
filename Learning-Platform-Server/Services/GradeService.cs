using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Helpers;
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
        List<GradeResponse> GetAll();
        List<GradeResponseToTeacher> GetAllToTeacher(string teacherId);
        GradeResponse? GetById(int id);
        int GetStep(UserResponse userResponse);
    }

    public class GradeService : IGradeService
    {

        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/grade";
        private readonly IUserService _userService;

        public GradeService(IUserService userService)
        {
            _userService = userService;
        }

        public List<GradeResponse> GetAll()
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

                // only return valid grades
                if (gradeResponse is not null)
                    gradeList.Add(gradeResponse);

            }

            return gradeList;
        }

        public List<GradeResponseToTeacher> GetAllToTeacher(string teacherId)
        {
            UserResponse? teacher = _userService.GetById(teacherId);

            if (teacher is null)
                throw new KeyNotFoundException("Could not find teacher with id " + teacherId);

            if (teacher.Type is null)
                throw new NullReferenceException("No type was found for user with id " + teacherId + ", so could not determine if the user is really a teacher. Details: " + teacher);

            if (!teacher.Type.Equals("T"))
                throw new BadHttpRequestException("The user with id " + teacherId + " has a type that does not represent a teacher. Details: " + teacher);

            if (teacher.AssignedGradeIds is null)
                throw new NullReferenceException("No assigned grade ids were found for the teacher: " + teacher);

            List<GradeResponse> gradeResponses = GetAll(); //TODO Use caching instead ?

            List<GradeResponseToTeacher> gradeResponsesToTeacher = new();

            foreach (int gradeId in teacher.AssignedGradeIds)
            {
                GradeResponse? gradeResponse = gradeResponses.FirstOrDefault(x => x.GradeId is not null && x.GradeId.Equals(gradeId + ""));

                if (gradeResponse is null)
                    throw new KeyNotFoundException("Could not find grade with grade id " + gradeId);

                string? gradeName = gradeResponse.GradeName;

                List<UserResponse> userResponseList = _userService.GetByGradeId(gradeId);
                List<UserResponseToTeacher> studentsToTeacher = new();
                foreach (UserResponse? user in userResponseList)
                {
                    if (user.Type is null)
                    {
                        Console.WriteLine("Could not read type from user: " + user + ", so the user is not included.");
                        break;
                    }

                    if (user.Type.Equals("S"))
                        studentsToTeacher.Add(new UserResponseToTeacher() { UserId = user.UserId, Name = user.Name });
                }


                gradeResponsesToTeacher.Add(new GradeResponseToTeacher() { GradeId = gradeId + "", GradeName = gradeName, Students = studentsToTeacher });
            }

            return gradeResponsesToTeacher;
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

        public int GetStep(UserResponse userResponse)
        {
            if (userResponse.AssignedGradeIds is null || userResponse.AssignedGradeIds.Count == 0)
                throw new Exception("No assignedgradeids were found for user with userid " + userResponse.UserId);

            List<GradeResponse> gradeResponseList = GetAll();

            int assignedGradeId = userResponse.AssignedGradeIds[0];

            GradeResponse? gradeResponse = gradeResponseList.FirstOrDefault(x => x.GradeId is not null && x.GradeId.Equals(assignedGradeId + ""));
            if (gradeResponse is null)
                throw new Exception("No grade was found for gradeid " + assignedGradeId);

            if (gradeResponse.Step is null)
                throw new NullReferenceException("No step was found for grade with gradeid " + assignedGradeId);

            return (int)gradeResponse.Step;
        }
    }
}

﻿using Learning_Platform_Server.Entities;
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
    public interface IStatisticService
    {
        List<StatisticResponse> GetAll(int studentId);
    }

    public class StatisticService : IStatisticService
    {
        private static readonly string Url = "https://westeurope.azure.data.mongodb-api.com/app/application-1-vuehv/endpoint/statistic";

        public List<StatisticResponse> GetAll(int studentId)
        {
            HttpRequestMessage httpRequestMessage = new(new HttpMethod("GET"), Url + "?studentid=" + studentId);
            HttpResponseMessage httpResponseMessage = MongoDbHelper.GetHttpClient().SendAsync(httpRequestMessage).Result;

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

                // only return valid tasks
                if (statisticResponse is not null)
                    statisticList.Add(statisticResponse);

            }

            return statisticList;
        }

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

            if (mongoDbStatisticRoot.StatisticId is null)
            {
                Console.WriteLine("StatisticId was not found in mongoDbStatisticRoot");
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
                StatisticId = mongoDbStatisticRoot.StatisticId.NumberLong,
                StudentId = mongoDbStatisticRoot.Statistic.StudentId,
                GradeId = mongoDbStatisticRoot.Statistic.GradeId,
                Score = mongoDbStatisticRoot.Statistic.Score,
                TimeStamp = MongoDbHelper.MapToDateTime(date)
            };
        }
    }
}
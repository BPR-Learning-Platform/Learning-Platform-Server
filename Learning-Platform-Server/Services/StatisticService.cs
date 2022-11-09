using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models;
using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Statistics;
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
        List<StatisticResponse> GetAllByStudentId(int studentId);
        List<StatisticResponse> GetAllByGradeId(int gradetId);
    }

    public class StatisticService : IStatisticService
    {
        private readonly IStatisticDAO _statisticDAO;

        public StatisticService(IStatisticDAO statisticDAO)
        {
            _statisticDAO = statisticDAO;
        }

        public List<StatisticResponse> GetAllByStudentId(int studentId)
            => _statisticDAO.GetAllByParameter(studentId, null);

        public List<StatisticResponse> GetAllByGradeId(int gradeId)
        {
            List<StatisticResponse>? statisticListForTheGrade = _statisticDAO.GetAllByParameter(null, gradeId);

            // Group by TimeStamp Date and calculate the average Score for each group
            List<StatisticResponse> statisticListWithAvgScores = statisticListForTheGrade.GroupBy(
                statisticResponse => statisticResponse.TimeStamp.Date,
                statisticResponse => statisticResponse,
                (key, val) =>

                    new StatisticResponse
                    {
                        GradeId = val.ToList()[0].GradeId,
                        MultipleScore = new MultipleScore()
                        {
                            A = val.Average(x => x.MultipleScore is not null ? x.MultipleScore.A : null),
                            M = val.Average(x => x.MultipleScore is not null ? x.MultipleScore.M : null),
                            S = val.Average(x => x.MultipleScore is not null ? x.MultipleScore.S : null),
                            D = val.Average(x => x.MultipleScore is not null ? x.MultipleScore.D : null)
                        },
                        TimeStamp = key
                    })

                    .ToList();

            return statisticListWithAvgScores;
        }
    }
}

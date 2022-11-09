using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Users;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Learning_Platform_Server.Services
{
    public interface IUserService
    {
        UserResponse SignInUser(SignInRequest signInRequest);
        UserResponse? GetById(string id);
        List<UserResponse> GetByGradeId(int gradeId);
        void Create(CreateUserRequest createUserRequest);
        UserResponse UpdateUserScore(UserResponse userResponse, int correct);
        float CalculateNewScore(float score, int correct);
    }

    public class UserService : IUserService
    {
        private readonly IUserDAO _userDAO;

        public UserService(IUserDAO userDAO)
        {
            _userDAO = userDAO;
        }

        public UserResponse SignInUser(SignInRequest signInRequest)
            => _userDAO.SignInUser(signInRequest);

        public UserResponse? GetById(string id)
            => _userDAO.GetById(id);

        public List<UserResponse> GetByGradeId(int gradeId)
            => _userDAO.GetByGradeId(gradeId);

        public void Create(CreateUserRequest createUserRequest)
            => _userDAO.Create(createUserRequest);

        public UserResponse UpdateUserScore(UserResponse userResponse, int correct)
        {
            if (userResponse.Score is null)
                throw new NullReferenceException("No score was found for the user: " + userResponse);

            userResponse.Score = CalculateNewScore((float)userResponse.Score, correct);

            // calling DAO
            _userDAO.UpdateUser(userResponse);

            return userResponse;
        }



        // helper methods

        public float CalculateNewScore(float score, int correct)
        {
            float correctNumber = 0;
            if (correct > 0)
                correctNumber = correct / 100 * Util.BatchSize;

            float incorrectNumber = Util.BatchSize - correctNumber;

            // increase/decrease score with 0.1 points for each correct/incorrect answer
            float change = (correctNumber - incorrectNumber) / 10;
            float newScore = score + change;

            //rounding the result
            newScore = (float)Math.Round((newScore), 2);

            if (newScore < 0)
                newScore = 0;
            else if (newScore > 10)
                newScore = 10;

            return newScore;
        }
    }
}

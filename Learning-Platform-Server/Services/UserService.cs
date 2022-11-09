using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Entities;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Scores;
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
        UserResponse UpdateUserScore(UserResponse userResponse, CorrectInfo? correctInfo);
        MultipleScore CalculateNewScore(MultipleScore multipleScore, CorrectInfo? correctInfo);
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

        public UserResponse UpdateUserScore(UserResponse userResponse, CorrectInfo? correctInfo)
        {
            if (userResponse.MultipleScore is null)
                throw new NullReferenceException("No score was found for the user: " + userResponse);

            userResponse.MultipleScore = CalculateNewScore(userResponse.MultipleScore, correctInfo); //TODO Calculate new score

            // calling DAO
            _userDAO.UpdateUser(userResponse);

            return userResponse;
        }



        // helper methods

        public MultipleScore CalculateNewScore(MultipleScore multipleScore, CorrectInfo? correctInfo)
        {
            int correct = 100; //TODO remove hardcode
            int score = 5;  //TODO remove hardcode

            float correctNumber = 0;
            if (correct > 0)
                correctNumber = correct / 100 * Util.BatchSize;

            float incorrectNumber = Util.BatchSize - correctNumber;

            // increase/decrease score with 0.1 points for each correct/incorrect answer
            float change = (correctNumber - incorrectNumber) / 10;
            float newScore = score + change;

            //rounding the result
            newScore = (float)Math.Round((newScore), 2);

            if (newScore < Util.MinimumScore)
                newScore = Util.MinimumScore;
            else if (newScore > Util.MaximumScore)
                newScore = Util.MinimumScore;

            //return newScore;
            return new MultipleScore() { A = GetRandomFloat(), M = GetRandomFloat(), S = GetRandomFloat(), D = GetRandomFloat() }; // TODO Remove dummy data
        }

        static float GetRandomFloat()
        {
            System.Random random = new();
            double val = (random.NextDouble() * (3 - 1) + 1);
            float randomFloat = (float)val;
            return (float)Math.Round((randomFloat), 1);
        }
    }
}

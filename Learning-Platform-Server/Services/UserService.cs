using Learning_Platform_Server.DAOs;
using Learning_Platform_Server.Helpers;
using Learning_Platform_Server.Models.Scores;
using Learning_Platform_Server.Models.Users;

namespace Learning_Platform_Server.Services
{
    public interface IUserService
    {
        UserResponse SignInUser(SignInRequest signInRequest);
        UserResponse? GetById(string id);
        List<UserResponse> GetByGradeId(int gradeId);
        void Create(CreateUserRequest createUserRequest);
        UserResponse UpdateUserScore(UserResponse userResponse, CorrectInfo correctInfo);
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

        public UserResponse UpdateUserScore(UserResponse userResponse, CorrectInfo correctInfo)
        {
            if (userResponse.Score is null)
                throw new NullReferenceException("No score was found for the user: " + userResponse);

            userResponse.Score = CalculateNewScore(userResponse.Score, correctInfo);

            // calling DAO
            _userDAO.UpdateUser(userResponse);

            return userResponse;
        }



        // helper methods

        public static ScoreResponse CalculateNewScore(ScoreResponse score, CorrectInfo correctInfo)
        {
            return new ScoreResponse()
            {
                A = CalulateNewScorePoint(score.A, correctInfo.A),
                M = CalulateNewScorePoint(score.M, correctInfo.M),
                S = CalulateNewScorePoint(score.S, correctInfo.S),
                D = CalulateNewScorePoint(score.D, correctInfo.D)
            };

            static float? CalulateNewScorePoint(float? subScore, ScorePoint? scorePoint)
            {
                if (subScore is null || scorePoint is null)
                    return Util.MinimumScore;

                if (scorePoint.Count == 0)
                    return subScore;


                float correctPercentage = 0;
                if (scorePoint.Percentage is not null)
                    correctPercentage = (int)scorePoint.Percentage;

                int correctNumber = 0;
                if (correctPercentage > 0)
                    correctNumber = (int)(correctPercentage / 100 * scorePoint.Count);

                int incorrectNumber = scorePoint.Count - correctNumber;

                float change = 0;
                // increase/decrease score with 0.1 points for each correct/incorrect answer
                if (correctNumber != incorrectNumber)
                    change = ((float)(correctNumber - incorrectNumber)) / 10;


                float newScore = (float)subScore + change;

                //rounding the result
                newScore = (float)Math.Round((newScore), 2);

                if (newScore < Util.MinimumScore)
                    newScore = Util.MinimumScore;
                else if (newScore > Util.MaximumScore)
                    newScore = Util.MinimumScore;

                return newScore;
            }
        }
    }
}

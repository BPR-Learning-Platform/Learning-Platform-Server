using FluentAssertions;
using Learning_Platform_Server.Models.Users;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Learning_Platform_Server.Tests
{
    public class UsersControllerTests
    {
        public const string Url = "/users";
        public const string SignInUrl = $"{Url}/signin";
        private readonly ITestOutputHelper _output;

        public UsersControllerTests(ITestOutputHelper output)
        {
            _output = output;
        }


        // SIGN IN

        [Fact]
        public async Task Sign_IN_existing_user_with_valid_credentials_receives_200_OK()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            JsonContent content = JsonContent.Create(new { email = "student3@student.com", password = "12345678" });

            HttpResponseMessage? httpResponseMessage = await client.PostAsync(SignInUrl, content);
            _output.WriteLine("Statuscode:" + httpResponseMessage.StatusCode);
            _output.WriteLine(content.ReadAsStringAsync().Result);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Sign_IN_non_existing_user_receives_401_Unauthorized()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            JsonContent content = JsonContent.Create(new { email = "noone@email.dk", password = "12345678" });

            HttpResponseMessage? httpResponseMessage = await client.PostAsync(SignInUrl, content);

            _output.WriteLine("Response message content: " + await httpResponseMessage.Content.ReadAsStringAsync());
            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Sign_IN_existing_user_with_wrong_password_receives_401_Unauthorized()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            JsonContent content = JsonContent.Create(new { email = "student20@student.com", password = "wrongpassword" });
            HttpResponseMessage? httpResponseMessage = await client.PostAsync(SignInUrl, content);

            _output.WriteLine("Response message content: " + await httpResponseMessage.Content.ReadAsStringAsync());
            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }


        // CREATE 

        [Fact]
        public async Task Create_new_user_receives_200_OK_and_create_same_user_again_receives_403_forbidden()
        {
            string email = GetRandomString() + "@integrationtest.com";

            HttpResponseMessage httpResponseMessage = await CreateUserAsync(email);
            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Created);

            httpResponseMessage = await CreateUserAsync(email);
            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // helper methods

        private async Task<HttpResponseMessage> CreateUserAsync(string email)
        {
            string randomString = GetRandomString();

            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            JsonContent content = JsonContent.Create(new CreateUserRequest() { Email = email, AssignedGradeIds = new() { 3 }, Name = randomString, Password = "integrationtestpassword", Type = UserType.S });

            HttpResponseMessage? httpResponseMessage = await client.PostAsync(Url, content);

            _output.WriteLine("Statuscode:" + httpResponseMessage.StatusCode);
            _output.WriteLine(content.ReadAsStringAsync().Result);

            return httpResponseMessage;
        }

        private static string GetRandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 5)
                        .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }
    }
}

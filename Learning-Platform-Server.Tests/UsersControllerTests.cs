using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Learning_Platform_Server.Tests
{
    public class UsersControllerTests
    {
        public const string SignInUrl = "/users/signin";
        private readonly ITestOutputHelper _output;

        public UsersControllerTests(ITestOutputHelper output)
        {
            this._output = output;
        }


        // SIGN IN

        [Fact]
        public async Task Sign_IN_existing_user_with_valid_credentials_receives_200_OK()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            JsonContent content = JsonContent.Create(new { email = "student3@student.com", password = "12345678" });

            HttpResponseMessage? responseMsg = await client.PostAsync(SignInUrl, content);
            _output.WriteLine("Statuscode:" + responseMsg.StatusCode);
            _output.WriteLine(content.ReadAsStringAsync().Result);

            responseMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Sign_IN_non_existing_user_receives_401_Unauthorized()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            JsonContent content = JsonContent.Create(new { email = "noone@email.dk", password = "12345678" });

            HttpResponseMessage? responseMsg = await client.PostAsync(SignInUrl, content);

            responseMsg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Sign_IN_existing_user_with_wrong_password_receives_401_Unauthorized()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            JsonContent content = JsonContent.Create(new { email = "student20@student.com", password = "wrongpassword" });
            HttpResponseMessage? responseMsg = await client.PostAsync(SignInUrl, content);

            responseMsg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}

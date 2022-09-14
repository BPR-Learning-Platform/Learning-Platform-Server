using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Learning_Platform_Server.Tests
{
    public class UsersControllerTests
    {
        private const string RequestUriSignIn = "/users/signin";
        private const string MediaType = "application/json";
        Encoding uTF8 = Encoding.UTF8;
        private readonly ITestOutputHelper output;

        public UsersControllerTests(ITestOutputHelper output)
        {
            this.output = output;
        }


        // sign IN

        [Fact]
        public async Task sign_IN_existing_user_with_valid_credentials_receives_200_OK()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            StringContent stringContent = new StringContent(@"{""Email"": ""Teacher1@Teacher.com"", ""Password"": ""12345678"" }", uTF8, MediaType);

            HttpResponseMessage? responseMsg = await client.PostAsync(RequestUriSignIn, stringContent);
            //output.WriteLine("Statuscode:" + responseMsg.StatusCode);
            //output.WriteLine(stringContent.ReadAsStringAsync().Result);

            responseMsg.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task sign_IN_non_existing_user_receives_401_Unauthorized()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            StringContent stringContent = new StringContent(@"{""Email"": ""noone@email.dk"", ""Password"": ""12345678"" }", uTF8, MediaType);
            HttpResponseMessage? responseMsg = await client.PostAsync(RequestUriSignIn, stringContent);

            responseMsg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task sign_IN_existing_user_with_wrong_password_receives_401_Unauthorized()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            StringContent stringContent = new StringContent(@"{""Email"": ""Teacher1@Teacher.com"", ""Password"": ""wrongpassword"" }", uTF8, MediaType);
            HttpResponseMessage? responseMsg = await client.PostAsync(RequestUriSignIn, stringContent);

            responseMsg.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}

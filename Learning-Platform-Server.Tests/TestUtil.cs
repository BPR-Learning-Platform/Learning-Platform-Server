using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Learning_Platform_Server.Tests
{
    internal class TestUtil
    {
        // More information on how this works: https://www.thecodebuzz.com/read-appsettings-json-in-net-core-test-project-xunit-mstest/
        private static readonly IConfiguration _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, false)
                //.AddEnvironmentVariables()
                .Build();

        public static IHttpClientFactory GetHttpClientFactoryMock()
        {
            IHttpClientFactory? httpClientFactoryMock = Substitute.For<IHttpClientFactory>();

            string? mongoDbBaseUrl = _configuration.GetConnectionString("MongoDbBaseUrl");

            HttpClient httpClient = new()
            {
                BaseAddress = new Uri(mongoDbBaseUrl)
            };

            httpClientFactoryMock.CreateClient("MongoDB").Returns(httpClient);

            return httpClientFactoryMock;
        }
    }
}

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
    public class WeatherForecastControllerTests
    {
        private readonly ITestOutputHelper output;
        private const string _endPoint = "/weatherforecast";

        public WeatherForecastControllerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task GET_returns_200_OK()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            var response = await client.GetAsync(_endPoint);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GET_returns_multiple_objects()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            HttpResponseMessage? httpResponseMessage = await client.GetAsync(_endPoint);
            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

            string? responseContentString = httpResponseMessage.Content.ReadAsStringAsync().Result;
            List<WeatherForecast> weatherForecastList = JsonConvert.DeserializeObject<List<WeatherForecast>>(responseContentString);

            weatherForecastList.Count.Should().BeGreaterThan(1);
        }

        [Fact]
        public async Task GET_returns_expected_objects()
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            HttpResponseMessage? httpResponseMessage = await client.GetAsync(_endPoint);
            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

            string? responseContentString = httpResponseMessage.Content.ReadAsStringAsync().Result;
            List<WeatherForecast> weatherForecastList = JsonConvert.DeserializeObject<List<WeatherForecast>>(responseContentString);

            WeatherForecast? weatherForecast = weatherForecastList.First();

            weatherForecast.Should().NotBeNull();
            weatherForecast.Date.Should().BeAfter(DateTime.MinValue);
            weatherForecast.TemperatureC.Should().BeGreaterThan(-100);
            weatherForecast.TemperatureF.Should().BeGreaterThan(-148);
            weatherForecast.Summary.Should().NotBeNull();
        }
    }
}

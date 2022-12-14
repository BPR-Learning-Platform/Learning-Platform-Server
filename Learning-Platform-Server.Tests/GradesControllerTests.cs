using FluentAssertions;
using Learning_Platform_Server.Models.Grades;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using Xunit.Abstractions;

namespace Learning_Platform_Server.Tests
{
    public class GradesControllerTests
    {
        private const string GradesUrl = "/grades";

        private const string TeacherId = "48";
        private const string StudentId = "124";
        private const string NonexistentId = "9999";

        private readonly ITestOutputHelper _output;



        public GradesControllerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Get_grades_for_teacher_receives_200_OK_with_multiple_objects_of_expected_type()
        {
            HttpResponseMessage httpResponseMessage = await GetHttpResponseMessageAsync(TeacherId);

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                _output.WriteLine("Response message content: " + await httpResponseMessage.Content.ReadAsStringAsync());

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

            string? responseContentString = httpResponseMessage.Content.ReadAsStringAsync().Result;
            responseContentString.Should().NotBeNullOrEmpty();
            List<GradeResponseToTeacher>? gradeResponseList = JsonConvert.DeserializeObject<List<GradeResponseToTeacher>>(responseContentString);

            gradeResponseList.Should().NotBeNullOrEmpty();

            gradeResponseList!.Count.Should().BeGreaterThan(1);
            _output.WriteLine("Number of grades deserialized: " + gradeResponseList.Count);

            int totalNumberOfStudents = 0;
            foreach (var gradeResponse in gradeResponseList)
            {
                _output.WriteLine("Found a grade: " + gradeResponse);

                // Id
                gradeResponse.GradeId.Should().NotBeNullOrEmpty();
                int.Parse(gradeResponse.GradeId!).Should().BeGreaterThan(0);

                // Name
                gradeResponse.GradeName.Should().NotBeNullOrEmpty();
                gradeResponse.GradeName!.Length.Should().BeGreaterThan(0);

                // Students
                if (gradeResponse.GradeName.Equals("1A") || gradeResponse.GradeName.Equals("1B"))
                {
                    gradeResponse.Students.Should().NotBeNullOrEmpty();
                    totalNumberOfStudents += gradeResponse.Students!.Count;
                }
                _output.WriteLine($"Number of students associated with {gradeResponse.GradeName}: {gradeResponse.Students?.Count: 0} ");
            }

            // Students: There should be student(s) in at least some of the lists
            totalNumberOfStudents.Should().BeGreaterThan(0);

        }

        [Fact]
        public async Task Get_grades_with_teacherid_set_to_be_a_studentid_receives_400_BadRequest()
        {
            HttpResponseMessage httpResponseMessage = await GetHttpResponseMessageAsync(StudentId);
            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_grades_for_nonexistent_teacher_receives_404_NotFound()
        {
            HttpResponseMessage httpResponseMessage = await GetHttpResponseMessageAsync(NonexistentId);

            _output.WriteLine("Response message content: " + await httpResponseMessage.Content.ReadAsStringAsync());
            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }



        // helper methods

        private static async Task<HttpResponseMessage> GetHttpResponseMessageAsync(string teacherId)
        {
            await using var application = new WebApplicationFactory<Program>();
            using var client = application.CreateClient();

            HttpResponseMessage? httpResponseMessage = await client.GetAsync(GradesUrl + "?teacherid=" + teacherId);
            return httpResponseMessage;
        }
    }
}

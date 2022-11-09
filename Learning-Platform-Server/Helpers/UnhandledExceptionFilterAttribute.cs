using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Learning_Platform_Server.Helpers
{
    public class UnhandledExceptionFilterAttribute : ExceptionFilterAttribute
    {
        // More information on how this works: https://nwb.one/blog/exception-filter-attribute-dotnet

        private readonly ILogger<UnhandledExceptionFilterAttribute> _logger;

        public UnhandledExceptionFilterAttribute(ILogger<UnhandledExceptionFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            int statusCode = 500;

            if (context.Exception is UnauthorizedAccessException)
                statusCode = 401;
            else if (context.Exception is KeyNotFoundException)
                statusCode = 404;
            else if (context.Exception is BadHttpRequestException)
                statusCode = 400;
            else if (context.Exception is ArgumentException)
                statusCode = 403;

            var result = new ObjectResult(new
            {
                context.Exception.Message,
                context.Exception.Source,
                ExceptionType = context.Exception.GetType().FullName,
            })
            {
                StatusCode = statusCode
            };

            // Log the exception
            _logger.LogError("Unhandled exception occurred while executing request: {ex}", context.Exception);

            // Set the result
            context.Result = result;
        }
    }
}

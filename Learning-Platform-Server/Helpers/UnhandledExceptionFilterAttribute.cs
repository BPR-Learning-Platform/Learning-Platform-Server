using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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
            int statusCode = context.Exception switch
            {
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                BadHttpRequestException => StatusCodes.Status400BadRequest,
                UpsertedIdNotFoundException => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError,
            };

            ObjectResult result = new(new
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

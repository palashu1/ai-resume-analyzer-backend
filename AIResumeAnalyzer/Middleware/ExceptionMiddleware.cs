using AIResumeAnalyzer.DTO.Utility;

namespace AIResumeAnalyzer.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var response = new ApiResponseContainer<object>
                {
                    Success = false,
                    Message = "An unexpected error occurred.",
                    Errors = new List<string>
                {
                    ex.Message
                }
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}

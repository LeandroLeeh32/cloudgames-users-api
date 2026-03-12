using System.Net;
using System.Text.Json;
using Users.Domain.Exceptions;

namespace Users.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                await HandleStatusCodes(context);
            }
            catch (DomainException ex)
            {
                await HandleExceptionAsync(
                    context,
                    HttpStatusCode.BadRequest,
                    ex.Message);
            }
            catch (Exception)
            {
                await HandleExceptionAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred.");
            }
        }

        private static async Task HandleStatusCodes(HttpContext context)
        {
            var response = context.Response;

            switch (response.StatusCode)
            {
                case (int)HttpStatusCode.Unauthorized:
                    await WriteResponse(response, 401, "Unauthorized", "Authentication required.");
                    break;

                case (int)HttpStatusCode.Forbidden:
                    await WriteResponse(response, 403, "Forbidden", "User must be Admin and active to access this resource.");
                    break;

                case (int)HttpStatusCode.NotFound:
                    await WriteResponse(response, 404, "Not Found", "Resource not found.");
                    break;

                case (int)HttpStatusCode.BadRequest:
                    await WriteResponse(response, 400, "Bad Request", "Invalid request.");
                    break;

                case (int)HttpStatusCode.Conflict:
                    await WriteResponse(response, 409, "Conflict", "Resource already exists.");
                    break;
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string message)
        {
            var response = context.Response;

            response.ContentType = "application/json";
            response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new
            {
                status = (int)statusCode,
                error = statusCode.ToString(),
                message
            });

            await response.WriteAsync(result);
        }

        private static async Task WriteResponse(
            HttpResponse response,
            int status,
            string error,
            string message)
        {
            response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(new
            {
                status,
                error,
                message
            });

            await response.WriteAsync(json);
        }
    }
}
using System.Diagnostics;

namespace Users.API.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next,ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var request = context.Request;

            _logger.LogInformation("[API][RequestLoggingMiddleware] Request iniciado: {method} {url}", request.Method, request.Path);

            try
            {
                await _next(context);

                stopwatch.Stop();

                _logger.LogInformation("[API][RequestLoggingMiddleware] Request finalizado: {method} {url} StatusCode: {statusCode} Tempo: {elapsed} ms",
                    request.Method,
                    request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    "[API][RequestLoggingMiddleware] Erro na request: {Method} {Url} Tempo: {Elapsed}ms Error: {Error}",
                    request.Method,
                    request.Path,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);

                throw;
            }
        }
    }
}

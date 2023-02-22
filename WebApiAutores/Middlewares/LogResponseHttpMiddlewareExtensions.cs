
namespace WebApiAutores.Middlewares
{
    public static class LogResponseHttpMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogResponseHttp(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LogResponseHttpMiddleware>();
        }
    }
}

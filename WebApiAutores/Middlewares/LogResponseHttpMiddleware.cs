namespace WebApiAutores.Middlewares
{
    public class LogResponseHttpMiddleware
    {
        public RequestDelegate Next { get; }
        public ILogger<LogResponseHttpMiddleware> Logger { get; }

        public LogResponseHttpMiddleware(RequestDelegate next, ILogger<LogResponseHttpMiddleware> logger)
        {
            Next = next;
            Logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (var ms = new MemoryStream())
            {
                var bodyOriginal = context.Response.Body;
                context.Response.Body = ms;

                await Next(context);

                ms.Seek(0, SeekOrigin.Begin);
                string response = new StreamReader(ms).ReadToEnd();
                ms.Seek(0, SeekOrigin.Begin);

                await ms.CopyToAsync(bodyOriginal);
                context.Response.Body = bodyOriginal;

                Logger.LogInformation(response);
            }
        }

    }
}

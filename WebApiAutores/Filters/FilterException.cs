using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiAutores.Filters
{
    public class FilterException: ExceptionFilterAttribute
    {
        private readonly ILogger<FilterException> Logger;
        public FilterException(ILogger<FilterException> logger)
        {
            Logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            Logger.LogError(context.Exception, context.Exception.Message);

            base.OnException(context);
        }

    }
}

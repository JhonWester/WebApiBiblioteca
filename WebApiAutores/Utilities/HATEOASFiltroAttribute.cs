using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiAutores.Utilities
{
    public class HATEOASFiltroAttribute: ResultFilterAttribute
    {
        protected bool DebeIncluirHATEOAS(ResultExecutingContext resultExecutingContext)
        {
            var resultado = resultExecutingContext.Result as ObjectResult;

            if (!IsSucceded(resultado))
            {
                return false;
            }

            var cabecera = resultExecutingContext.HttpContext.Request.Headers["incluirHATEOAS"];

            if (cabecera.Count == 0)
            {
                return false;
            }

            var valor = cabecera[0];

            if (!valor.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private bool IsSucceded(ObjectResult objectResult)
        {
            if (objectResult == null || objectResult.Value == null)
            {
                return false;
            }

            if (objectResult.StatusCode.HasValue && !objectResult.StatusCode.Value.ToString().StartsWith("2"))
            {
                return false;
            }

            return true;
        }
    }
}

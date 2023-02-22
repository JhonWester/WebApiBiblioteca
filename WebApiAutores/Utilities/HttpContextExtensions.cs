using Microsoft.EntityFrameworkCore;

namespace WebApiAutores.Utilities
{
    public static class HttpContextExtensions
    {
        public async static Task InsertarParametroPaginacionCabecera<T>(this HttpContext httpContext, 
            IQueryable<T> queryable)
        {
            if (httpContext == null) { throw new ArgumentNullException(nameof(httpContext)); }

            double cantidad = await queryable.CountAsync();
            httpContext.Response.Headers.Add("CantidadTotalRegistros", cantidad.ToString());
        }
    }
}

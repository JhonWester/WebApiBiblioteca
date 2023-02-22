using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using WebApiAutores.DTOs;

namespace WebApiAutores.Services
{
    public class GeneradorEnlaces
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IActionContextAccessor actionContextAccessor;

        public GeneradorEnlaces(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor,
            IActionContextAccessor actionContextAccessor)
        {
            this.authorizationService = authorizationService;
            this.httpContextAccessor = httpContextAccessor;
            this.actionContextAccessor = actionContextAccessor;
        }

        private IUrlHelper ContruirURLHelper()
        {
            var factoria = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            return factoria.GetUrlHelper(actionContextAccessor.ActionContext);
        }

        private async Task<bool> IsAdminMethod()
        {
            var httpContext = httpContextAccessor.HttpContext;
            var result =  await authorizationService.AuthorizeAsync(httpContext.User, "isAdmin");

            return result.Succeeded;
        }

        public async Task GenerarEnlaces(AutorDTO autorDTO)
        {

            var isAdmin = await IsAdminMethod();
            var Url = ContruirURLHelper();

            autorDTO.Enlaces.Add(new DatoHATEOAS(enlace: Url.Link("obtenerAutor", new { id = autorDTO.Id }),
                descripcion: "self",
                metodo: "GET"));

            if (isAdmin)
            {
                autorDTO.Enlaces.Add(new DatoHATEOAS(enlace: Url.Link("actualizarAutor", new { id = autorDTO.Id }),
                   descripcion: "actualizar-autor",
                   metodo: "PUT"));

                autorDTO.Enlaces.Add(new DatoHATEOAS(enlace: Url.Link("borrarAutor", new { id = autorDTO.Id }),
                   descripcion: "borrar-autor",
                   metodo: "DELETE"));
            }

        }
    }
}

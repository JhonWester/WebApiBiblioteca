using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RouteController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        public RouteController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "ObtenerRoot")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DatoHATEOAS>>> Get()
        {
            var datosHateoas = new List<DatoHATEOAS>();

            var isAdmin = await authorizationService.AuthorizeAsync(User, "isAdmin");

            datosHateoas.Add(new DatoHATEOAS(enlace: Url.Link("ObtenerRoot", new { }), 
                descripcion: "self", metodo: "GET"));

            datosHateoas.Add(new DatoHATEOAS(enlace: Url.Link("obtenerAutores", new { }),
                descripcion: "autores", metodo: "GET"));
            
            if (isAdmin.Succeeded)
            {
                datosHateoas.Add(new DatoHATEOAS(enlace: Url.Link("crearAutor", new { }),
                    descripcion: "autor-crear", metodo: "POST"));

                datosHateoas.Add(new DatoHATEOAS(enlace: Url.Link("crearLibro", new { }),
                    descripcion: "libro-crear", metodo: "POST"));
            }


            return datosHateoas;
        }
    }
}

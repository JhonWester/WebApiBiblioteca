using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entities;
using WebApiAutores.Utilities;
#nullable disable

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    //[Route("api/v1/autores")]
    [Route("api/autores")]
    [CabeceraEstaPresente("x-version", "1")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
    public class AutoresController : Controller
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;
        private readonly IAuthorizationService authorizationService;

        public AutoresController(ApplicationDBContext _context, IMapper mapper, IAuthorizationService authorizationService)
        {
            this.context = _context;
            this.mapper = mapper;
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "obtenerAutoresv1")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<List<AutorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Autores.AsQueryable();
            await HttpContext.InsertarParametroPaginacionCabecera(queryable);
            var autores = await queryable.OrderBy(autor => autor.Nombre).Paginar(paginacionDTO).ToListAsync();

            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpGet("{id:int}", Name = "obtenerAutorv1")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<AutorDTOConLibros>> GetAutor(int id)
        {
            var autor = await context.Autores.Include(autorDB => autorDB.AutoresLibros)
                .ThenInclude(autorLibroDB => autorLibroDB.Libro)
                .FirstOrDefaultAsync(autorDB => autorDB.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            var dto = mapper.Map<AutorDTOConLibros>(autor);

            return dto;
        }

        [HttpGet("{name}", Name = "obtenerAutorPorNombrev1")]
        public async Task<ActionResult<List<AutorDTO>>> GetAutorName(string name)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(name)).ToListAsync();

            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost(Name = "crearAutorv1")]
        public async Task<ActionResult> Post([FromBody] AutorCreationDTO autorCreation)
        {
            var existSameName = await context.Autores.AnyAsync(x => x.Nombre == autorCreation.Nombre);
            
            if (existSameName)
            {
                return BadRequest($"It already exist an Author with the name {autorCreation.Nombre}");
            }

            var autor = mapper.Map<Autor>(autorCreation);

            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("obtenerAutorv1", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarAutorv1")] // api/autores/1
        public async Task<ActionResult> Put([FromBody] AutorCreationDTO autorCreacionDTO, int id)
        {
            var exist = await context.Autores.AnyAsync(autor => autor.Id == id);

            if (!exist)
            {
                return NotFound();
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Autor Delete
        /// </summary>
        /// <param name="id">Autor Id</param>
        /// <returns></returns>
        [HttpDelete("{id:int}", Name = "borrarAutorv1")] // api/autores/2
        public async Task<ActionResult> Delete(int id)
        {
            var exist = await context.Autores.AnyAsync(autor => autor.Id == id);

            if (!exist)
            {
                return NotFound();
            }

            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}

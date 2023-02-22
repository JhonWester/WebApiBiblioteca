using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entities;
using WebApiAutores.Utilities;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/libros/{libroId:int}/comentarios")]
    public class ComentariosController : Controller
    {
        private readonly ApplicationDBContext context;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;

        public ComentariosController(ApplicationDBContext context, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [HttpGet(Name = "obtenerComentariosLibro")]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId, [FromQuery] PaginacionDTO paginacionDTO)
        {
            var exist = await context.Libros.AnyAsync(libro => libro.Id == libroId);

            if (!exist)
            {
                return NotFound();
            }

            var queryable = context.Comentarios.Where(c => c.Id == libroId).AsQueryable();
            await HttpContext.InsertarParametroPaginacionCabecera(queryable);

            var comentarios = await queryable.OrderBy(comentario => comentario.Id).Paginar(paginacionDTO).ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);
        }

        [HttpGet("{id:int}", Name = "obtenerComentario")]
        public async Task<ActionResult<ComentarioDTO>> GetPorID(int id)
        {
            var comentario = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);

            if (comentario == null)
            {
                return NotFound();
            }

            return mapper.Map<ComentarioDTO>(comentario);
        }

        [HttpPut("{id:int}", Name = "actualizarComentario")]
        public async Task<ActionResult> Put(int libroId, int id, [FromBody]ComentarioCreacionDTO comentarioDTO)
        {
            var existLibro = await context.Libros.AnyAsync(libro => libro.Id == libroId);

            if (!existLibro)
            {
                return NotFound();
            }

            var exist = await context.Comentarios.AnyAsync(coment => coment.Id == id);

            if (!exist)
            {
                return NotFound();
            }

            var comentario = mapper.Map<Comentario>(comentarioDTO);
            comentario.Id = id;
            comentario.LibroId = libroId;

            context.Update(comentario);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost(Name = "crearComentario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacion)
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;
            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;
            var exist = await context.Libros.AnyAsync(libro => libro.Id == libroId);

            if (!exist)
            {
                return NotFound();
            }

            var comentario = mapper.Map<Comentario>(comentarioCreacion);
            comentario.LibroId = libroId;
            comentario.UsuarioId = usuarioId;
            context.Add(comentario);
            await context.SaveChangesAsync();

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);

            return CreatedAtRoute("obtenerComentario", new { id = comentario.Id, libroId = libroId }, comentarioDTO);
        }
    }
}

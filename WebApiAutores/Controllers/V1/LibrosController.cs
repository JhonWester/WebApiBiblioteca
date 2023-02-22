using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entities;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/libros")]
    public class LibrosController : Controller
    {
        public readonly ApplicationDBContext context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDBContext _context, IMapper mapper)
        {
            this.context = _context;
            this.mapper = mapper;
        }

        [HttpGet("{id:int}", Name = "obtenerLibro")]
        public async Task<ActionResult<LibroDTOConAutores>> Get(int id)
        {
            var libro = await context.Libros
                .Include(libroBD => libroBD.AutoresLibros)
                .ThenInclude(AutorLibroDB => AutorLibroDB.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libro == null) { return NotFound(); }

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();
            return mapper.Map<LibroDTOConAutores>(libro);
        }

        [HttpPost(Name = "crearLibro")]
        public async Task<ActionResult> Post(LibroCreationDTO libroDTO)
        {
            if (libroDTO.AutoresIds == null)
            {
                return BadRequest("No se puede crear un libro sin autores");
            }

            var autoresIds = await context.Autores.Where(autorDB => libroDTO.AutoresIds.Contains(autorDB.Id)).Select(x => x.Id).ToListAsync();

            if (libroDTO.AutoresIds.Count != autoresIds.Count)
            {
                return BadRequest("No existe uno de los autores enviados");
            }
            var libro = mapper.Map<Libro>(libroDTO);

            asignarOrden(libro);

            context.Add(mapper.Map<Libro>(libro));
            await context.SaveChangesAsync();

            var libroDTOMap = mapper.Map<LibroDTO>(libro);

            return CreatedAtRoute("obtenerLibro", new { id = libro.Id }, libroDTOMap);
        }

        [HttpPut("{id:int}", Name = "actualizarLibro")]
        public async Task<ActionResult> Put(int id, [FromBody]LibroCreationDTO libroCreacionDTO)
        {
            var libroDB = await context.Libros.Include(x => x.AutoresLibros)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (libroDB == null) { return NotFound(); }

            libroDB = mapper.Map(libroCreacionDTO, libroDB);

            asignarOrden(libroDB);

            await context.SaveChangesAsync();
            return NoContent();
        }

        private void asignarOrden(Libro libro)
        {
            if (libro != null)
            {
                for (var i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }

        [HttpPatch("{id:int}", Name = "patchLibro")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> patchDocument)
        {
            if (patchDocument == null) { return BadRequest(); }

            var libroDB = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null) { return NotFound(); }

            var libroDTO = mapper.Map<LibroPatchDTO>(libroDB);

            patchDocument.ApplyTo(libroDTO, ModelState);

            var isValid = TryValidateModel(libroDTO);

            if (!isValid)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(libroDTO, libroDB);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "borrarLibro")] // api/libro/2
        public async Task<ActionResult> Delete(int id)
        {
            var exist = await context.Libros.AnyAsync(libroDB => libroDB.Id == id);

            if (!exist)
            {
                return NotFound();
            }

            context.Remove(new Libro() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}

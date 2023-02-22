using AutoMapper;
using WebApiAutores.DTOs;
using WebApiAutores.Entities;

namespace WebApiAutores.Utilities
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AutorCreationDTO, Autor>();

            CreateMap<Autor, AutorDTO>();

            CreateMap<Autor, AutorDTOConLibros>()
                .ForMember(Autor => Autor.Libros, opciones => opciones.MapFrom(MapAutorDTOLibros));

            CreateMap<LibroCreationDTO, Libro>()
                .ForMember(libro => libro.AutoresLibros, opciones => opciones.MapFrom(MapAutoresLibros));

            CreateMap<Libro, LibroDTO>();
            CreateMap<Libro, LibroDTOConAutores>()
                .ForMember(libroDTO => libroDTO.Autores, opciones => opciones.MapFrom(MapLibroDTOAutores));
            CreateMap<LibroPatchDTO, Libro>().ReverseMap();

            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>();
        }

        private List<LibroDTO> MapAutorDTOLibros(Autor autor, AutorDTO autorDTO)
        {
            var resultado = new List<LibroDTO>();

            if (autor.AutoresLibros == null) { return resultado; }

            foreach (var autorLibro in autor.AutoresLibros)
            {
                resultado.Add(new LibroDTO { Id = autorLibro.LibroId, Titulo = autorLibro.Libro.Titulo });
            }

            return resultado;
        }

        private List<AutorDTO> MapLibroDTOAutores(Libro libro, LibroDTO libroDTO)
        {
            var resultado = new List<AutorDTO>();

            if (libro.AutoresLibros == null) { return resultado; }

            foreach (var autorLibro in libro.AutoresLibros)
            {
                resultado.Add(new AutorDTO { Id = autorLibro.AutorId, Nombre = autorLibro.Autor.Nombre });
            }
            return resultado;
        }

        private List<AutorLibro> MapAutoresLibros(LibroCreationDTO libroDTO, Libro libro)
        {
            List<AutorLibro> autorLibro = new List<AutorLibro>();

            if (libroDTO.AutoresIds == null) { return autorLibro; }

            foreach (var id in libroDTO.AutoresIds)
            {
                autorLibro.Add(new AutorLibro() { AutorId = id });
            }

            return autorLibro;
        }
    }
}

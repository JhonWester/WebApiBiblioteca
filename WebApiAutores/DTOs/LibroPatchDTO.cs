using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validations;

namespace WebApiAutores.DTOs
{
    public class LibroPatchDTO
    {
        [FirstUppercaseAttribute]
        [StringLength(maximumLength: 250)]
        [Required]
        public string Titulo { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}

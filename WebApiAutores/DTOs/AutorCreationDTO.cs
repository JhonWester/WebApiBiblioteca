using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validations;

namespace WebApiAutores.DTOs
{
    public class AutorCreationDTO
    {
        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(maximumLength: 120, ErrorMessage = "The {0} field has min {1} character")]
        [FirstUppercase]
        public string Nombre { get; set; }
    }
}

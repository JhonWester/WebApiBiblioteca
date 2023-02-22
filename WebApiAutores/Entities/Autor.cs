using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApiAutores.Validations;

namespace WebApiAutores.Entities
{
    public class Autor
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(maximumLength: 120, ErrorMessage = "The {0} field has min {1} character")]
        [FirstUppercase]
        public string Nombre { get; set; }
        public List<AutorLibro> AutoresLibros { get; set; }

    }
}

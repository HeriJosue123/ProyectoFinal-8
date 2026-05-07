using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactManagerWeb.Models
{
    public class Correo
    {
        [Key]
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public int ContactoId { get; set; }
        [ForeignKey("ContactoId")]
        public virtual Contacto? Contacto { get; set; }
    }
}
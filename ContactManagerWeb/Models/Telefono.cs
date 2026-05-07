using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactManagerWeb.Models
{
    public class Telefono
    {
        [Key]
        public int Id { get; set; }

        public string Numero { get; set; } = string.Empty;

        // Así lo enlazamos al contacto dueño de este teléfono
        public int ContactoId { get; set; }
        [ForeignKey("ContactoId")]
        public virtual Contacto? Contacto { get; set; }
    }
}
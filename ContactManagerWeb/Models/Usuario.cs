using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ContactManagerWeb.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        // Un usuario tiene muchos contactos en su agenda
        public virtual ICollection<Contacto> Contactos { get; set; } = new List<Contacto>();
    }
}
using System;
using System.Collections.Generic; 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ContactManagerWeb.Models
{
    public class Contacto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Apellido (Opcional)")]
        public string? Apellido { get; set; }

        [Display(Name = "¿Es Favorito?")]
        public bool EsFavorito { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // 1. Relación con Categoría (Reemplaza al string Categoria)
        public int? CategoriaId { get; set; }
        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        // 2. Relación con Usuario (Para hacerlo escalable)
        public int? UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        // 3. Relaciones de 1 a Muchos (Un contacto puede tener VARIOS de estos)
        public virtual ICollection<Telefono> Telefonos { get; set; } = new List<Telefono>();
        public virtual ICollection<Correo> Correos { get; set; } = new List<Correo>();
        public virtual ICollection<Direccion> Direcciones { get; set; } = new List<Direccion>();
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace ContactManagerWeb.Models
{
    public class Contacto
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Apellido (Opcional)")]
        public string? Apellido { get; set; }

        [Display(Name = "Número de Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [Display(Name = "Correo Electrónico")]
        public string? Correo { get; set; }

        [Display(Name = "Dirección Física")]
        public string? Direccion { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; } = string.Empty;

        [Display(Name = "¿Es Favorito?")]
        public bool EsFavorito { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
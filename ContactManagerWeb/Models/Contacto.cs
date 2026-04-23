using System;
using System.ComponentModel.DataAnnotations;

namespace ContactManagerWeb.Models
{
    public class Contacto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Formato de teléfono no válido")]
        [Display(Name = "Número de Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido")]
        [StringLength(150)]
        public string? Correo { get; set; }


        
        [Display(Name = "Dirección Física")]
        [StringLength(200)]
        public string? Direccion { get; set; }

        
        [Required(ErrorMessage = "Debe seleccionar una categoría")]
        public string Categoria { get; set; } = string.Empty;

        [Display(Name = "¿Es Favorito?")]
        public bool EsFavorito { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
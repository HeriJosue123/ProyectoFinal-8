using System;
using System.ComponentModel.DataAnnotations;

namespace ContactManagerWeb.Models
{
    public class Contacto
    {
        [Key]
        public int Id { get; set; }

        // SIN el "?". Es obligatorio siempre. Lo inicializamos vacío.
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        // SIN el "?". Obligatorio siempre.
        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Formato de teléfono no válido")]
        [Display(Name = "Número de Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        // El correo sí puede tener "?" porque a veces la gente no tiene correo
        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido")]
        [StringLength(150)]
        public string? Correo { get; set; }

        // La dirección también puede ser opcional
        [Display(Name = "Dirección Física")]
        [StringLength(200)]
        public string? Direccion { get; set; }

        // SIN el "?". Toda persona debe pertenecer a un grupo.
        [Required(ErrorMessage = "Debe seleccionar una categoría")]
        public string Categoria { get; set; } = string.Empty;

        [Display(Name = "¿Es Favorito?")]
        public bool EsFavorito { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
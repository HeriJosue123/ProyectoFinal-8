using System;
using System.ComponentModel.DataAnnotations;

namespace ContactManagerWeb.Models
{
    public class Contacto
    {
        [Key]
        public int Id { get; set; }

        // Aquí agregas el Required con el mensaje en español
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Apellido (Opcional)")]
        public string? Apellido { get; set; }

        // Lo mismo para el teléfono
        [Required(ErrorMessage = "El número de teléfono es requerido")]
        [RegularExpression(@"^\+503 \d{4}-\d{4}$", ErrorMessage = "El formato debe ser +503 XXXX-XXXX")]
        [Display(Name = "Número de Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        // Para el correo, usamos EmailAddress para que valide el @
        [EmailAddress(ErrorMessage = "Ingresa una dirección de correo válida")]
        [Display(Name = "Correo Electrónico")]
        public string? Correo { get; set; }

        [Display(Name = "Dirección Física")]
        public string? Direccion { get; set; }

        // Importante para el Select de categorías
        [Required(ErrorMessage = "Debes seleccionar una categoría")]
        [Display(Name = "Categoría")]
        public string Categoria { get; set; } = "General";

        [Display(Name = "¿Es Favorito?")]
        public bool EsFavorito { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
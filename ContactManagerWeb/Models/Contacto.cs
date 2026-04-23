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
        [RegularExpression(@"^.*[a-zA-ZáéíóúñÁÉÍÓÚÑ].*$", ErrorMessage = "El nombre no puede contener solo números, debe incluir letras.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100)]
        [RegularExpression(@"^.*[a-zA-ZáéíóúñÁÉÍÓÚÑ].*$", ErrorMessage = "El apellido no puede contener solo números, debe incluir letras.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Display(Name = "Número de Teléfono")]
        // Expresión regular que exige exactamente el formato +503 XXXX-XXXX
        [RegularExpression(@"^\+503 \d{4}-\d{4}$", ErrorMessage = "El teléfono debe tener el formato +503 XXXX-XXXX con 8 dígitos.")]
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
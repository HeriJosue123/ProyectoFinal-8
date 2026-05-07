using System;
using System.ComponentModel.DataAnnotations;

namespace ContactManagerWeb.ViewModels
{
    // Esta clase es SOLO para transportar datos desde tu formulario HTML hacia el backend
    public class ContactoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Apellido (Opcional)")]
        public string? Apellido { get; set; }

        [Required(ErrorMessage = "El número de teléfono es requerido")]
        [Display(Name = "Número de Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Dirección de correo inválida: debe llevar obligatoriamente una '@' y un dominio válido.")]
        [Display(Name = "Correo Electrónico")]
        public string? Correo { get; set; }

        [Display(Name = "Dirección Física")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "Debes seleccionar una categoría")]
        [Display(Name = "Categoría")]
        public string Categoria { get; set; } = "General";

        [Display(Name = "¿Es Favorito?")]
        public bool EsFavorito { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Teléfono Secundario (Opcional)")]
        [RegularExpression(@"^\+503\s\d{4}-\d{4}$", ErrorMessage = "Formato inválido. Ej: +503 1234-5678")]
        public string? TelefonoSecundario { get; set; }

        [Display(Name = "Correo Secundario (Opcional)")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string? CorreoSecundario { get; set; }
    }
}
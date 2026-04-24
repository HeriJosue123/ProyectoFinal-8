using System.ComponentModel.DataAnnotations;

namespace ContactManagerWeb.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [StringLength(50)]
        public string? NombreCategoria { get; set; }

        [StringLength(200)]
        public string? Descripcion { get; set; }

        public string? ColorHexadecimal { get; set; }

        // Agregamos la propiedad para el Emoji de la interfaz
        public string? Emoji { get; set; }
    }
}
namespace ContactManagerWeb.Models
{
    public class Contacto
    {
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Direccion { get; set; }
        public string Categoria { get; set; }
        public bool EsFavorito { get; set; }
    }
}
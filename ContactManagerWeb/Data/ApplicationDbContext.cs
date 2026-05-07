using ContactManagerWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactManagerWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Contacto> Contactos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Telefono> Telefonos { get; set; }
        public DbSet<Correo> Correos { get; set; }
        public DbSet<Direccion> Direcciones { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}
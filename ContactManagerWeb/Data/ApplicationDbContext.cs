using ContactManagerWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ContactManagerWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Estas líneas conectan tus modelos con las tablas en SQL Server
        public DbSet<Contacto> Contactos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
    }
}
using ContactManagerWeb.Data;
using ContactManagerWeb.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContactManagerWeb.Services
{
    public class CategoriaService
    {
        private readonly ApplicationDbContext _context;

        public CategoriaService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- REGLA 1: Límite de 10 categorías ---
        public async Task ValidarLimiteCategorias()
        {
            var total = await _context.Categorias.CountAsync();
            if (total >= 10)
            {
                throw new Exception("Límite alcanzado: Solo puedes tener un máximo de 10 categorías. Edita o elimina una existente para agregar otra.");
            }
        }

        // --- REGLA 2 y 3: Proteger "General" y Reasignar contactos al eliminar ---
        public async Task EliminarYReasignar(int idCategoria)
        {
            var categoria = await _context.Categorias.FindAsync(idCategoria);
            if (categoria == null) throw new Exception("La categoría no existe.");

            // REGLA: No permitir borrar la categoría "General"
            if (categoria.NombreCategoria.Trim().ToLower() == "general")
            {
                throw new Exception("La categoría 'General' es la predeterminada del sistema y no puede ser eliminada.");
            }

            // Buscar todos los contactos que están en la categoría que vamos a borrar
            var contactosAfectados = await _context.Contactos
                .Where(c => c.Categoria == categoria.NombreCategoria)
                .ToListAsync();

            // Garantizar que la categoría "General" exista en la base de datos
            bool existeGeneral = await _context.Categorias.AnyAsync(c => c.NombreCategoria == "General");
            if (!existeGeneral)
            {
                _context.Categorias.Add(new Categoria { NombreCategoria = "General", Emoji = "📁" });
                await _context.SaveChangesAsync();
            }

            // REGLA: Reasignar los contactos afectados a "General"
            foreach (var contacto in contactosAfectados)
            {
                contacto.Categoria = "General";
                _context.Update(contacto);
            }

            // Finalmente, eliminamos la categoría original
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }
    }
}
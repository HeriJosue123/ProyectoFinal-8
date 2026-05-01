using ContactManagerWeb.Data;
using ContactManagerWeb.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
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

        // --- REGLA: Normalización y Validación de Nombre ---
        public void ValidarYNormalizar(Categoria categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria.NombreCategoria))
            {
                throw new Exception("El nombre de la categoría es obligatorio.");
            }

            var textInfo = new CultureInfo("es-ES", false).TextInfo;
            string nombreLimpio = textInfo.ToTitleCase(categoria.NombreCategoria.Trim().ToLower());

            // REGLA DE NEGOCIO: Evitar categorías duplicadas
            bool existe = _context.Categorias.Any(c => c.NombreCategoria.ToLower() == nombreLimpio.ToLower() && c.Id != categoria.Id);

            if (existe)
            {
                throw new Exception($"La categoría '{nombreLimpio}' ya existe en tu agenda.");
            }

            categoria.NombreCategoria = nombreLimpio;

            if (string.IsNullOrWhiteSpace(categoria.Emoji))
            {
                categoria.Emoji = "📁";
            }
        }

        public async Task ValidarLimiteCategorias()
        {
            var total = await _context.Categorias.CountAsync();
            if (total >= 10)
            {
                throw new Exception("Límite alcanzado: Solo puedes tener un máximo de 10 categorías. Edita o elimina una existente para agregar otra.");
            }
        }

        public async Task EliminarYReasignar(int idCategoria)
        {
            var categoria = await _context.Categorias.FindAsync(idCategoria);
            if (categoria == null) throw new Exception("La categoría no existe.");

            if (categoria.NombreCategoria.Trim().ToLower() == "general")
            {
                throw new Exception("La categoría 'General' es la predeterminada del sistema y no puede ser eliminada.");
            }

            var contactosAfectados = await _context.Contactos
                .Where(c => c.Categoria == categoria.NombreCategoria)
                .ToListAsync();

            bool existeGeneral = await _context.Categorias.AnyAsync(c => c.NombreCategoria == "General");
            if (!existeGeneral)
            {
                _context.Categorias.Add(new Categoria { NombreCategoria = "General", Emoji = "📁" });
                await _context.SaveChangesAsync();
            }

            foreach (var contacto in contactosAfectados)
            {
                contacto.Categoria = "General";
                _context.Update(contacto);
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }
    }
}
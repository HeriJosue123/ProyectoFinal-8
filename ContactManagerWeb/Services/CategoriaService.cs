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
            var categoriaABorrar = await _context.Categorias.FindAsync(idCategoria);
            if (categoriaABorrar == null) throw new Exception("La categoría no existe.");

            // 1. Buscamos los contactos comparando el ID (Mucho más rápido y seguro)
            var contactosAfectados = await _context.Contactos
                .Where(c => c.CategoriaId == idCategoria)
                .ToListAsync();

            // 2. Buscamos o creamos la categoría "General" (el objeto completo)
            var catGeneral = await _context.Categorias
                .FirstOrDefaultAsync(c => c.NombreCategoria == "General");

            if (catGeneral == null)
            {
                catGeneral = new Categoria { NombreCategoria = "General", Emoji = "📁" };
                _context.Categorias.Add(catGeneral);
                await _context.SaveChangesAsync();
            }

            // 3. Reasignamos el OBJETO 'catGeneral' a cada contacto
            foreach (var contacto in contactosAfectados)
            {
                contacto.Categoria = catGeneral; 
                _context.Update(contacto);
            }

            // 4. Finalmente, borramos la categoría vieja
            _context.Categorias.Remove(categoriaABorrar);
            await _context.SaveChangesAsync();
        }
    }
}
using ContactManagerWeb.Data;
using ContactManagerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContactManagerWeb.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listado de todas las categorías
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categorias.ToListAsync());
        }

        // Formulario para crear categoría
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // Formulario para editar categoría
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Muestra la confirmación de eliminación (Aquí estaba el error)
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoria == null) return NotFound();

            // Enviamos el objeto 'categoria' a la vista
            return View(categoria);
        }

        // POST: Ejecuta la eliminación
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria != null)
            {
                try
                {
                    _context.Categorias.Remove(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Error común: No se puede borrar si hay contactos usando esta categoría
                    TempData["Error"] = "No se puede eliminar esta categoría porque tiene contactos asociados.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
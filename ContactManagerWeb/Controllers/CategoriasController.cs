using ContactManagerWeb.Data;
using ContactManagerWeb.Models;
using ContactManagerWeb.Services;
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
        private readonly CategoriaService _categoriaService;

        public CategoriasController(ApplicationDbContext context, CategoriaService categoriaService)
        {
            _context = context;
            _categoriaService = categoriaService;
        }

        // --- 1. LISTADO DE CATEGORÍAS ---
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categorias.ToListAsync());
        }

        // --- 2. DETALLES ---
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // --- 3. CREAR CATEGORÍA ---
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
                try
                {
                    // REGLA: Normalizar nombre antes de cualquier validación
                    _categoriaService.ValidarYNormalizar(categoria);

                    // REGLA DE NEGOCIO: Validar límite de 10 categorías
                    await _categoriaService.ValidarLimiteCategorias();

                    _context.Add(categoria);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Contactos", new { categoria = categoria.NombreCategoria });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(categoria);
        }

        // --- 4. EDITAR CATEGORÍA ---
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            // REGLA DE NEGOCIO: Impedir entrar a editar "General"
            if (categoria.NombreCategoria == "General")
            {
                TempData["Error"] = "La categoría 'General' es del sistema y no puede ser modificada.";
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // REGLA: Normalizar nombre antes de guardar
                    _categoriaService.ValidarYNormalizar(categoria);

                    // REGLA DE NEGOCIO: Proteger el nombre "General"
                    if (categoria.NombreCategoria == "General")
                    {
                        ModelState.AddModelError(string.Empty, "No puedes renombrar una categoría como 'General'.");
                        return View(categoria);
                    }

                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(categoria);
        }

        // --- 5. ELIMINAR CATEGORÍA ---
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null) return NotFound();

            // REGLA DE NEGOCIO: Impedir borrar "General"
            if (categoria.NombreCategoria == "General")
            {
                TempData["Error"] = "Operación prohibida: 'General' es la categoría raíz.";
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            try
            {
                // REGLA DE NEGOCIO: Reasignar contactos a General y eliminar
                await _categoriaService.EliminarYReasignar(id);
                TempData["Success"] = "Categoría eliminada. Los contactos asociados se movieron a 'General'.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
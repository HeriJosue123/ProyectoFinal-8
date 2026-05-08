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
                    _categoriaService.ValidarYNormalizar(categoria);
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

        // --- 4. EDITAR CATEGORÍA (Arreglado para recibir volverA) ---
        public async Task<IActionResult> Editar(int? id, string volverA)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            if (categoria.NombreCategoria == "General")
            {
                TempData["Error"] = "La categoría 'General' es del sistema y no puede ser modificada.";
                return RedirectToAction(nameof(Index));
            }

            // Pasamos el origen a la vista
            ViewBag.VolverA = volverA;
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Categoria categoria, string? volverA) // Agregamos el ? para que sea opcional
        {
            if (id != categoria.Id) return NotFound();

            // Eliminamos la validación de volverA del ModelState para que no bloquee el guardado
            ModelState.Remove("volverA");

            if (ModelState.IsValid)
            {
                try
                {
                    // REGLA: Normalizar nombre antes de guardar
                    _categoriaService.ValidarYNormalizar(categoria);

                    // REGLA DE NEGOCIO: Proteger el nombre "General"
                    if (categoria.NombreCategoria.ToLower() == "general")
                    {
                        ModelState.AddModelError(string.Empty, "No puedes renombrar una categoría como 'General'.");
                        ViewBag.VolverA = volverA;
                        return View(categoria);
                    }

                    _context.Update(categoria);
                    await _context.SaveChangesAsync();

                    // Redirección inteligente
                    if (volverA == "Contactos")
                    {
                        return RedirectToAction("Index", "Contactos");
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Si el servicio lanza un error (ej. nombre duplicado), lo mostramos en pantalla
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            // Si llegamos aquí es porque algo falló, recargamos la vista con los errores
            ViewBag.VolverA = volverA;
            return View(categoria);
        }

        // --- 5. ELIMINAR CATEGORÍA ---
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null) return NotFound();

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
using ContactManagerWeb.Data;
using ContactManagerWeb.Models;
using ContactManagerWeb.Services; // ¡Importante para usar la Lógica de Negocio!
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
        private readonly CategoriaService _categoriaService; // Agregamos el servicio

        // Inyectamos ambos: el contexto de la base de datos y nuestro servicio de BLL
        public CategoriasController(ApplicationDbContext context, CategoriaService categoriaService)
        {
            _context = context;
            _categoriaService = categoriaService;
        }

        // LISTADO DE CATEGORÍAS
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categorias.ToListAsync());
        }


        // DETALLES DE UNA CATEGORÍA

        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // CREAR CATEGORÍA
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
                    // --- REGLA DE NEGOCIO: Validar límite máximo de 10 categorías ---
                    await _categoriaService.ValidarLimiteCategorias();

                    _context.Add(categoria);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Si tira error (ej. hay 10 categorías), bloquea el guardado y avisa a la vista
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(categoria);
        }

        // EDITAR CATEGORÍA
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
                try
                {
                    // --- REGLA DE NEGOCIO: Proteger "General" de ser editada ---
                    if (categoria.NombreCategoria.Trim().ToLower() == "general")
                    {
                        ModelState.AddModelError(string.Empty, "La categoría 'General' está reservada por el sistema y no puede ser modificada.");
                        return View(categoria);
                    }

                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categorias.Any(e => e.Id == categoria.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(categoria);
        }

        // ELIMINAR CATEGORÍA

        // GET: Confirmación
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // POST: Ejecución
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            try
            {
                // --- REGLA DE NEGOCIO: Reasignar contactos a "General" y eliminar la categoría ---
                // Toda la validación pesada ocurre en el Service (capa BLL)
                await _categoriaService.EliminarYReasignar(id);

                TempData["Success"] = "Categoría eliminada correctamente. Los contactos asociados se movieron a la categoría 'General'.";
            }
            catch (Exception ex)
            {
                // Si intenta borrar "General" u ocurre otro error de lógica
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
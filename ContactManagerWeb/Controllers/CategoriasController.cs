using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactManagerWeb.Data;
using ContactManagerWeb.Models;
using System.Threading.Tasks;
using System.Linq;

namespace ContactManagerWeb.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. LISTADO (OPCIONAL) ---
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categorias.ToListAsync());
        }

        // --- 2. DETALLES DE CATEGORÍA ---
        // Se activa al dar clic en "👀 Ver Detalles"
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // --- 3. CREAR CATEGORÍA (GET) ---
        public IActionResult Crear()
        {
            return View();
        }

        // --- 4. CREAR CATEGORÍA (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Contactos");
            }
            return View(categoria);
        }

        // --- 5. EDITAR CATEGORÍA (GET) ---
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // --- 6. EDITAR CATEGORÍA (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Index", "Contactos");
            }
            return View(categoria);
        }

        // --- 7. ELIMINAR CATEGORÍA (GET) ---
        // ¡ESTE ES EL QUE CARGA LA VISTA BONITA DE CONFIRMACIÓN!
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // --- 8. ELIMINAR CATEGORÍA (POST) ---
        // Se activa cuando confirmas en la vista anterior
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Contactos");
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.Id == id);
        }
    }
}
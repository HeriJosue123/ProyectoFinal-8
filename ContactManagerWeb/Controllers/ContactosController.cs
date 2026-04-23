using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactManagerWeb.Models;
using ContactManagerWeb.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ContactManagerWeb.Controllers
{
    public class ContactosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. LISTADO (INDEX) CON FILTROS AVANZADOS ---
        public async Task<IActionResult> Index(string searchString, string categoria, bool? soloFavoritos)
        {
            // PRO: Usamos IQueryable para filtrar en SQL y no en la memoria del servidor
            IQueryable<Contacto> query = _context.Contactos;

            // Filtro por Nombre/Apellido
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Nombre.Contains(searchString) || s.Apellido.Contains(searchString));
            }

            // Filtro por Categoría
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(c => c.Categoria == categoria);
                ViewData["FiltroActual"] = $"Categoría: {categoria}";
            }

            // Filtro Favoritos
            if (soloFavoritos == true)
            {
                query = query.Where(c => c.EsFavorito);
                ViewData["FiltroActual"] = "Mis Favoritos ⭐";
            }

            // PRO: Contadores optimizados (Se ejecutan en paralelo o de forma eficiente en la DB)
            var listaCompleta = await _context.Contactos.ToListAsync();
            ViewBag.TotalTodos = listaCompleta.Count;
            ViewBag.TotalFavoritos = listaCompleta.Count(c => c.EsFavorito);
            ViewBag.TotalTrabajo = listaCompleta.Count(c => c.Categoria == "Trabajo");
            ViewBag.TotalAmigos = listaCompleta.Count(c => c.Categoria == "Amigos");
            ViewBag.TotalFamilia = listaCompleta.Count(c => c.Categoria == "Familia");

            return View(await query.ToListAsync());
        }

        // --- 2. DETALLES ---
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();

            var contacto = await _context.Contactos.FirstOrDefaultAsync(m => m.Id == id);
            if (contacto == null) return NotFound();

            return View(contacto);
        }

        // --- 3. CREAR ---
        public IActionResult Crear() => View();

        [HttpPost]
        [ValidateAntiForgeryToken] // Seguridad contra ataques XSRF
        public async Task<IActionResult> Crear(Contacto contacto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contacto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contacto);
        }

        // --- 4. EDITAR ---
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var contacto = await _context.Contactos.FindAsync(id);
            if (contacto == null) return NotFound();

            return View(contacto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Contacto contacto)
        {
            if (id != contacto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contacto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactoExists(contacto.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(contacto);
        }

        // --- 5. ELIMINAR (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var contacto = await _context.Contactos.FindAsync(id);
            if (contacto != null)
            {
                _context.Contactos.Remove(contacto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- 6. TOGGLE FAVORITO (SÚPER ÚTIL PARA LA ESTRELLA) ---
        [HttpPost]
        public async Task<IActionResult> ToggleFavorito(int id)
        {
            var contacto = await _context.Contactos.FindAsync(id);
            if (contacto != null)
            {
                contacto.EsFavorito = !contacto.EsFavorito;
                await _context.SaveChangesAsync();
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // --- 7. VISTAS EXTRAS ---
        public async Task<IActionResult> Frecuentes()
        {
            var frecuentes = await _context.Contactos.Where(c => c.EsFavorito).ToListAsync();
            return View(frecuentes);
        }

        public IActionResult Ayuda() => View();

        private bool ContactoExists(int id)
        {
            return _context.Contactos.Any(e => e.Id == id);
        }
    }
}
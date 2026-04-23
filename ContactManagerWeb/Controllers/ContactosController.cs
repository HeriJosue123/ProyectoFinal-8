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
            IQueryable<Contacto> query = _context.Contactos;

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Nombre.Contains(searchString) || s.Apellido.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(c => c.Categoria == categoria);
                ViewData["FiltroActual"] = $"Categoría: {categoria}";
            }

            if (soloFavoritos == true)
            {
                query = query.Where(c => c.EsFavorito);
                ViewData["FiltroActual"] = "Mis Favoritos ⭐";
            }

            // --- MODIFICACIÓN: ORDENAR ALFABÉTICAMENTE ---
            // Ordena primero por Nombre (A-Z) y luego por Apellido (A-Z) si tienen el mismo nombre
            query = query.OrderBy(c => c.Nombre).ThenBy(c => c.Apellido);
            // ---------------------------------------------

            var listaCompleta = await _context.Contactos.ToListAsync();
            ViewBag.TotalTodos = listaCompleta.Count;
            ViewBag.TotalFavoritos = listaCompleta.Count(c => c.EsFavorito);
            ViewBag.TotalTrabajo = listaCompleta.Count(c => c.Categoria == "Trabajo");
            ViewBag.TotalAmigos = listaCompleta.Count(c => c.Categoria == "Amigos");
            ViewBag.TotalFamilia = listaCompleta.Count(c => c.Categoria == "Familia");

            // Mantiene la lógica intacta para encontrar los 3 más recientes para el badge "NUEVO"
            ViewBag.NuevosIds = listaCompleta
                .OrderByDescending(c => c.FechaCreacion)
                .Take(3)
                .Select(c => c.Id)
                .ToList();

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Contacto contacto)
        {
            if (ModelState.IsValid)
            {
                contacto.Nombre = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(contacto.Nombre.ToLower());
                contacto.Apellido = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(contacto.Apellido.ToLower());

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
                    contacto.Nombre = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(contacto.Nombre.ToLower());
                    contacto.Apellido = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(contacto.Apellido.ToLower());

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

        // --- 6. TOGGLE FAVORITO ---
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

        // --- 7. VISTAS EXTRAS (Recientes) ---
        public async Task<IActionResult> Recientes()
        {
            var recientes = await _context.Contactos
                .OrderByDescending(c => c.FechaCreacion)
                .Take(4)
                .ToListAsync();

            ViewData["FiltroActual"] = "Agregados Recientemente";

            return View(recientes);
        }

        public IActionResult Ayuda() => View();

        private bool ContactoExists(int id)
        {
            return _context.Contactos.Any(e => e.Id == id);
        }
    }
}
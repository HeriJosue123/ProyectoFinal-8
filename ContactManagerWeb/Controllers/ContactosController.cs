using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactManagerWeb.Models;
using ContactManagerWeb.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ContactManagerWeb.Controllers
{
    public class ContactosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. LISTADO (INDEX) CON FILTROS Y CATEGORÍAS DE DB ---
        public async Task<IActionResult> Index(string searchString, string categoria, bool? soloFavoritos)
        {
            IQueryable<Contacto> query = _context.Contactos;

            // Filtro por texto
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Nombre.Contains(searchString) || s.Apellido.Contains(searchString));
            }

            // Filtro por Categoría Dinámica
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(c => c.Categoria == categoria);
                ViewData["FiltroActual"] = $"Categoría: {categoria}";
            }

            // Filtro por Favoritos
            if (soloFavoritos == true)
            {
                query = query.Where(c => c.EsFavorito);
                ViewData["FiltroActual"] = "Mis Favoritos ⭐";
            }

            // Orden Alfabético
            query = query.OrderBy(c => c.Nombre).ThenBy(c => c.Apellido);

            // Conteos Generales
            var listaContactos = await _context.Contactos.ToListAsync();
            ViewBag.TotalTodos = listaContactos.Count;
            ViewBag.TotalFavoritos = listaContactos.Count(c => c.EsFavorito);

            // --- CARGA DE CATEGORÍAS DINÁMICAS PARA EL SIDEBAR ---
            var categoriasMenu = await _context.Categorias
                .Select(cat => new
                {
                    Id = cat.Id,
                    Nombre = cat.NombreCategoria,
                    Emoji = cat.Emoji,
                    // Contamos contactos que pertenecen a esta categoría específica
                    Count = _context.Contactos.Count(c => c.Categoria == cat.NombreCategoria)
                }).ToListAsync();

            ViewBag.CategoriasDinamicas = categoriasMenu;

            // Lógica para badge "NUEVO" (3 últimos registros)
            ViewBag.NuevosIds = listaContactos
                .OrderByDescending(c => c.Id) // Usamos Id o FechaCreacion
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
        public async Task<IActionResult> Crear()
        {
            // Mandamos la lista de categorías para el dropdown
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Contacto contacto)
        {
            if (ModelState.IsValid)
            {
                contacto.Nombre = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(contacto.Nombre.ToLower());
                if (!string.IsNullOrEmpty(contacto.Apellido))
                    contacto.Apellido = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(contacto.Apellido.ToLower());

                _context.Add(contacto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(contacto);
        }

        // --- 4. EDITAR ---
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var contacto = await _context.Contactos.FindAsync(id);
            if (contacto == null) return NotFound();

            // Mandamos la lista de categorías para el dropdown
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
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
                    if (!string.IsNullOrEmpty(contacto.Apellido))
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
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(contacto);
        }

        // --- 5. ELIMINAR (SOLUCIÓN AL ERROR 404) ---

        // GET: Para mostrar la vista o modal de confirmación
        [HttpGet]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var contacto = await _context.Contactos.FirstOrDefaultAsync(m => m.Id == id);
            if (contacto == null) return NotFound();

            return View(contacto);
        }

        // POST: La acción que realmente borra de la base de datos
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
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

        // --- 7. VISTAS EXTRAS ---
        public async Task<IActionResult> Recientes()
        {
            // Usamos la misma lógica de Navbar premium en esta vista
            var recientes = await _context.Contactos
                .OrderByDescending(c => c.Id)
                .Take(4)
                .ToListAsync();

            return View(recientes);
        }

        public IActionResult Ayuda() => View();

        private bool ContactoExists(int id)
        {
            return _context.Contactos.Any(e => e.Id == id);
        }
    }
}
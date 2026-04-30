using ContactManagerWeb.Services;
using ContactManagerWeb.Models;
using ContactManagerWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContactManagerWeb.Controllers
{
    public class ContactosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ContactService _contactService;

        public ContactosController(ApplicationDbContext context, ContactService contactService)
        {
            _context = context;
            _contactService = contactService;
        }

        // --- VISTA PRINCIPAL (INDEX) ---
        public async Task<IActionResult> Index(string searchString, string categoria, bool? soloFavoritos)
        {
            IQueryable<Contacto> query = _context.Contactos;

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(s => s.Nombre.Contains(searchString) || s.Apellido.Contains(searchString));

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

            query = query.OrderBy(c => c.Nombre);

            var listaContactos = await _context.Contactos.ToListAsync();

            ViewBag.TotalTodos = listaContactos.Count;
            ViewBag.TotalFavoritos = listaContactos.Count(c => c.EsFavorito);

            ViewBag.CategoriasDinamicas = await _context.Categorias
                .Select(cat => new {
                    Id = cat.Id,
                    NombreCategoria = cat.NombreCategoria,
                    Emoji = cat.Emoji,
                    Count = _context.Contactos.Count(c => c.Categoria == cat.NombreCategoria)
                }).ToListAsync();

            ViewBag.NuevosIds = listaContactos
                .OrderByDescending(c => c.Id)
                .Take(3)
                .Select(c => c.Id)
                .ToList();

            return View(await query.ToListAsync());
        }

        // --- DETALLES ---
        public async Task<IActionResult> Detalles(int? id) =>
            (id == null) ? NotFound() : View(await _context.Contactos.FirstOrDefaultAsync(m => m.Id == id));

        // --- CREAR (GET) ---
        public async Task<IActionResult> Crear()
        {
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View();
        }

        // --- CREAR (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Contacto contacto, string NuevaCategoriaTexto, string NuevoEmoji)
        {
            try
            {
                // --- REGLA: Creación dinámica de categoría desde el formulario ---
                if (contacto.Categoria == "NUEVA_CATEGORIA" && !string.IsNullOrWhiteSpace(NuevaCategoriaTexto))
                {
                    // Validar límite antes de crear
                    var totalCats = await _context.Categorias.CountAsync();
                    if (totalCats >= 10) throw new Exception("Categoría: Límite de 10 categorías alcanzado.");

                    _context.Categorias.Add(new Categoria
                    {
                        NombreCategoria = NuevaCategoriaTexto,
                        Emoji = !string.IsNullOrWhiteSpace(NuevoEmoji) ? NuevoEmoji : "📁"
                    });
                    await _context.SaveChangesAsync();

                    contacto.Categoria = NuevaCategoriaTexto;
                }
                else if (contacto.Categoria == "NUEVA_CATEGORIA")
                {
                    throw new Exception("Categoría: Debes escribir un nombre para la nueva categoría.");
                }

                // El Service se encarga de asignar "General" si viene vacío y de validar duplicados
                await _contactService.ValidarYNormalizar(contacto);

                _context.Add(contacto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                CapturarErrores(ex);
                ViewBag.Categorias = await _context.Categorias.ToListAsync();
                return View(contacto);
            }
        }

        // --- EDITAR (GET) ---
        public async Task<IActionResult> Editar(int? id)
        {
            var c = await _context.Contactos.FindAsync(id);
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return c == null ? NotFound() : View(c);
        }

        // --- EDITAR (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Contacto contacto, string NuevaCategoriaTexto, string NuevoEmoji)
        {
            if (id != contacto.Id) return NotFound();

            try
            {
                // --- REGLA: Creación dinámica de categoría también desde Editar ---
                if (contacto.Categoria == "NUEVA_CATEGORIA" && !string.IsNullOrWhiteSpace(NuevaCategoriaTexto))
                {
                    // Validar límite antes de crear
                    var totalCats = await _context.Categorias.CountAsync();
                    if (totalCats >= 10) throw new Exception("Categoría: Límite de 10 categorías alcanzado.");

                    _context.Categorias.Add(new Categoria
                    {
                        NombreCategoria = NuevaCategoriaTexto,
                        Emoji = !string.IsNullOrWhiteSpace(NuevoEmoji) ? NuevoEmoji : "📁"
                    });
                    await _context.SaveChangesAsync();

                    contacto.Categoria = NuevaCategoriaTexto;
                }
                else if (contacto.Categoria == "NUEVA_CATEGORIA")
                {
                    throw new Exception("Categoría: Debes escribir un nombre para la nueva categoría.");
                }

                await _contactService.ValidarYNormalizar(contacto);
                _context.Update(contacto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                CapturarErrores(ex);
                ViewBag.Categorias = await _context.Categorias.ToListAsync();
                return View(contacto);
            }
        }

        // --- ELIMINAR (GET) ---
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();
            var contacto = await _context.Contactos.FirstOrDefaultAsync(m => m.Id == id);
            return contacto == null ? NotFound() : View(contacto);
        }

        // --- ELIMINAR (POST) ---
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var c = await _context.Contactos.FindAsync(id);
            if (c != null)
            {
                _context.Contactos.Remove(c);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // --- FAVORITOS ---
        [HttpPost]
        public async Task<IActionResult> ToggleFavorito(int id)
        {
            var c = await _context.Contactos.FindAsync(id);
            if (c != null)
            {
                c.EsFavorito = !c.EsFavorito;
                await _context.SaveChangesAsync();
            }
            // Retorna sin recargar la vista perdiendo la posición
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // --- RECIENTES ---
        public async Task<IActionResult> Recientes()
        {
            var lista = await _context.Contactos
                .OrderByDescending(c => c.Id)
                .Take(9)
                .ToListAsync();
            return View(lista);
        }

        // --- AYUDA ---
        public IActionResult Ayuda() => View();

        // --- MANEJO DE ERRORES ---
        private void CapturarErrores(Exception ex)
        {
            string msg = ex.Message.ToLower();
            if (msg.Contains("nombre"))
                ModelState.AddModelError("Nombre", ex.Message);
            else if (msg.Contains("correo"))
                ModelState.AddModelError("Correo", ex.Message);
            else if (msg.Contains("teléfono") || msg.Contains("número") || msg.Contains("registrado"))
                ModelState.AddModelError("Telefono", ex.Message);
            else if (msg.Contains("categoría") || msg.Contains("límite"))
                ModelState.AddModelError("Categoria", ex.Message);
            else
                ModelState.AddModelError(string.Empty, ex.Message);
        }
    }
}
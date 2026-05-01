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

        // --- 1. AGENDA PRINCIPAL ---
        public async Task<IActionResult> Index(string searchString, string categoria, bool? soloFavoritos)
        {
            IQueryable<Contacto> query = _context.Contactos;

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(s => s.Nombre.Contains(searchString) || s.Apellido.Contains(searchString));

            if (!string.IsNullOrEmpty(categoria))
                query = query.Where(c => c.Categoria == categoria);

            if (soloFavoritos == true)
                query = query.Where(c => c.EsFavorito);

            // Filtramos "General" de la lista lateral
            ViewBag.CategoriasDinamicas = await _context.Categorias
                .Where(cat => cat.NombreCategoria != "General")
                .Select(cat => new {
                    Id = cat.Id,
                    NombreCategoria = cat.NombreCategoria,
                    Emoji = cat.Emoji,
                    Count = _context.Contactos.Count(c => c.Categoria == cat.NombreCategoria)
                }).ToListAsync();

            ViewBag.TotalTodos = await _context.Contactos.CountAsync();
            ViewBag.TotalFavoritos = await _context.Contactos.CountAsync(c => c.EsFavorito);

            return View(await query.OrderBy(c => c.Nombre).ToListAsync());
        }

        // --- 2. RECIENTES ---
        public async Task<IActionResult> Recientes()
        {
            var lista = await _context.Contactos
                .OrderByDescending(c => c.Id)
                .Take(9)
                .ToListAsync();
            return View(lista);
        }

        // --- 3. AYUDA ---
        public IActionResult Ayuda() => View();

        // --- 4. DETALLES ---
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();
            var contacto = await _context.Contactos.FirstOrDefaultAsync(m => m.Id == id);
            return contacto == null ? NotFound() : View(contacto);
        }

        // --- 5. CREAR ---
        public async Task<IActionResult> Crear()
        {
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Contacto contacto, string? NuevaCategoriaTexto, string? NuevoEmoji)
        {
            try
            {
                if (contacto.Categoria != "NUEVA_CATEGORIA")
                {
                    ModelState.Remove("NuevaCategoriaTexto");
                    ModelState.Remove("NuevoEmoji");
                }

                // REGLA DE NEGOCIO: Validamos y normalizamos (Devuelve lista de errores)
                var erroresValidacion = await _contactService.ValidarYNormalizar(contacto);

                // Si hay errores, los agregamos todos juntos al ModelState
                if (erroresValidacion.Any())
                {
                    foreach (var error in erroresValidacion)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }

                // Si el ModelState es válido (No hubo errores de validación)
                if (ModelState.IsValid)
                {
                    contacto.FechaCreacion = DateTime.Now;

                    // Creación de Categoría al vuelo
                    if (contacto.Categoria == "NUEVA_CATEGORIA" && !string.IsNullOrWhiteSpace(NuevaCategoriaTexto))
                    {
                        var catNombreLimpio = NuevaCategoriaTexto.Trim();
                        var categoriaExistente = await _context.Categorias
                            .FirstOrDefaultAsync(c => c.NombreCategoria.ToLower() == catNombreLimpio.ToLower());

                        if (categoriaExistente == null)
                        {
                            var nuevaCat = new Categoria { NombreCategoria = catNombreLimpio, Emoji = NuevoEmoji ?? "📁" };
                            _context.Categorias.Add(nuevaCat);
                            await _context.SaveChangesAsync();
                            contacto.Categoria = nuevaCat.NombreCategoria;
                        }
                        else
                        {
                            contacto.Categoria = categoriaExistente.NombreCategoria;
                        }
                    }
                    else if (contacto.Categoria == "NUEVA_CATEGORIA" && string.IsNullOrWhiteSpace(NuevaCategoriaTexto))
                    {
                        // Failsafe por si eligen "Nueva Categoría" pero la dejan en blanco
                        contacto.Categoria = "General";
                    }

                    _context.Add(contacto);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                CapturarErrores(ex);
            }

            // Recargar el ViewBag en caso de error para que el Select funcione
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(contacto);
        }

        // --- 6. EDITAR ---
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();
            var contacto = await _context.Contactos.FindAsync(id);
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return contacto == null ? NotFound() : View(contacto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Contacto contacto, string? NuevaCategoriaTexto, string? NuevoEmoji)
        {
            if (id != contacto.Id) return NotFound();

            try
            {
                if (contacto.Categoria != "NUEVA_CATEGORIA")
                {
                    ModelState.Remove("NuevaCategoriaTexto");
                    ModelState.Remove("NuevoEmoji");
                }

                // REGLA DE NEGOCIO: Validamos y normalizamos (Devuelve lista de errores)
                var erroresValidacion = await _contactService.ValidarYNormalizar(contacto);

                // Si hay errores, los agregamos todos juntos al ModelState
                if (erroresValidacion.Any())
                {
                    foreach (var error in erroresValidacion)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }

                // Si el ModelState es válido
                if (ModelState.IsValid)
                {
                    // Creación de Categoría al vuelo
                    if (contacto.Categoria == "NUEVA_CATEGORIA" && !string.IsNullOrWhiteSpace(NuevaCategoriaTexto))
                    {
                        var catNombreLimpio = NuevaCategoriaTexto.Trim();
                        var categoriaExistente = await _context.Categorias
                            .FirstOrDefaultAsync(c => c.NombreCategoria.ToLower() == catNombreLimpio.ToLower());

                        if (categoriaExistente == null)
                        {
                            var nuevaCat = new Categoria { NombreCategoria = catNombreLimpio, Emoji = NuevoEmoji ?? "📁" };
                            _context.Categorias.Add(nuevaCat);
                            await _context.SaveChangesAsync();
                            contacto.Categoria = nuevaCat.NombreCategoria;
                        }
                        else
                        {
                            contacto.Categoria = categoriaExistente.NombreCategoria;
                        }
                    }
                    else if (contacto.Categoria == "NUEVA_CATEGORIA" && string.IsNullOrWhiteSpace(NuevaCategoriaTexto))
                    {
                        contacto.Categoria = "General";
                    }

                    _context.Update(contacto);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                CapturarErrores(ex);
            }

            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(contacto);
        }

        // --- 7. ELIMINAR ---
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();
            var contacto = await _context.Contactos.FirstOrDefaultAsync(m => m.Id == id);
            return contacto == null ? NotFound() : View(contacto);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var c = await _context.Contactos.FindAsync(id);
            if (c != null)
            {
                _context.Contactos.Remove(c);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contacto eliminado correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- 8. FAVORITOS ---
        [HttpPost]
        public async Task<IActionResult> ToggleFavorito(int id)
        {
            var c = await _context.Contactos.FindAsync(id);
            if (c != null) { c.EsFavorito = !c.EsFavorito; await _context.SaveChangesAsync(); }
            return Redirect(Request.Headers["Referer"].ToString());
        }

        private void CapturarErrores(Exception ex)
        {
            var mensajeReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            ModelState.AddModelError(string.Empty, mensajeReal);
        }
    }
}
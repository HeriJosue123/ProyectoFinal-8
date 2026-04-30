using ContactManagerWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactManagerWeb.Models;
using ContactManagerWeb.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        // --- 1. LISTADO (INDEX) ---
        public async Task<IActionResult> Index(string searchString, string categoria, bool? soloFavoritos)
        {
            IQueryable<Contacto> query = _context.Contactos;

            // Filtros de búsqueda
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

            query = query.OrderBy(c => c.Nombre);

            var listaContactos = await _context.Contactos.ToListAsync();
            ViewBag.TotalTodos = listaContactos.Count;
            ViewBag.TotalFavoritos = listaContactos.Count(c => c.EsFavorito);

            // Carga de categorías con el conteo de contactos por cada una
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

        // --- 2. DETALLES ---
        public async Task<IActionResult> Detalles(int? id) => (id == null) ? NotFound() : View(await _context.Contactos.FirstOrDefaultAsync(m => m.Id == id));

        // --- 3. CREAR (GET) ---
        public async Task<IActionResult> Crear()
        {
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View();
        }

        // --- 3. CREAR (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Contacto contacto)
        {
            try
            {
                // Validamos y normalizamos usando el servicio
                _contactService.ValidarYNormalizar(contacto);

                _context.Add(contacto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // ASIGNAR ERROR AL CAMPO ESPECÍFICO PARA QUE APAREZCA ABAJO DEL BOX
                string msg = ex.Message.ToLower();

                if (msg.Contains("nombre"))
                    ModelState.AddModelError("Nombre", ex.Message);
                else if (msg.Contains("correo"))
                    ModelState.AddModelError("Correo", ex.Message);
                else if (msg.Contains("teléfono") || msg.Contains("dígitos"))
                    ModelState.AddModelError("Telefono", ex.Message);
                else if (msg.Contains("categoría"))
                    ModelState.AddModelError("Categoria", ex.Message);
                else
                    ModelState.AddModelError(string.Empty, ex.Message); // Error general arriba

                ViewBag.Categorias = await _context.Categorias.ToListAsync();
                return View(contacto);
            }
        }

        // --- 4. EDITAR (GET) ---
        public async Task<IActionResult> Editar(int? id)
        {
            var c = await _context.Contactos.FindAsync(id);
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return c == null ? NotFound() : View(c);
        }

        // --- 4. EDITAR (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Contacto contacto)
        {
            if (id != contacto.Id) return NotFound();
            try
            {
                _contactService.ValidarYNormalizar(contacto);
                _context.Update(contacto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // ASIGNAR ERROR AL CAMPO ESPECÍFICO
                string msg = ex.Message.ToLower();

                if (msg.Contains("nombre"))
                    ModelState.AddModelError("Nombre", ex.Message);
                else if (msg.Contains("correo"))
                    ModelState.AddModelError("Correo", ex.Message);
                else if (msg.Contains("teléfono") || msg.Contains("dígitos"))
                    ModelState.AddModelError("Telefono", ex.Message);
                else if (msg.Contains("categoría"))
                    ModelState.AddModelError("Categoria", ex.Message);
                else
                    ModelState.AddModelError(string.Empty, ex.Message);

                ViewBag.Categorias = await _context.Categorias.ToListAsync();
                return View(contacto);
            }
        }

        // --- 5. ELIMINAR ---
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

        // --- 6. FAVORITOS ---
        [HttpPost]
        public async Task<IActionResult> ToggleFavorito(int id)
        {
            var c = await _context.Contactos.FindAsync(id);
            if (c != null)
            {
                c.EsFavorito = !c.EsFavorito;
                await _context.SaveChangesAsync();
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
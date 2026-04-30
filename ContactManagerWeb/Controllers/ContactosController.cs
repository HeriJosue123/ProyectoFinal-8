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

        // Muestra el listado principal con filtros de búsqueda, categorías y favoritos
        public async Task<IActionResult> Index(string searchString, string categoria, bool? soloFavoritos)
        {
            IQueryable<Contacto> query = _context.Contactos;

            // Filtro por nombre o apellido
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.Nombre.Contains(searchString) || s.Apellido.Contains(searchString));
            }

            // Filtro por categoría específica
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(c => c.Categoria == categoria);
                ViewData["FiltroActual"] = $"Categoría: {categoria}";
            }

            // Filtro de contactos marcados como favoritos
            if (soloFavoritos == true)
            {
                query = query.Where(c => c.EsFavorito);
                ViewData["FiltroActual"] = "Mis Favoritos ⭐";
            }

            query = query.OrderBy(c => c.Nombre);

            var listaContactos = await _context.Contactos.ToListAsync();

            // Datos para las estadísticas de la barra lateral
            ViewBag.TotalTodos = listaContactos.Count;
            ViewBag.TotalFavoritos = listaContactos.Count(c => c.EsFavorito);

            // Conteo dinámico de contactos por cada categoría
            ViewBag.CategoriasDinamicas = await _context.Categorias
                .Select(cat => new {
                    Id = cat.Id,
                    NombreCategoria = cat.NombreCategoria,
                    Emoji = cat.Emoji,
                    Count = _context.Contactos.Count(c => c.Categoria == cat.NombreCategoria)
                }).ToListAsync();

            // Identifica los 3 contactos más recientes para mostrar badges de "Nuevo"
            ViewBag.NuevosIds = listaContactos
                .OrderByDescending(c => c.Id)
                .Take(3)
                .Select(c => c.Id)
                .ToList();

            return View(await query.ToListAsync());
        }

        // Muestra la información detallada de un contacto
        public async Task<IActionResult> Detalles(int? id) =>
            (id == null) ? NotFound() : View(await _context.Contactos.FirstOrDefaultAsync(m => m.Id == id));

        // Carga el formulario para crear un nuevo contacto
        public async Task<IActionResult> Crear()
        {
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View();
        }

        // Procesa la creación del contacto validando duplicados y formato mediante el Service
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Contacto contacto)
        {
            try
            {
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

        // Carga el formulario de edición con los datos del contacto
        public async Task<IActionResult> Editar(int? id)
        {
            var c = await _context.Contactos.FindAsync(id);
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return c == null ? NotFound() : View(c);
        }

        // Procesa la actualización de un contacto existente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Contacto contacto)
        {
            if (id != contacto.Id) return NotFound();

            try
            {
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

        // Muestra la página de confirmación antes de borrar (GET)
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();
            var contacto = await _context.Contactos.FirstOrDefaultAsync(m => m.Id == id);
            return contacto == null ? NotFound() : View(contacto);
        }

        // Ejecuta la eliminación definitiva del contacto (POST)
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

        // Cambia el estado de favorito sin salir de la página actual
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

        // Muestra los últimos 10 contactos agregados al sistema
        public async Task<IActionResult> Recientes()
        {
            var lista = await _context.Contactos.OrderByDescending(c => c.Id).Take(10).ToListAsync();
            return View(lista);
        }

        // Muestra la vista de ayuda al usuario
        public IActionResult Ayuda() => View();

        // Método privado para mapear excepciones del Service hacia los campos del formulario
        private void CapturarErrores(Exception ex)
        {
            string msg = ex.Message.ToLower();
            if (msg.Contains("nombre"))
                ModelState.AddModelError("Nombre", ex.Message);
            else if (msg.Contains("correo"))
                ModelState.AddModelError("Correo", ex.Message);
            else if (msg.Contains("teléfono") || msg.Contains("número") || msg.Contains("registrado"))
                ModelState.AddModelError("Telefono", ex.Message);
            else if (msg.Contains("categoría"))
                ModelState.AddModelError("Categoria", ex.Message);
            else
                ModelState.AddModelError(string.Empty, ex.Message);
        }
    }
}
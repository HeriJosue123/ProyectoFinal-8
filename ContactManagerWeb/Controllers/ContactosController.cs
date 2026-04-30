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

        // --- ACCIÓN: VISTA PRINCIPAL (INDEX) ---
        // Muestra el listado con filtros de búsqueda, categorías y favoritos
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

            // Estadísticas para la barra lateral
            ViewBag.TotalTodos = listaContactos.Count;
            ViewBag.TotalFavoritos = listaContactos.Count(c => c.EsFavorito);

            // Conteo dinámico de contactos por categoría para el Sidebar
            ViewBag.CategoriasDinamicas = await _context.Categorias
                .Select(cat => new {
                    Id = cat.Id,
                    NombreCategoria = cat.NombreCategoria,
                    Emoji = cat.Emoji,
                    Count = _context.Contactos.Count(c => c.Categoria == cat.NombreCategoria)
                }).ToListAsync();

            // Badge de "Nuevo" para los últimos 3 registros
            ViewBag.NuevosIds = listaContactos
                .OrderByDescending(c => c.Id)
                .Take(3)
                .Select(c => c.Id)
                .ToList();

            return View(await query.ToListAsync());
        }

        // --- ACCIÓN: DETALLES DEL CONTACTO ---
        public async Task<IActionResult> Detalles(int? id) =>
            (id == null) ? NotFound() : View(await _context.Contactos.FirstOrDefaultAsync(m => m.Id == id));

        // --- ACCIÓN: CREAR CONTACTO (GET) ---
        // Carga el formulario de creación
        public async Task<IActionResult> Crear()
        {
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View();
        }

        // --- ACCIÓN: CREAR CONTACTO (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Contacto contacto)
        {
            try
            {
                // ASIGNACIÓN POR DEFECTO: Si no elige categoría, se guarda como "General"
                if (string.IsNullOrWhiteSpace(contacto.Categoria))
                {
                    contacto.Categoria = "General";
                }

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

        // --- ACCIÓN: EDITAR CONTACTO (GET) ---
        public async Task<IActionResult> Editar(int? id)
        {
            var c = await _context.Contactos.FindAsync(id);
            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return c == null ? NotFound() : View(c);
        }

        // --- ACCIÓN: EDITAR CONTACTO (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Contacto contacto)
        {
            if (id != contacto.Id) return NotFound();

            try
            {
                // ASIGNACIÓN POR DEFECTO: Mantenemos la lógica en la edición también
                if (string.IsNullOrWhiteSpace(contacto.Categoria))
                {
                    contacto.Categoria = "General";
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

        // --- ACCIÓN: ELIMINAR CONTACTO (GET - Confirmación) ---
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();
            var contacto = await _context.Contactos.FirstOrDefaultAsync(m => m.Id == id);
            return contacto == null ? NotFound() : View(contacto);
        }

        // --- ACCIÓN: ELIMINAR CONTACTO (POST - Ejecución) ---
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

        // --- ACCIÓN: TOGGLE FAVORITO (AJAX/REFERER) ---
        // Cambia el estado de estrella sin perder la posición actual
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

        // --- ACCIÓN: CONTACTOS RECIENTES ---
        // Muestra los últimos 9 contactos (Configuración 3x3)
        public async Task<IActionResult> Recientes()
        {
            var lista = await _context.Contactos
                .OrderByDescending(c => c.Id)
                .Take(9)
                .ToListAsync();
            return View(lista);
        }

        // --- ACCIÓN: AYUDA ---
        public IActionResult Ayuda() => View();

        // --- MÉTODO PRIVADO: MANEJO DE ERRORES ---
        // Mapea excepciones del Service hacia el ModelState para mostrarlos en el formulario
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
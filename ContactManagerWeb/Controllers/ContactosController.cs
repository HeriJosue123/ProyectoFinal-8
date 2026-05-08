using ContactManagerWeb.Services;
using ContactManagerWeb.Models;
using ContactManagerWeb.ViewModels;
using ContactManagerWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // 1. AGENDA PRINCIPAL (Arreglado para contadores persistentes)
        public async Task<IActionResult> Index(string searchString, string categoria, bool? soloFavoritos)
        {
            // CONSULTA A: Para el contenido central (filtrado)
            IQueryable<Contacto> query = _context.Contactos
                .AsNoTracking()
                .Include(c => c.Categoria)
                .Include(c => c.Telefonos);

            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(s => s.Nombre.Contains(searchString) || (s.Apellido != null && s.Apellido.Contains(searchString)));

            if (!string.IsNullOrEmpty(categoria))
                query = query.Where(c => c.Categoria != null && c.Categoria.NombreCategoria == categoria);

            if (soloFavoritos == true)
                query = query.Where(c => c.EsFavorito);

            var contactosFiltrados = await query.OrderBy(c => c.Nombre).ToListAsync();

            // --- CONSULTA B: ARREGLO DE CONTADORES (Globales para la barra lateral) ---
            // Traemos todos los contactos y categorías sin filtros para que el contador sea real
            var todosLosContactosGlobal = await _context.Contactos.AsNoTracking().ToListAsync();
            var categoriasLista = await _context.Categorias.AsNoTracking().ToListAsync();

            ViewBag.TotalTodos = todosLosContactosGlobal.Count;
            ViewBag.TotalFavoritos = todosLosContactosGlobal.Count(c => c.EsFavorito);

            ViewBag.CategoriasDinamicas = categoriasLista
                .Where(cat => cat.NombreCategoria != "General")
                .Select(cat => new {
                    Id = cat.Id,
                    NombreCategoria = cat.NombreCategoria,
                    Emoji = cat.Emoji,
                    // CLAVE: Contamos sobre el total global, así el sidebar no se queda en 0 al filtrar
                    Count = todosLosContactosGlobal.Count(c => c.CategoriaId == cat.Id)
                }).ToList();

            return View(contactosFiltrados);
        }

        // --- 2. RECIENTES ---
        public async Task<IActionResult> Recientes()
        {
            var lista = await _context.Contactos
                .AsNoTracking()
                .Include(c => c.Categoria)
                .Include(c => c.Telefonos)
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
            var contacto = await _context.Contactos
                .AsNoTracking()
                .Include(c => c.Categoria)
                .Include(c => c.Telefonos)
                .Include(c => c.Correos)
                .Include(c => c.Direcciones)
                .FirstOrDefaultAsync(m => m.Id == id);
            return contacto == null ? NotFound() : View(contacto);
        }

        // --- 5. CREAR ---
        public async Task<IActionResult> Crear()
        {
            ViewBag.Categorias = await _context.Categorias.AsNoTracking().ToListAsync();
            return View(new ContactoViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ContactoViewModel modelo, string? NuevaCategoriaTexto, string? NuevoEmoji)
        {
            try
            {
                if (modelo.Categoria != "NUEVA_CATEGORIA")
                {
                    ModelState.Remove("NuevaCategoriaTexto");
                    ModelState.Remove("NuevoEmoji");
                }

                if (ModelState.IsValid)
                {
                    int? idCategoriaFinal = null;

                    if (modelo.Categoria == "NUEVA_CATEGORIA" && !string.IsNullOrWhiteSpace(NuevaCategoriaTexto))
                    {
                        var catNombreLimpio = NuevaCategoriaTexto.Trim();
                        var categoriaExistente = await _context.Categorias
                            .FirstOrDefaultAsync(c => c.NombreCategoria.ToLower() == catNombreLimpio.ToLower());

                        if (categoriaExistente == null)
                        {
                            var nuevaCat = new Categoria { NombreCategoria = catNombreLimpio, Emoji = NuevoEmoji ?? "📁" };
                            _context.Categorias.Add(nuevaCat);
                            await _context.SaveChangesAsync();
                            idCategoriaFinal = nuevaCat.Id;
                        }
                        else { idCategoriaFinal = categoriaExistente.Id; }
                    }
                    else
                    {
                        var catSeleccionada = (modelo.Categoria == "NUEVA_CATEGORIA" && string.IsNullOrWhiteSpace(NuevaCategoriaTexto)) ? "General" : modelo.Categoria;
                        var catBd = await _context.Categorias.FirstOrDefaultAsync(c => c.NombreCategoria == catSeleccionada);
                        idCategoriaFinal = catBd?.Id;
                    }

                    var nuevoContacto = new Contacto
                    {
                        Nombre = modelo.Nombre,
                        Apellido = modelo.Apellido,
                        EsFavorito = modelo.EsFavorito,
                        FechaCreacion = DateTime.Now,
                        CategoriaId = idCategoriaFinal,
                        Telefonos = new List<Telefono>(),
                        Correos = new List<Correo>(),
                        Direcciones = new List<Direccion>()
                    };

                    if (!string.IsNullOrEmpty(modelo.Telefono))
                        nuevoContacto.Telefonos.Add(new Telefono { Numero = modelo.Telefono });
                    if (!string.IsNullOrEmpty(modelo.TelefonoSecundario))
                        nuevoContacto.Telefonos.Add(new Telefono { Numero = modelo.TelefonoSecundario });

                    if (!string.IsNullOrEmpty(modelo.Correo))
                        nuevoContacto.Correos.Add(new Correo { Email = modelo.Correo });
                    if (!string.IsNullOrEmpty(modelo.CorreoSecundario))
                        nuevoContacto.Correos.Add(new Correo { Email = modelo.CorreoSecundario });

                    if (!string.IsNullOrEmpty(modelo.Direccion))
                        nuevoContacto.Direcciones.Add(new Direccion { DetalleFisico = modelo.Direccion });

                    var erroresValidacion = await _contactService.ValidarYNormalizar(nuevoContacto);
                    if (erroresValidacion.Any())
                    {
                        foreach (var error in erroresValidacion) ModelState.AddModelError(string.Empty, error);
                        ViewBag.Categorias = await _context.Categorias.ToListAsync();
                        return View(modelo);
                    }

                    _context.Add(nuevoContacto);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex) { CapturarErrores(ex); }

            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(modelo);
        }

        // --- 6. EDITAR ---
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var contactoDb = await _context.Contactos
                .Include(c => c.Categoria)
                .Include(c => c.Telefonos)
                .Include(c => c.Correos)
                .Include(c => c.Direcciones)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contactoDb == null) return NotFound();

            var modelo = new ContactoViewModel
            {
                Id = contactoDb.Id,
                Nombre = contactoDb.Nombre,
                Apellido = contactoDb.Apellido,
                EsFavorito = contactoDb.EsFavorito,
                FechaCreacion = contactoDb.FechaCreacion,
                Categoria = contactoDb.Categoria?.NombreCategoria ?? "General",
                Telefono = contactoDb.Telefonos.FirstOrDefault()?.Numero,
                TelefonoSecundario = contactoDb.Telefonos.Skip(1).FirstOrDefault()?.Numero,
                Correo = contactoDb.Correos.FirstOrDefault()?.Email,
                CorreoSecundario = contactoDb.Correos.Skip(1).FirstOrDefault()?.Email,
                Direccion = contactoDb.Direcciones.FirstOrDefault()?.DetalleFisico
            };

            ViewBag.Categorias = await _context.Categorias.AsNoTracking().ToListAsync();
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ContactoViewModel modelo, string? NuevaCategoriaTexto, string? NuevoEmoji)
        {
            if (id != modelo.Id) return NotFound();

            try
            {
                if (modelo.Categoria != "NUEVA_CATEGORIA")
                {
                    ModelState.Remove("NuevaCategoriaTexto");
                    ModelState.Remove("NuevoEmoji");
                }

                if (ModelState.IsValid)
                {
                    var contactoDb = await _context.Contactos
                        .Include(c => c.Telefonos)
                        .Include(c => c.Correos)
                        .Include(c => c.Direcciones)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (contactoDb == null) return NotFound();

                    int? idCategoriaFinal = null;
                    if (modelo.Categoria == "NUEVA_CATEGORIA" && !string.IsNullOrWhiteSpace(NuevaCategoriaTexto))
                    {
                        var catNombreLimpio = NuevaCategoriaTexto.Trim();
                        var categoriaExistente = await _context.Categorias
                            .FirstOrDefaultAsync(c => c.NombreCategoria.ToLower() == catNombreLimpio.ToLower());

                        if (categoriaExistente == null)
                        {
                            var nuevaCat = new Categoria { NombreCategoria = catNombreLimpio, Emoji = NuevoEmoji ?? "📁" };
                            _context.Categorias.Add(nuevaCat);
                            await _context.SaveChangesAsync();
                            idCategoriaFinal = nuevaCat.Id;
                        }
                        else { idCategoriaFinal = categoriaExistente.Id; }
                    }
                    else
                    {
                        var catSeleccionada = (modelo.Categoria == "NUEVA_CATEGORIA" && string.IsNullOrWhiteSpace(NuevaCategoriaTexto)) ? "General" : modelo.Categoria;
                        var catBd = await _context.Categorias.FirstOrDefaultAsync(c => c.NombreCategoria == catSeleccionada);
                        idCategoriaFinal = catBd?.Id;
                    }

                    contactoDb.Nombre = modelo.Nombre;
                    contactoDb.Apellido = modelo.Apellido;
                    contactoDb.EsFavorito = modelo.EsFavorito;
                    contactoDb.CategoriaId = idCategoriaFinal;

                    _context.Telefonos.RemoveRange(contactoDb.Telefonos);
                    if (!string.IsNullOrEmpty(modelo.Telefono))
                        contactoDb.Telefonos.Add(new Telefono { Numero = modelo.Telefono });
                    if (!string.IsNullOrEmpty(modelo.TelefonoSecundario))
                        contactoDb.Telefonos.Add(new Telefono { Numero = modelo.TelefonoSecundario });

                    _context.Correos.RemoveRange(contactoDb.Correos);
                    if (!string.IsNullOrEmpty(modelo.Correo))
                        contactoDb.Correos.Add(new Correo { Email = modelo.Correo });
                    if (!string.IsNullOrEmpty(modelo.CorreoSecundario))
                        contactoDb.Correos.Add(new Correo { Email = modelo.CorreoSecundario });

                    _context.Direcciones.RemoveRange(contactoDb.Direcciones);
                    if (!string.IsNullOrEmpty(modelo.Direccion))
                        contactoDb.Direcciones.Add(new Direccion { DetalleFisico = modelo.Direccion });

                    var erroresValidacion = await _contactService.ValidarYNormalizar(contactoDb);
                    if (erroresValidacion.Any())
                    {
                        foreach (var error in erroresValidacion) ModelState.AddModelError(string.Empty, error);
                        ViewBag.Categorias = await _context.Categorias.ToListAsync();
                        return View(modelo);
                    }

                    _context.Update(contactoDb);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex) { CapturarErrores(ex); }

            ViewBag.Categorias = await _context.Categorias.ToListAsync();
            return View(modelo);
        }

        // --- 7. ELIMINAR ---
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();
            var contacto = await _context.Contactos.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
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
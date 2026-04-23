using Microsoft.AspNetCore.Mvc;
using ContactManagerWeb.Models;
using ContactManagerWeb.Data; 
using System.Linq;

namespace ContactManagerWeb.Controllers
{
    public class ContactosController : Controller
    {
        // Variable para manejar la base de datos
        private readonly ApplicationDbContext _context;

        // Inyectamos el contexto en el constructor
        public ContactosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LEER (Consultar)
        public IActionResult Index(string searchString, string categoria, bool? soloFavoritos)
        {
            // Traemos todos los contactos de SQL Server a la memoria
            var listaCompleta = _context.Contactos.ToList();
            var contactosFiltrados = listaCompleta.AsEnumerable();

            // Filtros
            if (!string.IsNullOrEmpty(searchString))
            {
                contactosFiltrados = contactosFiltrados.Where(s =>
                    (s.Nombre != null && s.Nombre.Contains(searchString, System.StringComparison.OrdinalIgnoreCase)) ||
                    (s.Apellido != null && s.Apellido.Contains(searchString, System.StringComparison.OrdinalIgnoreCase))
                );
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                contactosFiltrados = contactosFiltrados.Where(c => c.Categoria == categoria);
                ViewData["FiltroActual"] = "Mostrando Categoría: " + categoria;
            }

            if (soloFavoritos == true)
            {
                contactosFiltrados = contactosFiltrados.Where(c => c.EsFavorito == true);
                ViewData["FiltroActual"] = "Mostrando Contactos Favoritos";
            }

            // Tus contadores para el menú lateral (ahora consultando la BD)
            ViewBag.TotalTodos = listaCompleta.Count;
            ViewBag.TotalFavoritos = listaCompleta.Count(c => c.EsFavorito == true);
            ViewBag.TotalTrabajo = listaCompleta.Count(c => c.Categoria == "Trabajo");
            ViewBag.TotalAmigos = listaCompleta.Count(c => c.Categoria == "Amigos");
            ViewBag.TotalFamilia = listaCompleta.Count(c => c.Categoria == "Familia");

            return View(contactosFiltrados.ToList());
        }

        public IActionResult Frecuentes()
        {
            // Consultamos directo a la base de datos
            var contactosFrecuentes = _context.Contactos.Where(c => c.EsFavorito == true).ToList();
            return View(contactosFrecuentes);
        }

        public IActionResult Ayuda()
        {
            return View();
        }

        // 2. CREAR 
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Contacto nuevoContacto)
        {
            if (ModelState.IsValid)
            {
                _context.Contactos.Add(nuevoContacto); // Prepara el Insert
                _context.SaveChanges(); // Ejecuta el guardado en SQL
                return RedirectToAction("Index");
            }
            return View(nuevoContacto);
        }

        // 3. ACTUALIZAR (EDITAR)
        // GET: Muestra el formulario con los datos cargados
        public IActionResult Editar(int? id)
        {
            if (id == null) return NotFound();

            var contacto = _context.Contactos.Find(id); // Busca el contacto en la BD
            if (contacto == null) return NotFound();

            return View(contacto);
        }

        // POST: Guarda los cambios en la BD
        [HttpPost]
        public IActionResult Editar(int id, Contacto contactoModificado)
        {
            if (id != contactoModificado.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Contactos.Update(contactoModificado);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(contactoModificado);
        }

        // 4. ELIMINAR
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            var contacto = _context.Contactos.Find(id);
            if (contacto != null)
            {
                _context.Contactos.Remove(contacto);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
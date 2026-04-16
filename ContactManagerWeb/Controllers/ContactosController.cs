using Microsoft.AspNetCore.Mvc;
using ContactManagerWeb.Models;
using System.Collections.Generic;
using System.Linq; 

namespace ContactManagerWeb.Controllers
{
    public class ContactosController : Controller
    {
        public static List<Contacto> ListaContactos = new List<Contacto>
        {
            new Contacto { Nombre = "Ana López", Telefono = "+34 612 345 678", Correo = "ana.lopez@example.com", Categoria = "Trabajo", EsFavorito = true },
            new Contacto { Nombre = "Juan González", Telefono = "+34 612 345 678", Correo = "juangonzalez@gmail.com", Categoria = "Amigos", EsFavorito = false },
            new Contacto { Nombre = "María Umaña", Telefono = "+34 612 345 678", Correo = "maria123@gmail.com", Categoria = "Amigos", EsFavorito = false },
            new Contacto { Nombre = "Carlos Ruiz", Telefono = "+34 612 345 678", Correo = "carlosruiz@gmail.com", Categoria = "Familia", EsFavorito = false },
            new Contacto { Nombre = "Laura Pérez", Telefono = "+34 612 345 678", Correo = "lauraperez@gmail.com", Categoria = "Trabajo", EsFavorito = false },
            new Contacto { Nombre = "Miguel Soto", Telefono = "+34 612 345 678", Correo = "miguelsoto@gmail.com", Categoria = "Familia", EsFavorito = false }
        };

        public IActionResult Index(string searchString, string categoria, bool? soloFavoritos)
        {
            // 1. Convertimos la lista temporalmente para poder aplicar los filtros
            var contactos = ListaContactos.AsEnumerable();

            // 2. Filtro de Búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                contactos = contactos.Where(s => s.Nombre != null && s.Nombre.Contains(searchString, System.StringComparison.OrdinalIgnoreCase));
            }

            // 3. Filtro de Categoría
            if (!string.IsNullOrEmpty(categoria))
            {
                contactos = contactos.Where(c => c.Categoria == categoria);
                ViewData["FiltroActual"] = "Mostrando Categoría: " + categoria;
            }

            // 4. Filtro de Favoritos
            if (soloFavoritos == true)
            {
                contactos = contactos.Where(c => c.EsFavorito == true);
                ViewData["FiltroActual"] = "Mostrando Contactos Favoritos";
            }

            // --- ¡ESTO ES LO QUE FALTABA! LOS CONTADORES PARA EL MENÚ LATERAL ---
            ViewBag.TotalTodos = ListaContactos.Count();
            ViewBag.TotalFavoritos = ListaContactos.Count(c => c.EsFavorito == true);
            ViewBag.TotalTrabajo = ListaContactos.Count(c => c.Categoria == "Trabajo");
            ViewBag.TotalAmigos = ListaContactos.Count(c => c.Categoria == "Amigos");
            ViewBag.TotalFamilia = ListaContactos.Count(c => c.Categoria == "Familia");
            // ----------------------------------------------------------------------

            // 5. Devolvemos la lista final filtrada a la vista
            return View(contactos.ToList());
        }
        public IActionResult Frecuentes()
        {
            // Simulamos los frecuentes enviando solo los que son favoritos
            var contactosFrecuentes = ListaContactos.Where(c => c.EsFavorito == true).ToList();
            return View(contactosFrecuentes);
        }

        public IActionResult Ayuda()
        {
            // Simplemente renderiza la vista Ayuda.cshtml
            return View();
        }

        
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Contacto nuevoContacto)
        {
            ListaContactos.Add(nuevoContacto);
            return RedirectToAction("Index");
        }
    }
}
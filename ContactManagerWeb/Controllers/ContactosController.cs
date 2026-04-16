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
            var contactos = ListaContactos.AsEnumerable();

            // 1. Filtrado por búsqueda de texto
            if (!string.IsNullOrEmpty(searchString))
            {
                contactos = contactos.Where(s => s.Nombre != null && s.Nombre.Contains(searchString, System.StringComparison.OrdinalIgnoreCase));
            }

            // 2. Filtrado por categoría
            if (!string.IsNullOrEmpty(categoria))
            {
                contactos = contactos.Where(c => c.Categoria == categoria);
                ViewData["FiltroActual"] = "Categoría: " + categoria;
            }

            // 3. Filtrado por favoritos
            if (soloFavoritos == true)
            {
                contactos = contactos.Where(c => c.EsFavorito == true);
                ViewData["FiltroActual"] = "Contactos Favoritos";
            }

            // --- 4. CÁLCULO DE CONTADORES PARA EL SIDEBAR ---
            ViewBag.TotalTodos = ListaContactos.Count();
            ViewBag.TotalFavoritos = ListaContactos.Count(c => c.EsFavorito == true);
            ViewBag.TotalTrabajo = ListaContactos.Count(c => c.Categoria == "Trabajo");
            ViewBag.TotalAmigos = ListaContactos.Count(c => c.Categoria == "Amigos");
            ViewBag.TotalFamilia = ListaContactos.Count(c => c.Categoria == "Familia");

            return View(contactos.ToList());
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
      
        public IActionResult Frecuentes() { return View(); }
        public IActionResult Ayuda() { return View(); }
    }
}

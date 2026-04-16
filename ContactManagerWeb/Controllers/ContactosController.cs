using Microsoft.AspNetCore.Mvc;
using ContactManagerWeb.Models;
using System.Collections.Generic;
using System.Linq; // ¡NUEVO! Esencial para poder filtrar las listas

namespace ContactManagerWeb.Controllers
{
    public class ContactosController : Controller
    {
        // Lista estática en memoria simulando la base de datos (Intacta)
        public static List<Contacto> ListaContactos = new List<Contacto>
        {
            new Contacto { Nombre = "Ana López", Telefono = "+34 612 345 678", Correo = "ana.lopez@example.com", Categoria = "Trabajo", EsFavorito = true },
            new Contacto { Nombre = "Juan González", Telefono = "+34 612 345 678", Correo = "juangonzalez@gmail.com", Categoria = "Amigos", EsFavorito = false },
            new Contacto { Nombre = "María Umaña", Telefono = "+34 612 345 678", Correo = "maria123@gmail.com", Categoria = "Amigos", EsFavorito = false },
            new Contacto { Nombre = "Carlos Ruiz", Telefono = "+34 612 345 678", Correo = "carlosruiz@gmail.com", Categoria = "Familia", EsFavorito = false },
            new Contacto { Nombre = "Laura Pérez", Telefono = "+34 612 345 678", Correo = "lauraperez@gmail.com", Categoria = "Trabajo", EsFavorito = false },
            new Contacto { Nombre = "Miguel Soto", Telefono = "+34 612 345 678", Correo = "miguelsoto@gmail.com", Categoria = "Familia", EsFavorito = false }
        };

        // MODIFICADO: El Index ahora atrapa los clics del Sidebar y el texto del Navbar
        public IActionResult Index(string searchString, string categoria, bool? soloFavoritos)
        {
            // 1. Convertimos la lista temporalmente para poder aplicar los filtros
            var contactos = ListaContactos.AsEnumerable();

            // 2. Filtro de Búsqueda (Si el usuario escribió algo en el Navbar)
            if (!string.IsNullOrEmpty(searchString))
            {
                // Busca coincidencias ignorando mayúsculas y minúsculas
                contactos = contactos.Where(s => s.Nombre.Contains(searchString, System.StringComparison.OrdinalIgnoreCase));
            }

            // 3. Filtro de Categoría (Si el usuario hizo clic en Trabajo, Familia, etc. en el Sidebar)
            if (!string.IsNullOrEmpty(categoria))
            {
                contactos = contactos.Where(c => c.Categoria == categoria);
                // Guardamos esto para que la Vista sepa qué título mostrar
                ViewData["FiltroActual"] = "Mostrando Categoría: " + categoria;
            }

            // 4. Filtro de Favoritos (Si el usuario hizo clic en "Ver Favoritos")
            if (soloFavoritos == true)
            {
                contactos = contactos.Where(c => c.EsFavorito == true);
                ViewData["FiltroActual"] = "Mostrando Contactos Favoritos";
            }

            // 5. Devolvemos la lista final filtrada a la vista
            return View(contactos.ToList());
        }

        // NUEVO: Funcionalidad para el botón "Contactos Frecuentes" del Navbar
        public IActionResult Frecuentes()
        {
            // Simulamos los frecuentes enviando solo los que son favoritos
            var contactosFrecuentes = ListaContactos.Where(c => c.EsFavorito == true).ToList();
            return View(contactosFrecuentes);
        }

        // NUEVO: Funcionalidad para el botón "Ayuda" del Navbar
        public IActionResult Ayuda()
        {
            // Simplemente renderiza la vista Ayuda.cshtml
            return View();
        }

        // INTACTO: Tus métodos para crear contactos siguen funcionando igual de bien
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
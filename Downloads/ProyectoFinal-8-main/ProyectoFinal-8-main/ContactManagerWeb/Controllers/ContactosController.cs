using Microsoft.AspNetCore.Mvc;
using ContactManagerWeb.Models;
using System.Collections.Generic;

namespace ContactManagerWeb.Controllers
{
    public class ContactosController : Controller
    {
        // Lista estática en memoria simulando la base de datos
        public static List<Contacto> ListaContactos = new List<Contacto>
        {
            new Contacto { Nombre = "Ana López", Telefono = "+34 612 345 678", Correo = "ana.lopez@example.com", Categoria = "Trabajo", EsFavorito = true },
            new Contacto { Nombre = "Juan González", Telefono = "+34 612 345 678", Correo = "juangonzalez@gmail.com", Categoria = "Amigos", EsFavorito = false },
            new Contacto { Nombre = "María Umaña", Telefono = "+34 612 345 678", Correo = "maria123@gmail.com", Categoria = "Amigos", EsFavorito = false },
            new Contacto { Nombre = "Carlos Ruiz", Telefono = "+34 612 345 678", Correo = "carlosruiz@gmail.com", Categoria = "Familia", EsFavorito = false },
            new Contacto { Nombre = "Laura Pérez", Telefono = "+34 612 345 678", Correo = "lauraperez@gmail.com", Categoria = "Trabajo", EsFavorito = false },
            new Contacto { Nombre = "Miguel Soto", Telefono = "+34 612 345 678", Correo = "miguelsoto@gmail.com", Categoria = "Familia", EsFavorito = false }
        };

        public IActionResult Index()
        {
            return View(ListaContactos);
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
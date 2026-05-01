using ContactManagerWeb.Models;
using ContactManagerWeb.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ContactManagerWeb.Services
{
    public class ContactService
    {
        private readonly ApplicationDbContext _context;

        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        // AHORA DEVUELVE UNA LISTA DE ERRORES
        public async Task<List<string>> ValidarYNormalizar(Contacto contacto)
        {
            var errores = new List<string>();
            var textInfo = new CultureInfo("es-ES", false).TextInfo;

            // 1. Normalizar y Validar Nombre
            if (string.IsNullOrWhiteSpace(contacto.Nombre))
            {
                errores.Add("El nombre del contacto es obligatorio.");
            }
            else if (!Regex.IsMatch(contacto.Nombre, @"[a-zA-ZáéíóúÁÉÍÓÚñÑ]"))
            {
                errores.Add("El nombre no puede contener solo números o símbolos. Debe incluir letras.");
            }
            else
            {
                contacto.Nombre = textInfo.ToTitleCase(contacto.Nombre.Trim().ToLower());
            }

            // 2. Normalizar y Validar Apellido
            if (string.IsNullOrWhiteSpace(contacto.Apellido))
            {
                contacto.Apellido = "";
            }
            else
            {
                if (!Regex.IsMatch(contacto.Apellido, @"[a-zA-ZáéíóúÁÉÍÓÚñÑ]"))
                {
                    errores.Add("El apellido debe incluir letras, no puede ser solo números.");
                }
                else
                {
                    contacto.Apellido = textInfo.ToTitleCase(contacto.Apellido.Trim().ToLower());
                }
            }

            // 3. Categoría por defecto
            if (string.IsNullOrWhiteSpace(contacto.Categoria))
            {
                contacto.Categoria = "General";
            }

            // 4. Validación y Auto-Formato de Teléfono
            if (string.IsNullOrWhiteSpace(contacto.Telefono))
            {
                errores.Add("El número de teléfono es obligatorio.");
            }
            else
            {
                string soloNumeros = new string(contacto.Telefono.Where(char.IsDigit).ToArray());

                if (soloNumeros.Length < 8)
                {
                    errores.Add("El teléfono debe contener al menos 8 dígitos.");
                }
                else if (soloNumeros.Length == 8 && !contacto.Telefono.Contains("-"))
                {
                    contacto.Telefono = soloNumeros.Insert(4, "-");
                }

                // Validar Duplicados
                var duplicado = await _context.Contactos
                    .AnyAsync(c => c.Telefono == contacto.Telefono && c.Id != contacto.Id);

                if (duplicado)
                {
                    errores.Add($"El número {contacto.Telefono} ya pertenece a otro contacto en la agenda.");
                }
            }

            // 5. Arreglo para Correo y Dirección
            if (string.IsNullOrWhiteSpace(contacto.Correo))
            {
                contacto.Correo = "";
            }
            else
            {
                // Verifica que tenga algo, luego un @, luego algo, luego un punto y algo más.
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                if (!emailRegex.IsMatch(contacto.Correo))
                {
                    // ESTE ES EL MENSAJE QUE APARECERÁ EN LA CAJA ROJA DE ARRIBA Y EN EL SPAN
                    errores.Add("Dirección de correo inválida: debe llevar obligatoriamente una '@' y un dominio.");
                }
            }

            return errores; // Retornamos todos los errores juntos
        }
    }
}
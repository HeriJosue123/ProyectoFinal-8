using ContactManagerWeb.Models;
using ContactManagerWeb.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
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

        public async Task ValidarYNormalizar(Contacto contacto)
        {
            // --- REGLA 1: Asignar categoría por defecto si viene vacía ---
            if (string.IsNullOrWhiteSpace(contacto.Categoria))
            {
                contacto.Categoria = "General";
            }

            // --- REGLA 2: Evitar teléfonos duplicados ---
            var duplicado = await _context.Contactos
                .FirstOrDefaultAsync(c => c.Telefono == contacto.Telefono && c.Id != contacto.Id);

            if (duplicado != null)
            {
                throw new Exception($"El número ya está registrado bajo el nombre \"{duplicado.Nombre}\".");
            }

            // --- REGLA 3: Validación de formato y duplicidad de correo ---
            if (!string.IsNullOrWhiteSpace(contacto.Correo))
            {
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$");
                if (!emailRegex.IsMatch(contacto.Correo))
                {
                    throw new Exception("Formato de correo inválido. Asegúrate de incluir '@' y un dominio.");
                }

                var correoDuplicado = await _context.Contactos
                    .AnyAsync(c => c.Correo == contacto.Correo && c.Id != contacto.Id);

                if (correoDuplicado)
                {
                    throw new Exception("Esta dirección de correo ya pertenece a otro contacto.");
                }
            }

            // --- REGLA 4: Validación estricta de teléfono (8 dígitos) ---
            if (!string.IsNullOrWhiteSpace(contacto.Telefono))
            {
                var soloNumeros = contacto.Telefono
                    .Replace("+503", "")
                    .Replace("-", "")
                    .Replace(" ", "");

                if (soloNumeros.Length != 8 || !soloNumeros.All(char.IsDigit))
                {
                    throw new Exception("El número de teléfono debe tener exactamente 8 dígitos numéricos.");
                }
            }

            // --- REGLA 5: Normalización de textos (Mayúsculas iniciales) ---
            if (string.IsNullOrWhiteSpace(contacto.Nombre))
            {
                throw new Exception("El nombre del contacto es obligatorio.");
            }

            var textInfo = new CultureInfo("es-ES", false).TextInfo;
            contacto.Nombre = textInfo.ToTitleCase(contacto.Nombre.Trim().ToLower());

            if (!string.IsNullOrWhiteSpace(contacto.Apellido))
            {
                contacto.Apellido = textInfo.ToTitleCase(contacto.Apellido.Trim().ToLower());
            }
            else
            {
                contacto.Apellido = null;
            }
        }
    }
}
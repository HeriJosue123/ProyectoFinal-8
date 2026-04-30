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
            var textInfo = new CultureInfo("es-ES", false).TextInfo;

            // 1. Normalizar Nombre
            if (string.IsNullOrWhiteSpace(contacto.Nombre))
                throw new Exception("El nombre del contacto es obligatorio.");

            contacto.Nombre = textInfo.ToTitleCase(contacto.Nombre.Trim().ToLower());

            // 2. Arreglo para Apellido (Si es vacío, se guarda "" para evitar error de NULL en SQL)
            if (string.IsNullOrWhiteSpace(contacto.Apellido))
            {
                contacto.Apellido = "";
            }
            else
            {
                contacto.Apellido = textInfo.ToTitleCase(contacto.Apellido.Trim().ToLower());
            }

            // 3. Categoría por defecto
            if (string.IsNullOrWhiteSpace(contacto.Categoria))
            {
                contacto.Categoria = "General";
            }

            // 4. Validación de Teléfono
            if (string.IsNullOrWhiteSpace(contacto.Telefono))
                throw new Exception("El número de teléfono es obligatorio.");

            string soloNumeros = new string(contacto.Telefono.Where(char.IsDigit).ToArray());

            if (soloNumeros.Length < 8)
                throw new Exception("El teléfono debe contener al menos 8 dígitos.");

            // Validar Duplicados
            var duplicado = await _context.Contactos
                .AnyAsync(c => c.Telefono == contacto.Telefono && c.Id != contacto.Id);

            if (duplicado)
                throw new Exception($"El número {contacto.Telefono} ya está registrado.");

            // 5. Arreglo para Correo y Dirección (Evitar valores NULL en la BD)
            if (string.IsNullOrWhiteSpace(contacto.Correo))
            {
                contacto.Correo = "";
            }
            else
            {
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$");
                if (!emailRegex.IsMatch(contacto.Correo))
                    throw new Exception("Formato de correo inválido.");
            }

            if (string.IsNullOrWhiteSpace(contacto.Direccion))
            {
                contacto.Direccion = "";
            }
        }
    }
}
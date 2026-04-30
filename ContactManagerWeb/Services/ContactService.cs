using ContactManagerWeb.Models;
using ContactManagerWeb.Data; // Añadido para el DbContext
using Microsoft.EntityFrameworkCore; // Añadido para AnyAsync
using System;
using System.Globalization;
using System.Threading.Tasks; // Añadido para Task

namespace ContactManagerWeb.Services
{
    public class ContactService
    {
        private readonly ApplicationDbContext _context;

        // Inyectamos el contexto para poder consultar la base de datos
        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ValidarYNormalizar(Contacto contacto)
        {
            // --- 1. VALIDACIÓN DE DUPLICADOS ---
            // Buscamos si existe alguien con el mismo teléfono que NO sea el mismo contacto (para permitir edición)
            var duplicado = await _context.Contactos
                .FirstOrDefaultAsync(c => c.Telefono == contacto.Telefono && c.Id != contacto.Id);

            if (duplicado != null)
            {
                throw new Exception($"Número ya registrado como \"{duplicado.Nombre}\"");
            }

            // --- 2. VALIDACIONES DE CAMPOS ---
            if (string.IsNullOrWhiteSpace(contacto.Nombre))
                throw new Exception("¡Atención! El nombre del contacto no puede quedar vacío.");

            if (string.IsNullOrWhiteSpace(contacto.Categoria))
                throw new Exception("Por favor, selecciona una categoría para organizar tu contacto.");

            if (!string.IsNullOrWhiteSpace(contacto.Correo))
            {
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$");
                if (!emailRegex.IsMatch(contacto.Correo))
                    throw new Exception("El correo electrónico ingresado no es válido. Asegúrate de incluir '@' y un dominio (ejemplo: .com).");
            }

            // --- 3. NORMALIZACIÓN ---
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

            // VALIDACIÓN DE TELÉFONO 
            if (!string.IsNullOrWhiteSpace(contacto.Telefono))
            {
                // 1. Limpiamos el texto para dejar solo los números reales
                var soloNumeros = contacto.Telefono
                    .Replace("+503", "")
                    .Replace("-", "")
                    .Replace(" ", "");

                // 2. Regla: Debe tener exactamente 8 dígitos
                if (soloNumeros.Length != 8)
                {
                    throw new Exception("El número de teléfono debe tener exactamente 8 dígitos.");
                }

                // 3. Regla: No se permiten letras en este campo
                if (!soloNumeros.All(char.IsDigit))
                {
                    throw new Exception("El número de teléfono solo puede contener dígitos numéricos.");
                }
            }
        }
    }
}
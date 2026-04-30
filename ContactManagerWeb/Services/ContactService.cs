using ContactManagerWeb.Models;
using System;
using System.Globalization;

namespace ContactManagerWeb.Services
{
    public class ContactService
    {
        public void ValidarYNormalizar(Contacto contacto)
        {
            // --- VALIDACIONES ---
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

            // --- NORMALIZACIÓN ---
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
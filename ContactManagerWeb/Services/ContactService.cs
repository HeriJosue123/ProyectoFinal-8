using ContactManagerWeb.Models;
using System;

namespace ContactManagerWeb.Services
{
    public class ContactService
    {
        // TAREA DE SUSY: Validaciones Básicas de Reglas de Negocio
        public void ValidarDatosBasicos(Contacto contacto)
        {
            // 1. No permitir campos vacíos (Requisito Moodle)
            if (string.IsNullOrWhiteSpace(contacto.Nombre))
            {
                throw new Exception("Regla de Negocio: El nombre es obligatorio y no puede quedar vacío.");
            }

            // 2. Validar formato de correo electrónico (Requisito Moodle)
            if (!string.IsNullOrWhiteSpace(contacto.Correo) && !contacto.Correo.Contains("@"))
            {
                throw new Exception("Regla de Negocio: El correo debe contener un '@'.");
            }
        }
    }
}
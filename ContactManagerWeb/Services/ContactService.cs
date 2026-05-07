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

        // AHORA VALIDA USANDO LAS LISTAS RELACIONADAS
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

            // 3. Validación y Auto-Formato de Teléfono (Ahora en la tabla Telefonos)
            var telefonoPrincipal = contacto.Telefonos.FirstOrDefault();

            if (telefonoPrincipal == null || string.IsNullOrWhiteSpace(telefonoPrincipal.Numero))
            {
                errores.Add("El número de teléfono es obligatorio.");
            }
            else
            {
                string soloNumeros = new string(telefonoPrincipal.Numero.Where(char.IsDigit).ToArray());

                if (soloNumeros.Length < 8)
                {
                    errores.Add("El teléfono debe contener al menos 8 dígitos.");
                }
                else if (soloNumeros.Length == 8 && !telefonoPrincipal.Numero.Contains("-"))
                {
                    // Auto-formato XXXX-XXXX
                    telefonoPrincipal.Numero = soloNumeros.Insert(4, "-");
                }

                // Validar Duplicados (Buscando en la tabla de Telefonos)
                var numeroParaValidar = telefonoPrincipal.Numero;
                var duplicado = await _context.Telefonos
                    .AnyAsync(t => t.Numero == numeroParaValidar && t.ContactoId != contacto.Id);

                if (duplicado)
                {
                    errores.Add($"El número {numeroParaValidar} ya pertenece a otro contacto en la agenda.");
                }
            }

            // 4. Validación de Correo (Ahora en la tabla Correos)
            var correoPrincipal = contacto.Correos.FirstOrDefault();

            if (correoPrincipal != null && !string.IsNullOrWhiteSpace(correoPrincipal.Email))
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                if (!emailRegex.IsMatch(correoPrincipal.Email))
                {
                    errores.Add("Dirección de correo inválida: debe llevar obligatoriamente una '@' y un dominio.");
                }
            }

            return errores;
        }
    }
}
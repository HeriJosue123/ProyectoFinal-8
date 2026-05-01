using ContactManagerWeb.Data;
using ContactManagerWeb.Services;
using Microsoft.EntityFrameworkCore;

namespace ContactManagerWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // --- CONFIGURACIÓN DE BASE DE DATOS ---
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // --- REGISTRO DE LA CAPA DE LÓGICA DE NEGOCIO (SERVICES / BLL) ---
            // Indispensable para que los controladores puedan usar las validaciones
            builder.Services.AddScoped<ContactService>();
            builder.Services.AddScoped<CategoriaService>(); // Añadido para gestionar las reglas de categorías

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapStaticAssets();

            // --- CONFIGURACIÓN DE RUTA INICIAL ---
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
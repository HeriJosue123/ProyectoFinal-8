using ContactManagerWeb.Services;
using ContactManagerWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace ContactManagerWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<ContactService>();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Registro de la Capa de Servicios
            builder.Services.AddScoped<ContactManagerWeb.Services.ContactService>();

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

            // Configuración para que abra la Agenda directo
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Contactos}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
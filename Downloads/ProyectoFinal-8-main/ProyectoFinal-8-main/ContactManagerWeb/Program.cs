namespace ContactManagerWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();
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
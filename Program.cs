using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CafeteriaDb");
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                     ?? Environment.GetEnvironmentVariable("MYSQL_URL")
                     ?? Environment.GetEnvironmentVariable("MYSQLDATABASE_URL");
}

builder.WebHost.UseUrls("http://0.0.0.0:" + (Environment.GetEnvironmentVariable("PORT") ?? "8080"));

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
builder.Services.AddDbContext<CafeteriaDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));
builder.Services.AddSingleton<Cafeteria>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var cafeteria = scope.ServiceProvider.GetRequiredService<Cafeteria>();
    if (!cafeteria.HayProductos())
    {
        cafeteria.RegistrarProducto("A01", "Botella", 30.00m, 8);
        cafeteria.RegistrarProducto("B02", "Galleta", 10.50m, 15);
        cafeteria.RegistrarProducto("C03", "Jugo", 22.00m, 5);
    }
}

app.UseDefaultFiles();
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
provider.Mappings[".webp"] = "image/webp";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
app.UseCors();
app.MapControllers();

app.Run();

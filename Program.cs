using Microsoft.EntityFrameworkCore;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:" + (Environment.GetEnvironmentVariable("PORT") ?? "8080"));

var connectionString = builder.Configuration.GetConnectionString("CafeteriaDb");
if (string.IsNullOrEmpty(connectionString))
{
    var url = Environment.GetEnvironmentVariable("DATABASE_URL")
           ?? Environment.GetEnvironmentVariable("MYSQL_URL");
    if (!string.IsNullOrEmpty(url))
    {
        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':');
        connectionString = $"Server={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};User={userInfo[0]};Password={userInfo.Length > 1 ? userInfo[1] : ""};";
    }
}

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<CafeteriaDbContext>(options =>
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString)
        ));
    builder.Services.AddSingleton<Cafeteria>();
}

var app = builder.Build();

if (!string.IsNullOrEmpty(connectionString))
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var cafeteria = scope.ServiceProvider.GetRequiredService<Cafeteria>();
            if (!cafeteria.HayProductos())
            {
                cafeteria.RegistrarProducto("A01", "Botella", 30.00m, 8);
                cafeteria.RegistrarProducto("B02", "Galleta", 10.50m, 15);
                cafeteria.RegistrarProducto("C03", "Jugo", 22.00m, 5);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error de BD: " + ex.Message);
        }
    }
}

app.MapGet("/health", () => Results.Ok("ok"));

app.UseDefaultFiles();
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
provider.Mappings[".webp"] = "image/webp";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
app.UseCors();
app.MapControllers();

app.Run();

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
        var user = Uri.UnescapeDataString(uri.UserInfo.Split(':')[0]);
        var pass = uri.UserInfo.Contains(':') ? Uri.UnescapeDataString(uri.UserInfo.Split(':')[1]) : "";
        var db = uri.AbsolutePath.TrimStart('/');
        connectionString = $"Server={uri.Host};Port={uri.Port};Database={db};User={user};Password={pass};";
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
    try
    {
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
        builder.Services.AddDbContext<CafeteriaDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));
        builder.Services.AddScoped<Cafeteria>();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error configurando BD: " + ex.Message);
    }
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

app.MapGet("/debug", () =>
{
    var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    var mySqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL");
    var mySqlHost = Environment.GetEnvironmentVariable("MYSQLHOST");
    var port = Environment.GetEnvironmentVariable("PORT");
    var connStr = builder.Configuration.GetConnectionString("CafeteriaDb");
    return Results.Ok(new
    {
        hasConnectionString = !string.IsNullOrEmpty(connStr),
        hasDatabaseUrl = !string.IsNullOrEmpty(dbUrl),
        hasMysqlUrl = !string.IsNullOrEmpty(mySqlUrl),
        hasMysqlHost = !string.IsNullOrEmpty(mySqlHost),
        port = port,
        dbUrlPrefix = dbUrl?.Substring(0, Math.Min(10, dbUrl?.Length ?? 0)),
        mySqlUrlPrefix = mySqlUrl?.Substring(0, Math.Min(10, mySqlUrl?.Length ?? 0))
    });
});

app.UseDefaultFiles();
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
provider.Mappings[".webp"] = "image/webp";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });
app.UseCors();
app.MapControllers();

app.Run();

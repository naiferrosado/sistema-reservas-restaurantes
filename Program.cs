using Microsoft.EntityFrameworkCore;
using SistemaReservas.Data;
using SistemaReservas.Services;

// Habilitar compatibilidad de timestamps en Npgsql PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Configurar Base de Datos con PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// Inyección de dependencias
builder.Services.AddScoped<IEmailService, EmailService>();

// MVC con Vistas Razor + soporte JSON para controladores API
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SistemaReservas API", Version = "v1" });
});

var app = builder.Build();

// Inicializar la base de datos y sembrar datos de prueba
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
    }
}

// Swagger UI siempre accesible
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema Reservas API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

// Archivos estáticos (CSS, JS, imágenes)
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Ruta MVC convencional como ruta principal — HomeController.Index() sirve /
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mantener rutas API para compatibilidad Swagger
app.MapControllers();

app.Run();

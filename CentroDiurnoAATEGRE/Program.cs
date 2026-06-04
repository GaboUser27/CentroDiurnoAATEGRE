
using System.Text;
using CentroDiurnoAATEGRE.Application.Config;
using CentroDiurnoAATEGRE.Infraestructure.Data;
using CentroDiurnoAATEGRE.Web.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Serilog;
using Serilog.Events;

//***********
// =======================
// Configurar Serilog
// =======================
// Crear carpeta Logs automáticamente (evita errores si no existe)
Directory.CreateDirectory("Logs");

// Configuración Serilog
var logger = new LoggerConfiguration()
    // Nivel mínimo global (recomendado: Information)
    .MinimumLevel.Information()

    // Reducir ruido de logs internos de Microsoft
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    //Mostrar SQL ejecutado por EF Core
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)

    // Enriquecer logs con contexto (RequestId, etc.)
    .Enrich.FromLogContext()

    // Consola: útil para depurar en Visual Studio
    .WriteTo.Console()

    //Archivos separados por nivel (rolling diario)
    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
        .WriteTo.File(@"Logs\Info-.log",
            shared: true,
            encoding: Encoding.UTF8,
            rollingInterval: RollingInterval.Day))

    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
        .WriteTo.File(@"Logs\Warning-.log",
            shared: true,
            encoding: Encoding.UTF8,
            rollingInterval: RollingInterval.Day))

    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
        .WriteTo.File(@"Logs\Error-.log",
            shared: true,
            encoding: Encoding.UTF8,
            rollingInterval: RollingInterval.Day))

    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
        .WriteTo.File(@"Logs\Fatal-.log",
            shared: true,
            encoding: Encoding.UTF8,
            rollingInterval: RollingInterval.Day))

    .CreateLogger();

// Paso obligatorio ANTES de crear builder
Log.Logger = logger;

var builder = WebApplication.CreateBuilder(args);
// Mapeo de la clase AppConfig para leer appsettings.json
builder.Services.Configure<AppConfig>(builder.Configuration);




// Integrar Serilog al host
builder.Host.UseSerilog(Log.Logger);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cache en memoria (requerido por Session)
builder.Services.AddDistributedMemoryCache();

//Registrar Session con opciones
builder.Services.AddSession(options =>
{
    // Tiempo máximo sin actividad antes de expirar la sesión
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    // Opciones de la cookie de sesión
    options.Cookie.HttpOnly = true;          // No accesible desde JS
    options.Cookie.IsEssential = true;       // Necesaria aunque haya consentimiento de cookies
    options.Cookie.Name = ".CentroDiurnoAATEGRE.Session";
});

//***********
// =======================
// Configurar Dependency Injection
// =======================
//*** Repositories
//builder.services.addtransient<irepositoryautor, repositoryautor>();
//builder.services.addtransient<irepositorycategoria, repositorycategoria>();
//builder.services.addtransient<irepositorylibro, repositorylibro>();
//builder.services.addtransient<irepositoryorden, repositoryorden>();
//builder.services.addtransient<irepositorycliente, repositorycliente>();
//builder.services.addtransient<irepositoryusuario, repositoryusuario>();


//*** Services
//builder.Services.AddTransient<IServiceAutor, ServiceAutor>();
//builder.Services.AddTransient<IServiceCategoria, ServiceCategoria>();
//builder.Services.AddTransient<IServiceLibro, ServiceLibro>();
//builder.Services.AddTransient<IServiceOrden, ServiceOrden>();
//builder.Services.AddTransient<IServiceCliente, ServiceCliente>();
//builder.Services.AddTransient<IServiceUsuario, ServiceUsuario>();
//builder.Services.AddTransient<IServiceReporte, ServiceReporte>();

// =======================
// Configurar AutoMapper
// =======================
builder.Services.AddAutoMapper(config =>
{
    //*** Profiles
    //config.AddProfile<AutorProfile>();
    //config.AddProfile<CategoriaProfile>();
    //config.AddProfile<ClienteProfile>();
    //config.AddProfile<LibroProfile>();
    //config.AddProfile<OrdenDetalleProfile>();
    //config.AddProfile<OrdenProfile>();
    //config.AddProfile<RolProfile>();
    //config.AddProfile<UsuarioProfile>();
});

//Seguridad
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Login/Index";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.AccessDeniedPath = "/Login/Forbidden";
    });
builder.Services.AddControllersWithViews(options => {
    options.Filters.Add(
        new ResponseCacheAttribute
        {
            NoStore = true,
            Location = ResponseCacheLocation.None,
        });
});

// =======================
// Configurar SQL Server DbContext
// =======================
var connectionString = builder.Configuration.GetConnectionString("SqlServerDataBase");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'SqlServerDataBase' en appsettings.json / appsettings.Development.json.");
}

builder.Services.AddDbContext<AATEGREContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Reintentos ante fallos transitorios (recomendado)
        sqlOptions.EnableRetryOnFailure();
    });

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});
//****API******
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];

builder.Services.AddHttpClient("LibreriaApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl!);
});
//****API******

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Middleware personalizado
    app.UseMiddleware<ErrorHandlingMiddleware>();
}



app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session DEBE ir antes de MapControllerRoute / MapDefaultControllerRoute
app.UseSession();

app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapStaticAssets();

app.UseAntiforgery();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

using AutoMapper;
using CentroDiurnoAATEGRE.Application.Profiles;
using CentroDiurnoAATEGRE.Application.Services.Implementations;
using CentroDiurnoAATEGRE.Application.Services.Interfaces;
using CentroDiurnoAATEGRE.Infraestructure.Data;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Implementations;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using CentroDiurnoAATEGRE.Web.BackgroundServices;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using System.Security.Cryptography;

var ejecutarMigracionContrasenas = args.Contains("--migrate-legacy-passwords", StringComparer.OrdinalIgnoreCase);
var builder = WebApplication.CreateBuilder(args.Where(arg =>
    !string.Equals(arg, "--migrate-legacy-passwords", StringComparison.OrdinalIgnoreCase)).ToArray());

// MVC
builder.Services.AddControllersWithViews();

// Rate limiting specifically for login attempts. Use a stable client IP only;
// configure trusted forwarded headers in hosting before relying on this behind a proxy.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "text/html; charset=utf-8";
        context.HttpContext.Response.Headers.CacheControl = "no-store";
        await context.HttpContext.Response.WriteAsync(
            "<!doctype html><html lang=\"es\"><meta charset=\"utf-8\"><title>Espere antes de intentar</title><main><h1>Demasiados intentos</h1><p>Espere unos minutos antes de volver a intentarlo.</p><a href=\"/Usuario/Login\">Volver al inicio de sesión</a></main></html>",
            cancellationToken);
    };
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

// Base de datos
builder.Services.AddDbContext<AATEGREContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection using deployment secrets.")));

// Repositorios
builder.Services.AddScoped<IAvisoRepository, AvisoRepository>();
builder.Services.AddScoped<ICategoriaImagenRepository, CategoriaImagenRepository>();
builder.Services.AddScoped<IImagenRepository, ImagenRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IInformacionInstitucionalRepository, InformacionInstitucionalRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
// Servicios
builder.Services.AddScoped<IAvisoService, AvisoService>();
builder.Services.AddScoped<IInformacionInstitucionalService, InformacionInstitucionalService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IImagenService, ImagenService>();
builder.Services.AddScoped<ICategoriaImagenService, CategoriaImagenService>();

// Tarea programada: inactiva los avisos vencidos (al arrancar y cada hora).
builder.Services.AddHostedService<AvisosVencidosService>();

//Automapper
builder.Services.AddAutoMapper(config =>
{
    // *** Profiles
    config.AddProfile<AvisoProfile>();
    config.AddProfile<CategoriaImagenProfile>();
    config.AddProfile<EstadoUsuarioProfile>();
    config.AddProfile<HorarioProfile>();
    config.AddProfile<ImagenProfile>();
    config.AddProfile<InformacionInstitucionalProfile>();
    config.AddProfile<RolProfile>();
    config.AddProfile<UsuarioProfile>();

});

// Autenticación con cookies
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "AATEGRE.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/Usuario/Login";
        options.LogoutPath = "/Usuario/Logout";
        options.AccessDeniedPath = "/Home/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.IsEssential = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "AATEGRE.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (ejecutarMigracionContrasenas)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AATEGREContext>();
    var usuarios = await db.Usuario.ToListAsync();
    var migrados = 0;
    foreach (var usuario in usuarios)
    {
        if (usuario.Contrasena.StartsWith("$2a$") || usuario.Contrasena.StartsWith("$2b$") || usuario.Contrasena.StartsWith("$2y$"))
            continue;
        usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasena);
        migrados++;
    }
    await db.SaveChangesAsync();
    Console.WriteLine($"Migración completada. Contraseñas convertidas a BCrypt: {migrados}.");
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(18));
    context.Items["CspNonce"] = nonce;
    context.Response.Headers["Content-Security-Policy"] =
        $"default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'none'; " +
        $"img-src 'self' data:; font-src 'self' https://cdn.jsdelivr.net https://fonts.gstatic.com; " +
        $"style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
        $"script-src 'self' 'nonce-{nonce}'; connect-src 'self'; " +
        $"frame-src https://maps.google.com https://www.google.com; form-action 'self'; upgrade-insecure-requests";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});
app.Use(async (context, next) =>
{
    if (string.Equals(context.Request.Host.Host, "www.aategre.cr", StringComparison.OrdinalIgnoreCase))
    {
        var canonicalUrl = $"https://www.aategre.cr{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
        context.Response.Redirect(canonicalUrl, permanent: true);
        return;
    }
    await next();
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Crear carpeta uploads si no existe
var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

app.Run();

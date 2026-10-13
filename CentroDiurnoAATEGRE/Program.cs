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
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

var ejecutarMigracionContrasenas = args.Contains("--migrate-legacy-passwords", StringComparer.OrdinalIgnoreCase);
var builder = WebApplication.CreateBuilder(args.Where(arg =>
    !string.Equals(arg, "--migrate-legacy-passwords", StringComparison.OrdinalIgnoreCase)).ToArray());

// MVC
builder.Services.AddControllersWithViews(options =>
{
    // Defensa en profundidad: todo POST/PUT/DELETE exige token antiforgery,
    // aunque en el futuro alguien olvide el atributo en una acción nueva.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Las claves de Data Protection deben persistir en disco; si no, cada reciclaje
// del proceso en el hosting invalida sesiones, TempData y tokens antiforgery.
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys");
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection()
    .SetApplicationName("AATEGRE.CentroDiurno")
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

// Puerto HTTPS explícito para que UseHttpsRedirection funcione detrás de IIS.
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    if (!builder.Environment.IsDevelopment())
        options.HttpsPort = 443; // en desarrollo se usa el puerto de launchSettings
});

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
        // Revalida en cada petición que el usuario siga activo y conserve el mismo rol,
        // para que desactivar o cambiar de rol a alguien surta efecto de inmediato.
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var idClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var rolClaim = context.Principal?.FindFirstValue(ClaimTypes.Role);
                var valido = false;
                if (int.TryParse(idClaim, out var idUsuario) && rolClaim != null)
                {
                    var usuarioService = context.HttpContext.RequestServices.GetRequiredService<IUsuarioService>();
                    valido = await usuarioService.SesionVigenteAsync(idUsuario, rolClaim);
                }
                if (!valido)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync("CookieAuth");
                }
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "AATEGRE.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
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
app.UseStatusCodePagesWithReExecute("/Home/Error", "?codigo={0}");

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
    // Redirige el dominio sin "www" al dominio canónico (www.aategre.cr).
    if (string.Equals(context.Request.Host.Host, "aategre.cr", StringComparison.OrdinalIgnoreCase))
    {
        var canonicalUrl = $"https://www.aategre.cr{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
        context.Response.Redirect(canonicalUrl, permanent: true);
        return;
    }

    // El panel administrativo y el login nunca deben indexarse.
    var ruta = context.Request.Path;
    if (ruta.StartsWithSegments("/Usuario", StringComparison.OrdinalIgnoreCase)
        || ruta.StartsWithSegments("/Aviso", StringComparison.OrdinalIgnoreCase)
        || ruta.StartsWithSegments("/CategoriaImagen", StringComparison.OrdinalIgnoreCase)
        || ruta.StartsWithSegments("/InformacionInstitucional", StringComparison.OrdinalIgnoreCase)
        || ruta.StartsWithSegments("/Home/Dashboard", StringComparison.OrdinalIgnoreCase)
        || ruta.StartsWithSegments("/Imagen/Index", StringComparison.OrdinalIgnoreCase)
        || ruta.StartsWithSegments("/Imagen/Formulario", StringComparison.OrdinalIgnoreCase)
        || ruta.StartsWithSegments("/Imagen/Crear", StringComparison.OrdinalIgnoreCase)
        || ruta.StartsWithSegments("/Imagen/Editar", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
    }
    await next();
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

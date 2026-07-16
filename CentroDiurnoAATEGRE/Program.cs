using AutoMapper;
using CentroDiurnoAATEGRE.Application.Profiles;
using CentroDiurnoAATEGRE.Application.Services.Implementations;
using CentroDiurnoAATEGRE.Application.Services.Interfaces;
using CentroDiurnoAATEGRE.Infraestructure.Data;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Implementations;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Base de datos
builder.Services.AddDbContext<AATEGREContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
        options.LoginPath = "/Usuario/Login";
        options.LogoutPath = "/Usuario/Logout";
        options.AccessDeniedPath = "/Home/Index";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
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

using CentroDiurnoAATEGRE.Application.Services.Interfaces;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentroDiurnoAATEGRE.Web.Controllers
{
    public class HomeController : Controller
    {
        // Nombre de la categor�a de im�genes que se usar� para mostrar
        // el presupuesto/transparencia en "Qui�nes Somos".
        // Debe crearse esta categor�a desde el panel administrativo
        // (Im�genes > Categor�as) y subir ah� la imagen del presupuesto.
        private const string CATEGORIA_PRESUPUESTO = "Presupuesto";

        private readonly IAvisoService _avisoService;
        private readonly IInformacionInstitucionalService _infoService;
        private readonly IUsuarioService _usuarioService;
        private readonly IImagenService _imagenService;
        private readonly ICategoriaImagenService _categoriaService;

        public HomeController(
            IAvisoService avisoService,
            IInformacionInstitucionalService infoService,
            IUsuarioService usuarioService,
            IImagenService imagenService,
            ICategoriaImagenService categoriaService)
        {
            _avisoService = avisoService;
            _infoService = infoService;
            _usuarioService = usuarioService;
            _imagenService = imagenService;
            _categoriaService = categoriaService;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            ViewBag.Avisos = await _avisoService.ObtenerVigentesAsync();
            ViewBag.Informacion = await _infoService.ObtenerPrimeraAsync();
            ViewBag.Imagenes = (await _imagenService.ObtenerConCategoriaAsync())
                .Where(i => i.TieneImagen)
                .OrderByDescending(i => i.FechaImagen)
                .Take(5)
                .ToList();
            return View();
        }

        // GET: /Home/QuienesSomos
        public async Task<IActionResult> QuienesSomos()
        {
            ViewData["ActivePage"] = "Quienes";
            ViewBag.Informacion = await _infoService.ObtenerPrimeraAsync();

            var imagenes = (await _imagenService.ObtenerConCategoriaAsync())
                .Where(i => i.TieneImagen)
                .ToList();

            // Im�genes para el carrusel: todas las que NO son la del presupuesto.
            ViewBag.ImagenesCarrusel = imagenes
                .Where(i => !EsCategoriaPresupuesto(i.IdCategoriaImagenNavigation?.Nombre))
                .OrderByDescending(i => i.FechaImagen)
                .Take(6)
                .ToList();

            // Imagen(es) del presupuesto asignado por el gobierno (JPS).
            ViewBag.ImagenesPresupuesto = imagenes
                .Where(i => EsCategoriaPresupuesto(i.IdCategoriaImagenNavigation?.Nombre))
                .OrderByDescending(i => i.FechaImagen)
                .ToList();

            return View();
        }

        // GET: /Home/Avisos  -> p�gina p�blica con todos los avisos vigentes
        public async Task<IActionResult> Avisos()
        {
            ViewData["ActivePage"] = "Avisos";
            ViewBag.Avisos = await _avisoService.ObtenerVigentesAsync();
            return View();
        }

        // GET: /Home/Contacto
        public async Task<IActionResult> Contacto()
        {
            ViewData["ActivePage"] = "Contacto";
            ViewBag.Informacion = await _infoService.ObtenerPrimeraAsync();
            return View();
        }

        // GET: /Home/Terminos  -> Terminos y condiciones de uso del sitio
        [AllowAnonymous]
        public IActionResult Terminos()
        {
            ViewData["ActivePage"] = "Terminos";
            return View();
        }

        // GET: /Home/AccesoDenegado
        // A donde redirige el sistema de autenticación cuando un usuario
        // autenticado (ej. un Colaborador) intenta entrar a una acción
        // reservada a otro rol (ej. [Authorize(Roles = "Administrador")]).
        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? codigo = null)
        {
            ViewData["CodigoError"] = codigo;
            return View("Error");
        }
        private async Task<int> ObtenerTotalAvisosActivosAsync()
        {
            var avisosActivos = await _avisoService.ObtenerVigentesAsync();
            return avisosActivos.Count();
        }
        // GET: /Home/Dashboard
        [Authorize(Roles = "Administrador,Colaborador,Empleado")]
        public async Task<IActionResult> Dashboard()
        {
            var avisos = await _avisoService.ObtenerTodosAsync();
            var imagenes = await _imagenService.ObtenerTodosAsync();
            var cats = await _categoriaService.ObtenerTodosAsync();
            var esAdministrador = User.IsInRole("Administrador");
            var usuarios = esAdministrador ? await _usuarioService.ObtenerTodosAsync() : null;

            // Solo los avisos marcados como activos: la tarjeta se llama
            // "Avisos activos" pero antes contaba el total de la tabla.
            ViewBag.TotalAvisos = await ObtenerTotalAvisosActivosAsync();
            ViewBag.TotalImagenes = imagenes.Count();
            ViewBag.TotalCategorias = cats.Count();
            ViewBag.TotalUsuarios = usuarios?.Count();
            ViewBag.AvisosRecientes = avisos.Take(5);
            ViewBag.Usuarios = usuarios?.Take(5);

            return View();
        }

        private static bool EsCategoriaPresupuesto(string? nombreCategoria)
        {
            if (string.IsNullOrWhiteSpace(nombreCategoria)) return false;
            return nombreCategoria.Trim().Equals(CATEGORIA_PRESUPUESTO, StringComparison.OrdinalIgnoreCase)
                || nombreCategoria.Trim().Contains("transparencia", StringComparison.OrdinalIgnoreCase);
        }
    }
}
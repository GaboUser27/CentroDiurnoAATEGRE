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
        // Nombre de la categoría de imágenes que se usará para mostrar
        // el presupuesto/transparencia en "Quiénes Somos".
        // Debe crearse esta categoría desde el panel administrativo
        // (Imágenes > Categorías) y subir ahí la imagen del presupuesto.
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
                .Where(i => i.Imagen1 != null)
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
                .Where(i => i.Imagen1 != null)
                .ToList();

            // Imágenes para el carrusel: todas las que NO son la del presupuesto.
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

        // GET: /Home/Avisos  -> página pública con todos los avisos vigentes
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

        // GET: /Home/AccesoDenegado
        // A donde redirige el sistema de autenticaciÃ³n cuando un usuario
        // autenticado (ej. un Colaborador) intenta entrar a una acciÃ³n
        // reservada a otro rol (ej. [Authorize(Roles = "Administrador")]).
        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        // GET: /Home/Dashboard
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var avisos = await _avisoService.ObtenerTodosAsync();
            var imagenes = await _imagenService.ObtenerTodosAsync();
            var cats = await _categoriaService.ObtenerTodosAsync();
            var usuarios = await _usuarioService.ObtenerTodosAsync();

            ViewBag.TotalAvisos = avisos.Count();
            ViewBag.TotalImagenes = imagenes.Count();
            ViewBag.TotalCategorias = cats.Count();
            ViewBag.TotalUsuarios = usuarios.Count();
            ViewBag.AvisosRecientes = avisos.Take(5);
            ViewBag.Usuarios = usuarios.Take(5);

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

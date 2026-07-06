using CentroDiurnoAATEGRE.Application.Services.Interfaces;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentroDiurnoAATEGRE.Web.Controllers
{
    public class HomeController : Controller
    {
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
                .Where(i => i.ImagenBytes != null)
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
            return View();
        }

        // GET: /Home/Contacto
        public async Task<IActionResult> Contacto()
        {
            ViewData["ActivePage"] = "Contacto";
            ViewBag.Informacion = await _infoService.ObtenerPrimeraAsync();
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
    }
}
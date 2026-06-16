using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentroDiurnoAATEGRE.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAvisoRepository _avisoRepo;
        private readonly IInformacionInstitucionalRepository _infoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IImagenRepository _imagenRepo;
        private readonly ICategoriaImagenRepository _categoriaRepo;

        public HomeController(
            IAvisoRepository avisoRepo,
            IInformacionInstitucionalRepository infoRepo,
            IUsuarioRepository usuarioRepo,
            IImagenRepository imagenRepo,
            ICategoriaImagenRepository categoriaRepo)
        {
            _avisoRepo = avisoRepo;
            _infoRepo = infoRepo;
            _usuarioRepo = usuarioRepo;
            _imagenRepo = imagenRepo;
            _categoriaRepo = categoriaRepo;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            ViewBag.Avisos = await _avisoRepo.ObtenerVigentesAsync();
            ViewBag.Informacion = await _infoRepo.ObtenerPrimeraAsync();
            return View();
        }

        // GET: /Home/QuienesSomos
        public async Task<IActionResult> QuienesSomos()
        {
            ViewData["ActivePage"] = "Quienes";
            ViewBag.Informacion = await _infoRepo.ObtenerPrimeraAsync();
            return View();
        }

        // GET: /Home/Contacto
        public async Task<IActionResult> Contacto()
        {
            ViewData["ActivePage"] = "Contacto";
            ViewBag.Informacion = await _infoRepo.ObtenerPrimeraAsync();
            return View();
        }

        // GET: /Home/Dashboard  [Autorizado]
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var avisos = await _avisoRepo.ObtenerTodosAsync();
            var imagenes = await _imagenRepo.ObtenerTodosAsync();
            var cats = await _categoriaRepo.ObtenerTodosAsync();
            var usuarios = await _usuarioRepo.ObtenerConRolesYEstadosAsync();

            ViewBag.TotalAvisos = avisos.Count();
            ViewBag.TotalImagenes = imagenes.Count();
            ViewBag.TotalCategorias = cats.Count();
            ViewBag.TotalUsuarios = usuarios.Count();
            ViewBag.AvisosRecientes = avisos.OrderByDescending(a => a.FechaPublicacion).Take(5);
            ViewBag.Usuarios = usuarios.Take(5);

            return View();
        }
    }
}
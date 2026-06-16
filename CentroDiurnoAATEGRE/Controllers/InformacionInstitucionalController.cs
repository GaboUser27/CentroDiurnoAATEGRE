using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentroDiurnoAATEGRE.Web.Controllers
{
    [Authorize]
    public class InformacionInstitucionalController : Controller
    {
        private readonly IInformacionInstitucionalRepository _infoRepo;

        public InformacionInstitucionalController(IInformacionInstitucionalRepository infoRepo)
        {
            _infoRepo = infoRepo;
        }

        // GET: /InformacionInstitucional
        public async Task<IActionResult> Index()
        {
            var info = await _infoRepo.ObtenerPrimeraAsync();
            if (info == null)
                return RedirectToAction(nameof(Crear));

            return View(info);
        }

        // GET: /InformacionInstitucional/Crear
        public IActionResult Crear()
        {
            ViewData["Title"] = "Crear Información Institucional";
            return View("Formulario", new InformacionInstitucionalDTO());
        }

        // POST: /InformacionInstitucional/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(InformacionInstitucionalDTO dto)
        {
            ViewData["Title"] = "Crear Información Institucional";
            if (!ModelState.IsValid) return View("Formulario", dto);

            var info = new InformacionInstitucional
            {
                Titulo    = dto.Titulo,
                Contenido = dto.Contenido,
                Telefono  = dto.Telefono,
                Correo    = dto.Correo,
                Direccion = dto.Direccion,
                Facebook  = dto.Facebook,
                Instagram = dto.Instagram
            };

            await _infoRepo.AgregarAsync(info);
            TempData["Exito"] = "Información institucional guardada.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /InformacionInstitucional/Editar/1
        public async Task<IActionResult> Editar(int id)
        {
            ViewData["Title"] = "Editar Información Institucional";
            var info = await _infoRepo.ObtenerPorIdAsync(id);
            if (info == null) return NotFound();

            var dto = new InformacionInstitucionalDTO
            {
                IdInformacion = info.IdInformacion,
                Titulo        = info.Titulo,
                Contenido     = info.Contenido,
                Telefono      = info.Telefono,
                Correo        = info.Correo,
                Direccion     = info.Direccion,
                Facebook      = info.Facebook,
                Instagram     = info.Instagram
            };
            return View("Formulario", dto);
        }

        // POST: /InformacionInstitucional/Editar/1
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, InformacionInstitucionalDTO dto)
        {
            ViewData["Title"] = "Editar Información Institucional";
            if (!ModelState.IsValid) return View("Formulario", dto);

            var info = await _infoRepo.ObtenerPorIdAsync(id);
            if (info == null) return NotFound();

            info.Titulo    = dto.Titulo;
            info.Contenido = dto.Contenido;
            info.Telefono  = dto.Telefono;
            info.Correo    = dto.Correo;
            info.Direccion = dto.Direccion;
            info.Facebook  = dto.Facebook;
            info.Instagram = dto.Instagram;

            await _infoRepo.ActualizarAsync(info);
            TempData["Exito"] = "Información actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}

using AutoMapper;
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
        private readonly IMapper _mapper;

        public InformacionInstitucionalController(
            IInformacionInstitucionalRepository infoRepo,
            IMapper mapper)
        {
            _infoRepo = infoRepo;
            _mapper = mapper;
        }

        // GET: /InformacionInstitucional
        public async Task<IActionResult> Index()
        {
            var info = await _infoRepo.ObtenerPrimeraAsync();
            if (info == null) return RedirectToAction(nameof(Crear));

            var dto = _mapper.Map<InformacionInstitucionalDTO>(info);
            return View(dto);
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

            var info = _mapper.Map<InformacionInstitucional>(dto);
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

            var dto = _mapper.Map<InformacionInstitucionalDTO>(info);
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

            _mapper.Map(dto, info);
            await _infoRepo.ActualizarAsync(info);

            TempData["Exito"] = "Información actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
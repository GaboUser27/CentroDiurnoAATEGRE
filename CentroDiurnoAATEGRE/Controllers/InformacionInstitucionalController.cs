using AutoMapper;
using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Application.Services.Interfaces;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CentroDiurnoAATEGRE.Web.Controllers
{
    [Authorize]
    public class InformacionInstitucionalController : Controller
    {
        private readonly IInformacionInstitucionalService _infoService;

        public InformacionInstitucionalController(IInformacionInstitucionalService infoService)
        {
            _infoService = infoService;
        }

        public async Task<IActionResult> Index()
        {
            var dto = await _infoService.ObtenerPrimeraAsync();
            if (dto == null) return RedirectToAction(nameof(Crear));
            return View(dto);
        }

        public IActionResult Crear()
        {
            ViewData["Title"] = "Crear Información Institucional";
            return View("Formulario", new InformacionInstitucionalDTO());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(InformacionInstitucionalDTO dto)
        {
            ViewData["Title"] = "Crear Información Institucional";
            if (!ModelState.IsValid) return View("Formulario", dto);

            await _infoService.CrearAsync(dto);
            TempData["Exito"] = "Información institucional guardada.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            ViewData["Title"] = "Editar Información Institucional";
            var dto = await _infoService.ObtenerPorIdAsync(id);
            if (dto == null) return NotFound();
            return View("Formulario", dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, InformacionInstitucionalDTO dto)
        {
            ViewData["Title"] = "Editar Información Institucional";
            if (!ModelState.IsValid) return View("Formulario", dto);

            await _infoService.EditarAsync(id, dto);
            TempData["Exito"] = "Información actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
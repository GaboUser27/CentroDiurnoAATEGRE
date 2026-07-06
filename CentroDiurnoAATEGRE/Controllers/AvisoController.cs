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
    public class AvisoController : Controller
    {
        private readonly IAvisoService _avisoService;

        public AvisoController(IAvisoService avisoService)
        {
            _avisoService = avisoService;
        }

        public async Task<IActionResult> Index()
        {
            var dtos = await _avisoService.ObtenerTodosAsync();
            return View(dtos);
        }

        public IActionResult Crear()
        {
            ViewData["Title"] = "Nuevo Aviso";
            return View("Formulario", new AvisoDTO
            {
                FechaPublicacion = DateTime.Now,
                Activo = true
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(AvisoDTO dto)
        {
            ViewData["Title"] = "Nuevo Aviso";
            if (!ModelState.IsValid) return View("Formulario", dto);

            await _avisoService.CrearAsync(dto);
            TempData["Exito"] = "Aviso creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            ViewData["Title"] = "Editar Aviso";
            var dto = await _avisoService.ObtenerPorIdAsync(id);
            if (dto == null) return NotFound();
            return View("Formulario", dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, AvisoDTO dto)
        {
            ViewData["Title"] = "Editar Aviso";
            if (!ModelState.IsValid) return View("Formulario", dto);

            await _avisoService.EditarAsync(id, dto);
            TempData["Exito"] = "Aviso actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Eliminar(int id)
        {
            await _avisoService.EliminarAsync(id);
            TempData["Exito"] = "Aviso eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
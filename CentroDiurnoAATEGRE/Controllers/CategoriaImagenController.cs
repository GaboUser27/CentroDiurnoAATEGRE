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
    public class CategoriaImagenController : Controller
    {
        private readonly ICategoriaImagenService _categoriaService;

        public CategoriaImagenController(ICategoriaImagenService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        public async Task<IActionResult> Index()
        {
            var dtos = await _categoriaService.ObtenerConImagenesAsync();
            return View(dtos);
        }

        public IActionResult Crear()
        {
            ViewData["Title"] = "Nueva Categoría";
            return View("Formulario", new CategoriaImagenDTO());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CategoriaImagenDTO dto)
        {
            ViewData["Title"] = "Nueva Categoría";
            if (!ModelState.IsValid) return View("Formulario", dto);

            await _categoriaService.CrearAsync(dto);
            TempData["Exito"] = $"Categoría «{dto.Nombre}» creada.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            ViewData["Title"] = "Editar Categoría";
            var dto = await _categoriaService.ObtenerPorIdAsync(id);
            if (dto == null) return NotFound();
            return View("Formulario", dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, CategoriaImagenDTO dto)
        {
            ViewData["Title"] = "Editar Categoría";
            if (!ModelState.IsValid) return View("Formulario", dto);

            await _categoriaService.EditarAsync(id, dto);
            TempData["Exito"] = "Categoría actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Eliminar(int id)
        {
            var eliminado = await _categoriaService.EliminarAsync(id);
            if (!eliminado)
                TempData["Error"] = "No se puede eliminar la categoría porque tiene imágenes asociadas. Elimínalas primero.";
            else
                TempData["Exito"] = "Categoría eliminada.";

            return RedirectToAction(nameof(Index));
        }
    }
}
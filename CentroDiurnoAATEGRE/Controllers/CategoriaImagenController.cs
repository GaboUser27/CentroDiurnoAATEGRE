using AutoMapper;
using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentroDiurnoAATEGRE.Web.Controllers
{
    [Authorize]
    public class CategoriaImagenController : Controller
    {
        private readonly ICategoriaImagenRepository _categoriaRepo;
        private readonly IMapper _mapper;

        public CategoriaImagenController(ICategoriaImagenRepository categoriaRepo, IMapper mapper)
        {
            _categoriaRepo = categoriaRepo;
            _mapper = mapper;
        }

        // GET: /CategoriaImagen
        public async Task<IActionResult> Index()
        {
            var categorias = await _categoriaRepo.ObtenerConImagenesAsync();
            var dtos = _mapper.Map<IEnumerable<CategoriaImagenDTO>>(categorias);
            return View(dtos);
        }

        // GET: /CategoriaImagen/Crear
        public IActionResult Crear()
        {
            ViewData["Title"] = "Nueva Categoría";
            return View("Formulario", new CategoriaImagenDTO());
        }

        // POST: /CategoriaImagen/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CategoriaImagenDTO dto)
        {
            ViewData["Title"] = "Nueva Categoría";
            if (!ModelState.IsValid) return View("Formulario", dto);

            var categoria = _mapper.Map<CategoriaImagen>(dto);
            await _categoriaRepo.AgregarAsync(categoria);

            TempData["Exito"] = $"Categoría «{dto.Nombre}» creada.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /CategoriaImagen/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            ViewData["Title"] = "Editar Categoría";
            var cat = await _categoriaRepo.ObtenerPorIdAsync(id);
            if (cat == null) return NotFound();

            var dto = _mapper.Map<CategoriaImagenDTO>(cat);
            return View("Formulario", dto);
        }

        // POST: /CategoriaImagen/Editar/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, CategoriaImagenDTO dto)
        {
            ViewData["Title"] = "Editar Categoría";
            if (!ModelState.IsValid) return View("Formulario", dto);

            var cat = await _categoriaRepo.ObtenerPorIdAsync(id);
            if (cat == null) return NotFound();

            _mapper.Map(dto, cat);
            await _categoriaRepo.ActualizarAsync(cat);

            TempData["Exito"] = "Categoría actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /CategoriaImagen/Eliminar/5
        public async Task<IActionResult> Eliminar(int id)
        {
            var cats = await _categoriaRepo.ObtenerConImagenesAsync();
            var categoria = cats.FirstOrDefault(c => c.IdCategoriaImagen == id);
            if (categoria == null) return NotFound();

            if (categoria.Imagenes.Any())
            {
                TempData["Error"] = $"No puedes eliminar «{categoria.Nombre}» porque tiene {categoria.Imagenes.Count} imagen(es). Elimínalas primero.";
                return RedirectToAction(nameof(Index));
            }

            await _categoriaRepo.EliminarAsync(id);
            TempData["Exito"] = "Categoría eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
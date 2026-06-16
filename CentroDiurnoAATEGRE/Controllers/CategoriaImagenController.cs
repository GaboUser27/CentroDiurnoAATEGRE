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

        public CategoriaImagenController(ICategoriaImagenRepository categoriaRepo)
        {
            _categoriaRepo = categoriaRepo;
        }

        // GET: /CategoriaImagen
        public async Task<IActionResult> Index()
        {
            var categorias = await _categoriaRepo.ObtenerConImagenesAsync();
            return View(categorias);
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

            var categoria = new CategoriaImagen
            {
                Nombre      = dto.Nombre,
                Descripcion = dto.Descripcion
            };

            await _categoriaRepo.AgregarAsync(categoria);
            TempData["Exito"] = $"Categoría «{dto.Nombre}» creada. Ahora puedes agregar imágenes.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /CategoriaImagen/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            ViewData["Title"] = "Editar Categoría";
            var cat = await _categoriaRepo.ObtenerPorIdAsync(id);
            if (cat == null) return NotFound();

            var dto = new CategoriaImagenDTO
            {
                IdCategoriaImagen = cat.IdCategoriaImagen,
                Nombre            = cat.Nombre,
                Descripcion       = cat.Descripcion
            };
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

            cat.Nombre      = dto.Nombre;
            cat.Descripcion = dto.Descripcion;

            await _categoriaRepo.ActualizarAsync(cat);
            TempData["Exito"] = "Categoría actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /CategoriaImagen/Eliminar/5
        public async Task<IActionResult> Eliminar(int id)
        {
            var cat = await _categoriaRepo.ObtenerConImagenesAsync();
            var categoria = cat.FirstOrDefault(c => c.IdCategoriaImagen == id);

            if (categoria == null) return NotFound();

            if (categoria.Imagenes.Any())
            {
                TempData["Error"] = $"No puedes eliminar «{categoria.Nombre}» porque tiene {categoria.Imagenes.Count} imagen(es) asociada(s). Elimina las imágenes primero.";
                return RedirectToAction(nameof(Index));
            }

            await _categoriaRepo.EliminarAsync(id);
            TempData["Exito"] = "Categoría eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }
}

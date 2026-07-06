using AutoMapper;
using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Application.Services.Interfaces;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CentroDiurnoAATEGRE.Web.Controllers
{
    public class ImagenController : Controller
    {
        private readonly IImagenService _imagenService;
        private readonly ICategoriaImagenService _categoriaService;

        public ImagenController(
            IImagenService imagenService,
            ICategoriaImagenService categoriaService)
        {
            _imagenService = imagenService;
            _categoriaService = categoriaService;
        }

        // ── PÚBLICO ────────────────────────────────────────────────────

        [AllowAnonymous]
        public async Task<IActionResult> Galeria(int? categoriaId)
        {
            ViewData["ActivePage"] = "Actividades";
            var categorias = await _categoriaService.ObtenerTodosAsync();
            ViewBag.Categorias = categorias;

            IEnumerable<ImagenDTO> dtos;
            if (categoriaId.HasValue)
            {
                dtos = await _imagenService.ObtenerPorCategoriaAsync(categoriaId.Value);
                var cat = categorias.FirstOrDefault(c => c.IdCategoriaImagen == categoriaId.Value);
                ViewBag.CategoriaActual = categoriaId.Value;
                ViewBag.NombreCategoria = cat?.Nombre;
                ViewBag.DescripcionCategoria = cat?.Descripcion;
            }
            else
            {
                dtos = await _imagenService.ObtenerConCategoriaAsync();
            }

            return View(dtos);
        }

        // ── ADMIN ──────────────────────────────────────────────────────

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var dtos = await _imagenService.ObtenerConCategoriaAsync();
            return View(dtos);
        }

        [Authorize]
        public async Task<IActionResult> Crear()
        {
            await CargarCategoriasAsync();
            return View("Formulario", new ImagenDTO { FechaImagen = DateTime.Now });
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ImagenDTO dto, IFormFile? archivo)
        {
            if (!ModelState.IsValid) { await CargarCategoriasAsync(); return View("Formulario", dto); }

            byte[]? bytes = null;
            if (archivo != null && archivo.Length > 0)
            {
                if (!EsImagenValida(archivo))
                {
                    ModelState.AddModelError("archivo", "Solo se permiten imágenes (jpg, png, webp, gif).");
                    await CargarCategoriasAsync();
                    return View("Formulario", dto);
                }
                bytes = await LeerBytesAsync(archivo);
            }

            await _imagenService.CrearAsync(dto, bytes);
            TempData["Exito"] = "Imagen agregada a la galería.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Editar(int id)
        {
            var dto = await _imagenService.ObtenerPorIdAsync(id);
            if (dto == null) return NotFound();

            await CargarCategoriasAsync();
            return View("Formulario", dto);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ImagenDTO dto, IFormFile? archivo)
        {
            if (!ModelState.IsValid) { await CargarCategoriasAsync(); return View("Formulario", dto); }

            byte[]? bytes = null;
            if (archivo != null && archivo.Length > 0)
            {
                if (!EsImagenValida(archivo))
                {
                    ModelState.AddModelError("archivo", "Solo se permiten imágenes (jpg, png, webp, gif).");
                    await CargarCategoriasAsync();
                    return View("Formulario", dto);
                }
                bytes = await LeerBytesAsync(archivo);
            }

            await _imagenService.EditarAsync(id, dto, bytes);
            TempData["Exito"] = "Imagen actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _imagenService.EliminarAsync(id);
            TempData["Exito"] = "Imagen eliminada.";
            return RedirectToAction(nameof(Index));
        }

        // ── PRIVADOS ───────────────────────────────────────────────────

        private async Task CargarCategoriasAsync()
        {
            var cats = await _categoriaService.ObtenerTodosAsync();
            ViewBag.Categorias = new SelectList(cats, "IdCategoriaImagen", "Nombre");
        }

        private static bool EsImagenValida(IFormFile archivo)
        {
            var extensiones = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            return extensiones.Contains(ext);
        }

        private static async Task<byte[]> LeerBytesAsync(IFormFile archivo)
        {
            using var ms = new MemoryStream();
            await archivo.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
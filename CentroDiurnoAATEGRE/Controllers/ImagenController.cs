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

        [HttpGet, Authorize]
        public async Task<IActionResult> Index()
        {
            var dtos = await _imagenService.ObtenerConCategoriaAsync();
            return View(dtos);
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> Crear()
        {
            await CargarCategoriasAsync();
            return View("Formulario", new ImagenDTO { FechaImagen = DateTime.Now });
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ImagenDTO dto, List<IFormFile>? archivos, IFormFile? archivo)
        {
            var listaArchivos = new List<IFormFile>();
            if (archivos != null && archivos.Count > 0)
            {
                listaArchivos.AddRange(archivos.Where(a => a != null && a.Length > 0));
            }
            else if (archivo != null && archivo.Length > 0)
            {
                listaArchivos.Add(archivo);
            }

            if (listaArchivos.Count == 0)
            {
                ModelState.AddModelError("archivos", "Debe seleccionar al menos una imagen.");
                await CargarCategoriasAsync();
                return View("Formulario", dto);
            }

            foreach (var f in listaArchivos)
            {
                if (!EsImagenValida(f))
                {
                    ModelState.AddModelError("archivos", $"El archivo '{f.FileName}' no es una imagen válida. Formatos permitidos: JPG, PNG, WebP y GIF.");
                    await CargarCategoriasAsync();
                    return View("Formulario", dto);
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync();
                return View("Formulario", dto);
            }

            var tituloBase = string.IsNullOrWhiteSpace(dto.Titulo) ? "Imagen" : dto.Titulo.Trim();
            int subidas = 0;

            for (int i = 0; i < listaArchivos.Count; i++)
            {
                var file = listaArchivos[i];
                var bytes = await LeerBytesAsync(file);

                var nuevaImagen = new ImagenDTO
                {
                    IdCategoriaImagen = dto.IdCategoriaImagen,
                    FechaImagen = dto.FechaImagen,
                    Descripcion = dto.Descripcion,
                    Titulo = listaArchivos.Count == 1 ? tituloBase : $"{tituloBase} ({i + 1})"
                };

                await _imagenService.CrearAsync(nuevaImagen, bytes);
                subidas++;
            }

            TempData["Exito"] = subidas == 1
                ? "Imagen agregada a la galería con éxito."
                : $"{subidas} imágenes agregadas a la galería con éxito.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> Editar(int id)
        {
            var dto = await _imagenService.ObtenerPorIdAsync(id);
            if (dto == null) return NotFound();

            await CargarCategoriasAsync();
            return View("Formulario", dto);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ImagenDTO dto, List<IFormFile>? archivos, IFormFile? archivo)
        {
            var archivoSubido = (archivos != null && archivos.Count > 0)
                ? archivos.FirstOrDefault(a => a != null && a.Length > 0)
                : (archivo != null && archivo.Length > 0 ? archivo : null);

            if (!ModelState.IsValid) { await CargarCategoriasAsync(); return View("Formulario", dto); }

            byte[]? bytes = null;
            if (archivoSubido != null)
            {
                if (!EsImagenValida(archivoSubido))
                {
                    ModelState.AddModelError("archivos", "Solo se permiten imágenes (jpg, png, webp, gif).");
                    await CargarCategoriasAsync();
                    return View("Formulario", dto);
                }
                bytes = await LeerBytesAsync(archivoSubido);
            }

            await _imagenService.EditarAsync(id, dto, bytes);
            TempData["Exito"] = "Imagen actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet, Authorize]
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
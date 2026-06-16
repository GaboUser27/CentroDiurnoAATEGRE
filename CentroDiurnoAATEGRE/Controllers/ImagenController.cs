using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CentroDiurnoAATEGRE.Web.Controllers
{
    public class ImagenController : Controller
    {
        private readonly IImagenRepository _imagenRepo;
        private readonly ICategoriaImagenRepository _categoriaRepo;
        private readonly IWebHostEnvironment _env;

        public ImagenController(
            IImagenRepository imagenRepo,
            ICategoriaImagenRepository categoriaRepo,
            IWebHostEnvironment env)
        {
            _imagenRepo    = imagenRepo;
            _categoriaRepo = categoriaRepo;
            _env           = env;
        }

        // ── PÚBLICO ────────────────────────────────────────────────────

        // GET: /Imagen/Galeria  (página pública)
        [AllowAnonymous]
        public async Task<IActionResult> Galeria(int? categoriaId)
        {
            var categorias = await _categoriaRepo.ObtenerTodosAsync();
            ViewBag.Categorias = categorias;

            IEnumerable<Imagen> imagenes;
            if (categoriaId.HasValue)
            {
                imagenes = await _imagenRepo.ObtenerPorCategoriaAsync(categoriaId.Value);
                var cat = categorias.FirstOrDefault(c => c.IdCategoriaImagen == categoriaId.Value);
                ViewBag.CategoriaActual     = categoriaId.Value;
                ViewBag.NombreCategoria     = cat?.Nombre;
                ViewBag.DescripcionCategoria= cat?.Descripcion;
            }
            else
            {
                imagenes = await _imagenRepo.ObtenerConCategoriaAsync();
            }

            return View(imagenes);
        }

        // ── ADMIN ──────────────────────────────────────────────────────

        // GET: /Imagen  (lista admin)
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var imagenes = await _imagenRepo.ObtenerConCategoriaAsync();
            return View(imagenes);
        }

        // GET: /Imagen/Crear
        [Authorize]
        public async Task<IActionResult> Crear()
        {
            await CargarCategoriasAsync();
            return View("Formulario", new ImagenDTO { FechaImagen = DateTime.Now });
        }

        // POST: /Imagen/Crear
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ImagenDTO dto, IFormFile? archivo)
        {
            if (!ModelState.IsValid) { await CargarCategoriasAsync(); return View("Formulario", dto); }

            string? rutaArchivo = null;

            if (archivo != null && archivo.Length > 0)
            {
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();

                if (!extensionesPermitidas.Contains(ext))
                {
                    ModelState.AddModelError("archivo", "Solo se permiten imágenes (jpg, png, webp, gif).");
                    await CargarCategoriasAsync();
                    return View("Formulario", dto);
                }

                var nombreArchivo = $"{Guid.NewGuid()}{ext}";
                var uploadsPath   = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);

                var rutaCompleta = Path.Combine(uploadsPath, nombreArchivo);
                using var stream = new FileStream(rutaCompleta, FileMode.Create);
                await archivo.CopyToAsync(stream);

                rutaArchivo = nombreArchivo;
            }

            var imagen = new Imagen
            {
                Titulo            = dto.Titulo,
                Descripcion       = dto.Descripcion,
                FechaImagen       = dto.FechaImagen,
                IdCategoriaImagen = dto.IdCategoriaImagen,
                RutaArchivo       = rutaArchivo
            };

            await _imagenRepo.AgregarAsync(imagen);
            TempData["Exito"] = "Imagen agregada a la galería.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Imagen/Editar/5
        [Authorize]
        public async Task<IActionResult> Editar(int id)
        {
            var imagen = await _imagenRepo.ObtenerPorIdAsync(id);
            if (imagen == null) return NotFound();

            await CargarCategoriasAsync();
            var dto = new ImagenDTO
            {
                IdImagen          = imagen.IdImagen,
                Titulo            = imagen.Titulo,
                Descripcion       = imagen.Descripcion,
                FechaImagen       = imagen.FechaImagen,
                IdCategoriaImagen = imagen.IdCategoriaImagen,
                RutaArchivo       = imagen.RutaArchivo
            };
            return View("Formulario", dto);
        }

        // POST: /Imagen/Editar/5
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ImagenDTO dto, IFormFile? archivo)
        {
            if (!ModelState.IsValid) { await CargarCategoriasAsync(); return View("Formulario", dto); }

            var imagen = await _imagenRepo.ObtenerPorIdAsync(id);
            if (imagen == null) return NotFound();

            if (archivo != null && archivo.Length > 0)
            {
                // Eliminar archivo anterior
                if (!string.IsNullOrEmpty(imagen.RutaArchivo))
                {
                    var rutaAnterior = Path.Combine(_env.WebRootPath, "uploads", imagen.RutaArchivo);
                    if (System.IO.File.Exists(rutaAnterior))
                        System.IO.File.Delete(rutaAnterior);
                }

                var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                var nombreArchivo = $"{Guid.NewGuid()}{ext}";
                var rutaCompleta  = Path.Combine(_env.WebRootPath, "uploads", nombreArchivo);

                using var stream = new FileStream(rutaCompleta, FileMode.Create);
                await archivo.CopyToAsync(stream);
                imagen.RutaArchivo = nombreArchivo;
            }

            imagen.Titulo            = dto.Titulo;
            imagen.Descripcion       = dto.Descripcion;
            imagen.FechaImagen       = dto.FechaImagen;
            imagen.IdCategoriaImagen = dto.IdCategoriaImagen;

            await _imagenRepo.ActualizarAsync(imagen);
            TempData["Exito"] = "Imagen actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Imagen/Eliminar/5
        [Authorize]
        public async Task<IActionResult> Eliminar(int id)
        {
            var imagen = await _imagenRepo.ObtenerPorIdAsync(id);
            if (imagen != null)
            {
                // Eliminar archivo físico
                if (!string.IsNullOrEmpty(imagen.RutaArchivo))
                {
                    var ruta = Path.Combine(_env.WebRootPath, "uploads", imagen.RutaArchivo);
                    if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
                }
                await _imagenRepo.EliminarAsync(id);
            }
            TempData["Exito"] = "Imagen eliminada.";
            return RedirectToAction(nameof(Index));
        }

        // ── PRIVADO ────────────────────────────────────────────────────
        private async Task CargarCategoriasAsync()
        {
            var cats = await _categoriaRepo.ObtenerTodosAsync();
            ViewBag.Categorias = new SelectList(cats, "IdCategoriaImagen", "Nombre");
        }
    }
}

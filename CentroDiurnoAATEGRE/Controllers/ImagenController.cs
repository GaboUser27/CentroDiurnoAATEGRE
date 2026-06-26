using AutoMapper;
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
        private readonly IMapper _mapper;

        public ImagenController(
            IImagenRepository imagenRepo,
            ICategoriaImagenRepository categoriaRepo,
            IWebHostEnvironment env,
            IMapper mapper)
        {
            _imagenRepo = imagenRepo;
            _categoriaRepo = categoriaRepo;
            _env = env;
            _mapper = mapper;
        }

        // ── PÚBLICO ────────────────────────────────────────────────────

        // GET: /Imagen/Galeria
        [AllowAnonymous]
        public async Task<IActionResult> Galeria(int? categoriaId)
        {
            var categorias = await _categoriaRepo.ObtenerTodosAsync();
            ViewBag.Categorias = _mapper.Map<IEnumerable<CategoriaImagenDTO>>(categorias);

            IEnumerable<Imagen> imagenes;
            if (categoriaId.HasValue)
            {
                imagenes = await _imagenRepo.ObtenerPorCategoriaAsync(categoriaId.Value);
                var cat = categorias.FirstOrDefault(c => c.IdCategoriaImagen == categoriaId.Value);
                ViewBag.CategoriaActual = categoriaId.Value;
                ViewBag.NombreCategoria = cat?.Nombre;
                ViewBag.DescripcionCategoria = cat?.Descripcion;
            }
            else
            {
                imagenes = await _imagenRepo.ObtenerConCategoriaAsync();
            }

            var dtos = _mapper.Map<IEnumerable<ImagenDTO>>(imagenes);
            return View(dtos);
        }

        // ── ADMIN ──────────────────────────────────────────────────────

        // GET: /Imagen
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var imagenes = await _imagenRepo.ObtenerConCategoriaAsync();
            var dtos = _mapper.Map<IEnumerable<ImagenDTO>>(imagenes);
            return View(dtos);
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

            var imagen = _mapper.Map<Imagen>(dto);
            imagen.RutaArchivo = await GuardarArchivoAsync(archivo);

            if (imagen.RutaArchivo == null && archivo != null)
            {
                // Hubo error de extensión
                ModelState.AddModelError("archivo", "Solo se permiten imágenes (jpg, png, webp, gif).");
                await CargarCategoriasAsync();
                return View("Formulario", dto);
            }

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
            var dto = _mapper.Map<ImagenDTO>(imagen);
            return View("Formulario", dto);
        }

        // POST: /Imagen/Editar/5
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ImagenDTO dto, IFormFile? archivo)
        {
            if (!ModelState.IsValid) { await CargarCategoriasAsync(); return View("Formulario", dto); }

            var imagen = await _imagenRepo.ObtenerPorIdAsync(id);
            if (imagen == null) return NotFound();

            // Guardar ruta anterior por si hay nuevo archivo
            var rutaAnterior = imagen.RutaArchivo;

            // Mapear campos editables (RutaArchivo se ignora en el perfil)
            _mapper.Map(dto, imagen);

            if (archivo != null && archivo.Length > 0)
            {
                var nuevaRuta = await GuardarArchivoAsync(archivo);
                if (nuevaRuta == null)
                {
                    ModelState.AddModelError("archivo", "Solo se permiten imágenes (jpg, png, webp, gif).");
                    await CargarCategoriasAsync();
                    return View("Formulario", dto);
                }

                // Eliminar archivo anterior
                EliminarArchivo(rutaAnterior);
                imagen.RutaArchivo = nuevaRuta;
            }
            else
            {
                // Conservar la ruta anterior
                imagen.RutaArchivo = rutaAnterior;
            }

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
                EliminarArchivo(imagen.RutaArchivo);
                await _imagenRepo.EliminarAsync(id);
            }
            TempData["Exito"] = "Imagen eliminada.";
            return RedirectToAction(nameof(Index));
        }

        // ── PRIVADOS ───────────────────────────────────────────────────

        private async Task CargarCategoriasAsync()
        {
            var cats = await _categoriaRepo.ObtenerTodosAsync();
            ViewBag.Categorias = new SelectList(cats, "IdCategoriaImagen", "Nombre");
        }

        private async Task<string?> GuardarArchivoAsync(IFormFile? archivo)
        {
            if (archivo == null || archivo.Length == 0) return null;

            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!extensionesPermitidas.Contains(ext)) return null;

            var nombreArchivo = $"{Guid.NewGuid()}{ext}";
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);

            var rutaCompleta = Path.Combine(uploadsPath, nombreArchivo);
            using var stream = new FileStream(rutaCompleta, FileMode.Create);
            await archivo.CopyToAsync(stream);

            return nombreArchivo;
        }

        private void EliminarArchivo(string? rutaArchivo)
        {
            if (string.IsNullOrEmpty(rutaArchivo)) return;
            var ruta = Path.Combine(_env.WebRootPath, "uploads", rutaArchivo);
            if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
        }
    }
}
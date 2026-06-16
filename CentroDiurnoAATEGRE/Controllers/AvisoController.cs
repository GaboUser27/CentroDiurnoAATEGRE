using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentroDiurnoAATEGRE.Web.Controllers
{
    [Authorize]
    public class AvisoController : Controller
    {
        private readonly IAvisoRepository _avisoRepo;

        public AvisoController(IAvisoRepository avisoRepo)
        {
            _avisoRepo = avisoRepo;
        }

        // GET: /Aviso
        public async Task<IActionResult> Index()
        {
            var avisos = await _avisoRepo.ObtenerTodosAsync();
            return View(avisos.OrderByDescending(a => a.FechaPublicacion));
        }

        // GET: /Aviso/Crear
        public IActionResult Crear()
        {
            ViewData["Title"] = "Nuevo Aviso";
            var dto = new AvisoDTO
            {
                FechaPublicacion = DateTime.Now,
                Activo = true
            };
            return View("Formulario", dto);
        }

        // POST: /Aviso/Crear
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(AvisoDTO dto)
        {
            ViewData["Title"] = "Nuevo Aviso";
            if (!ModelState.IsValid) return View("Formulario", dto);

            var aviso = new Aviso
            {
                Titulo = dto.Titulo,
                Contenido = dto.Contenido,
                FechaPublicacion = dto.FechaPublicacion,
                FechaExpiracion = dto.FechaExpiracion,
                Activo = dto.Activo
            };

            await _avisoRepo.AgregarAsync(aviso);
            TempData["Exito"] = "Aviso creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Aviso/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            ViewData["Title"] = "Editar Aviso";
            var aviso = await _avisoRepo.ObtenerPorIdAsync(id);
            if (aviso == null) return NotFound();

            var dto = new AvisoDTO
            {
                IdAviso = aviso.IdAviso,
                Titulo = aviso.Titulo,
                Contenido = aviso.Contenido,
                FechaPublicacion = aviso.FechaPublicacion,
                FechaExpiracion = aviso.FechaExpiracion,
                Activo = aviso.Activo
            };

            return View("Formulario", dto);
        }

        // POST: /Aviso/Editar/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, AvisoDTO dto)
        {
            ViewData["Title"] = "Editar Aviso";
            if (!ModelState.IsValid) return View("Formulario", dto);

            var aviso = await _avisoRepo.ObtenerPorIdAsync(id);
            if (aviso == null) return NotFound();

            aviso.Titulo = dto.Titulo;
            aviso.Contenido = dto.Contenido;
            aviso.FechaPublicacion = dto.FechaPublicacion;
            aviso.FechaExpiracion = dto.FechaExpiracion;
            aviso.Activo = dto.Activo;

            await _avisoRepo.ActualizarAsync(aviso);
            TempData["Exito"] = "Aviso actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Aviso/Eliminar/5
        public async Task<IActionResult> Eliminar(int id)
        {
            await _avisoRepo.EliminarAsync(id);
            TempData["Exito"] = "Aviso eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
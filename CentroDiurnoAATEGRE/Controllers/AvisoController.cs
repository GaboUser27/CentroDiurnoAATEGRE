using AutoMapper;
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
        private readonly IMapper _mapper;

        public AvisoController(IAvisoRepository avisoRepo, IMapper mapper)
        {
            _avisoRepo = avisoRepo;
            _mapper = mapper;
        }

        // GET: /Aviso
        public async Task<IActionResult> Index()
        {
            var avisos = await _avisoRepo.ObtenerTodosAsync();
            var dtos = _mapper.Map<IEnumerable<AvisoDTO>>(avisos.OrderByDescending(a => a.FechaPublicacion));
            return View(dtos);
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

            var aviso = _mapper.Map<Aviso>(dto);
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

            var dto = _mapper.Map<AvisoDTO>(aviso);
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

            _mapper.Map(dto, aviso);
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
using AutoMapper;
using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Application.Services.Interfaces;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CentroDiurnoAATEGRE.Web.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IGenericRepository<Rol> _rolRepo;
        private readonly IGenericRepository<EstadoUsuario> _estadoRepo;

        public UsuarioController(
            IUsuarioService usuarioService,
            IGenericRepository<Rol> rolRepo,
            IGenericRepository<EstadoUsuario> estadoRepo)
        {
            _usuarioService = usuarioService;
            _rolRepo = rolRepo;
            _estadoRepo = estadoRepo;
        }

        // ── LOGIN ──────────────────────────────────────────────────────

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard", "Home");
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(RolDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var usuario = await _usuarioService.ValidarLoginAsync(dto.Correo, dto.Contrasena);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos, o cuenta inactiva.";
                return View(dto);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name,  usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role,  usuario.NombreRol ?? "Colaborador")
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("CookieAuth", principal);

            return RedirectToAction("Dashboard", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        // ── CRUD ───────────────────────────────────────────────────────

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var dtos = await _usuarioService.ObtenerTodosAsync();
            return View(dtos);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear()
        {
            await CargarSelectsAsync();
            return View("Formulario", new UsuarioDTO { IdEstadoUsuario = 1 });
        }

        [HttpPost, Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(UsuarioDTO dto)
        {
            if (!ModelState.IsValid) { await CargarSelectsAsync(); return View("Formulario", dto); }

            await _usuarioService.CrearAsync(dto);
            TempData["Exito"] = "Usuario creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Editar(int id)
        {
            var dto = await _usuarioService.ObtenerPorIdAsync(id);
            if (dto == null) return NotFound();

            await CargarSelectsAsync();
            return View("Formulario", dto);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, UsuarioDTO dto)
        {
            ModelState.Remove(nameof(dto.Contrasena));
            if (!ModelState.IsValid) { await CargarSelectsAsync(); return View("Formulario", dto); }

            await _usuarioService.EditarAsync(id, dto);
            TempData["Exito"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            await _usuarioService.CambiarEstadoAsync(id);
            TempData["Exito"] = "Estado del usuario actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // ── PRIVADO ────────────────────────────────────────────────────

        private async Task CargarSelectsAsync()
        {
            var roles = await _rolRepo.ObtenerTodosAsync();
            var estados = await _estadoRepo.ObtenerTodosAsync();
            ViewBag.Roles = new SelectList(roles, "IdRol", "Nombre");
            ViewBag.Estados = new SelectList(estados, "IdEstadoUsuario", "Nombre");
        }
    }
}
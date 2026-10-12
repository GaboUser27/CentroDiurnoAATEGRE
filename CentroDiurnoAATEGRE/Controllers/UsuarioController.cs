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
using Microsoft.AspNetCore.RateLimiting;
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

        [HttpGet, AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard", "Home");
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken, EnableRateLimiting("login")]
        public async Task<IActionResult> Login(UsuarioDTO dto)
        {
            // El formulario de login solo envía Correo y Contraseña.
            // El resto de campos de UsuarioDTO (Nombre, IdRol, etc.) son
            // requeridos para el CRUD de usuarios, no para el login,
            // así que se excluyen de la validación aquí.
            ModelState.Remove(nameof(dto.Nombre));
            ModelState.Remove(nameof(dto.IdRol));
            ModelState.Remove(nameof(dto.IdEstadoUsuario));
            ModelState.Remove(nameof(dto.Contrasena));

            if (string.IsNullOrWhiteSpace(dto.Correo) || string.IsNullOrEmpty(dto.Contrasena) || !ModelState.IsValid)
            {
                ViewBag.Error = "Correo o contraseña incorrectos, o cuenta inactiva.";
                return View(dto);
            }

            var usuario = await _usuarioService.ValidarLoginAsync(dto.Correo, dto.Contrasena ?? string.Empty);

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
                new Claim(ClaimTypes.Role,  usuario.IdRolNavigation?.Nombre ?? "Colaborador")
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("CookieAuth", principal);

            return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        // ── CRUD ───────────────────────────────────────────────────────

        [HttpGet, Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var dtos = await _usuarioService.ObtenerTodosAsync();
            return View(dtos);
        }

        [HttpGet, Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear()
        {
            await CargarSelectsAsync();
            return View("Formulario", new UsuarioDTO { IdEstadoUsuario = 1 });
        }

        [HttpPost, Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(UsuarioDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Contrasena))
                ModelState.AddModelError(nameof(dto.Contrasena), "La contraseña es requerida.");
            if (!ModelState.IsValid) { await CargarSelectsAsync(); return View("Formulario", dto); }

            await _usuarioService.CrearAsync(dto);
            TempData["Exito"] = "Usuario creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet, Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int id)
        {
            var dto = await _usuarioService.ObtenerPorIdAsync(id);
            if (dto == null) return NotFound();

            await CargarSelectsAsync();
            return View("Formulario", dto);
        }

        [HttpPost, Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, UsuarioDTO dto)
        {
            dto.IdUsuario = id;
            var usuarioActual = await _usuarioService.ObtenerPorIdAsync(id);
            if (usuarioActual == null) return NotFound();

            var esAutogestion = id.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (esAutogestion && (dto.IdRol != usuarioActual.IdRol || dto.IdEstadoUsuario != usuarioActual.IdEstadoUsuario))
            {
                ModelState.AddModelError(string.Empty, "No puede cambiar su propio rol ni el estado de su cuenta.");
            }

            var roles = await _rolRepo.ObtenerTodosAsync();
            var nombreNuevoRol = roles.FirstOrDefault(r => r.IdRol == dto.IdRol)?.Nombre;
            var pierdeUltimoAdminActivo = usuarioActual.IdRolNavigation?.Nombre == "Administrador"
                && usuarioActual.IdEstadoUsuarioNavigation?.Nombre == "Activo"
                && (nombreNuevoRol != "Administrador" || dto.IdEstadoUsuario != usuarioActual.IdEstadoUsuario);
            if (pierdeUltimoAdminActivo)
            {
                var usuarios = await _usuarioService.ObtenerTodosAsync();
                var adminsActivos = usuarios.Count(u => u.IdRolNavigation?.Nombre == "Administrador"
                    && u.IdEstadoUsuarioNavigation?.Nombre == "Activo");
                if (adminsActivos <= 1)
                    ModelState.AddModelError(string.Empty, "Debe existir al menos un administrador activo.");
            }

            ModelState.Remove(nameof(dto.Contrasena));
            if (!string.IsNullOrWhiteSpace(dto.Contrasena) && dto.Contrasena.Length < 12)
                ModelState.AddModelError(nameof(dto.Contrasena), "Use al menos 12 caracteres.");
            if (!ModelState.IsValid) { await CargarSelectsAsync(); return View("Formulario", dto); }

            await _usuarioService.EditarAsync(id, dto);
            TempData["Exito"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);
            if (usuario == null) return NotFound();
            if (id.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                TempData["Error"] = "No puede cambiar el estado de su propia cuenta.";
                return RedirectToAction(nameof(Index));
            }
            if (usuario.IdRolNavigation?.Nombre == "Administrador"
                && usuario.IdEstadoUsuarioNavigation?.Nombre == "Activo")
            {
                var usuarios = await _usuarioService.ObtenerTodosAsync();
                if (usuarios.Count(u => u.IdRolNavigation?.Nombre == "Administrador"
                    && u.IdEstadoUsuarioNavigation?.Nombre == "Activo") <= 1)
                {
                    TempData["Error"] = "Debe existir al menos un administrador activo.";
                    return RedirectToAction(nameof(Index));
                }
            }
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

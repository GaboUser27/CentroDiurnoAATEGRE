using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace CentroDiurnoAATEGRE.Web.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IGenericRepository<Rol> _rolRepo;
        private readonly IGenericRepository<EstadoUsuario> _estadoRepo;

        public UsuarioController(
            IUsuarioRepository usuarioRepo,
            IGenericRepository<Rol> rolRepo,
            IGenericRepository<EstadoUsuario> estadoRepo)
        {
            _usuarioRepo = usuarioRepo;
            _rolRepo     = rolRepo;
            _estadoRepo  = estadoRepo;
        }

        // ── LOGIN ──────────────────────────────────────────────────────

        // GET: /Usuario/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard", "Home");
            return View();
        }

        // POST: /Usuario/Login
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var usuario = await _usuarioRepo.ObtenerPorCorreoAsync(dto.Correo);

            if (usuario == null)
            {
                ViewBag.Error = "El correo electrónico no está registrado.";
                return View(dto);
            }

            if (usuario.EstadoUsuario?.Nombre != "Activo")
            {
                ViewBag.Error = "Su cuenta está inactiva. Contacte al administrador.";
                return View(dto);
            }

            // Comparar contraseña (en producción usar BCrypt o similar)
            if (usuario.Contrasena != dto.Contrasena)
            {
                ViewBag.Error = "Contraseña incorrecta.";
                return View(dto);
            }

            // Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "Colaborador")
            };

            var identity  = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("CookieAuth", principal);
            return RedirectToAction("Dashboard", "Home");
        }

        // GET: /Usuario/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        // ── CRUD USUARIOS ──────────────────────────────────────────────

        // GET: /Usuario
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _usuarioRepo.ObtenerConRolesYEstadosAsync();
            return View(usuarios);
        }

        // GET: /Usuario/Crear
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear()
        {
            await CargarSelectsAsync();
            return View("Formulario", new UsuarioDTO { IdEstadoUsuario = 1 });
        }

        // POST: /Usuario/Crear
        [HttpPost, Authorize(Roles = "Administrador"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(UsuarioDTO dto)
        {
            if (!ModelState.IsValid) { await CargarSelectsAsync(); return View("Formulario", dto); }

            var usuario = new Usuario
            {
                Nombre           = dto.Nombre,
                Correo           = dto.Correo,
                Contrasena       = dto.Contrasena!, // Hash en producción
                IdRol            = dto.IdRol,
                IdEstadoUsuario  = dto.IdEstadoUsuario
            };

            await _usuarioRepo.AgregarAsync(usuario);
            TempData["Exito"] = "Usuario creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Usuario/Editar/5
        [Authorize]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
            if (usuario == null) return NotFound();

            await CargarSelectsAsync();
            var dto = new UsuarioDTO
            {
                IdUsuario       = usuario.IdUsuario,
                Nombre          = usuario.Nombre,
                Correo          = usuario.Correo,
                IdRol           = usuario.IdRol,
                IdEstadoUsuario = usuario.IdEstadoUsuario
            };
            return View("Formulario", dto);
        }

        // POST: /Usuario/Editar/5
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, UsuarioDTO dto)
        {
            // La contraseña no es requerida al editar
            ModelState.Remove(nameof(dto.Contrasena));
            if (!ModelState.IsValid) { await CargarSelectsAsync(); return View("Formulario", dto); }

            var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
            if (usuario == null) return NotFound();

            usuario.Nombre          = dto.Nombre;
            usuario.Correo          = dto.Correo;
            usuario.IdRol           = dto.IdRol;
            usuario.IdEstadoUsuario = dto.IdEstadoUsuario;

            if (!string.IsNullOrWhiteSpace(dto.Contrasena))
                usuario.Contrasena = dto.Contrasena;

            await _usuarioRepo.ActualizarAsync(usuario);
            TempData["Exito"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Usuario/CambiarEstado/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
            if (usuario == null) return NotFound();

            // Toggle: 1=Activo → 2=Inactivo y viceversa
            int nuevoEstado = usuario.IdEstadoUsuario == 1 ? 2 : 1;
            await _usuarioRepo.CambiarEstadoAsync(id, nuevoEstado);

            TempData["Exito"] = nuevoEstado == 1
                ? "Usuario activado."
                : "Usuario desactivado.";
            return RedirectToAction(nameof(Index));
        }

        // ── PRIVADO ────────────────────────────────────────────────────
        private async Task CargarSelectsAsync()
        {
            var roles   = await _rolRepo.ObtenerTodosAsync();
            var estados = await _estadoRepo.ObtenerTodosAsync();

            ViewBag.Roles = new SelectList(roles, "IdRol", "Nombre");
            ViewBag.Estados = new SelectList(estados, "IdEstadoUsuario", "Nombre");
        }
    }
}

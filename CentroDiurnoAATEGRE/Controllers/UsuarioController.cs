using AutoMapper;
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
        private readonly IMapper _mapper;

        public UsuarioController(
            IUsuarioRepository usuarioRepo,
            IGenericRepository<Rol> rolRepo,
            IGenericRepository<EstadoUsuario> estadoRepo,
            IMapper mapper)
        {
            _usuarioRepo = usuarioRepo;
            _rolRepo = rolRepo;
            _estadoRepo = estadoRepo;
            _mapper = mapper;
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

            if (usuario.Contrasena != dto.Contrasena)
            {
                ViewBag.Error = "Contraseña incorrecta.";
                return View(dto);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name,  usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role,  usuario.Rol?.Nombre ?? "Colaborador")
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
            var usuarios = await _usuarioRepo.ObtenerConRolesYEstadosAsync();
            var dtos = _mapper.Map<IEnumerable<UsuarioDTO>>(usuarios);
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

            var usuario = _mapper.Map<Usuario>(dto);
            await _usuarioRepo.AgregarAsync(usuario);

            TempData["Exito"] = "Usuario creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
            if (usuario == null) return NotFound();

            await CargarSelectsAsync();
            var dto = _mapper.Map<UsuarioDTO>(usuario);
            return View("Formulario", dto);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, UsuarioDTO dto)
        {
            ModelState.Remove(nameof(dto.Contrasena));
            if (!ModelState.IsValid) { await CargarSelectsAsync(); return View("Formulario", dto); }

            var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
            if (usuario == null) return NotFound();

            // Guardar contraseña actual antes del mapeo
            var contrasenaActual = usuario.Contrasena;

            _mapper.Map(dto, usuario);

            // Restaurar o actualizar contraseña
            usuario.Contrasena = !string.IsNullOrWhiteSpace(dto.Contrasena)
                ? dto.Contrasena
                : contrasenaActual;

            await _usuarioRepo.ActualizarAsync(usuario);
            TempData["Exito"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
            if (usuario == null) return NotFound();

            int nuevoEstado = usuario.IdEstadoUsuario == 1 ? 2 : 1;
            await _usuarioRepo.CambiarEstadoAsync(id, nuevoEstado);

            TempData["Exito"] = nuevoEstado == 1 ? "Usuario activado." : "Usuario desactivado.";
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
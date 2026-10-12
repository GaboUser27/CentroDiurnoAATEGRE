using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BCrypt.Net;
using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Application.Services.Interfaces;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;

namespace CentroDiurnoAATEGRE.Application.Services.Implementations
{
    public class UsuarioService : IUsuarioService
    {
        private const int MaxIntentosFallidos = 5;
        private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);
        // Hash BCrypt válido usado para mantener un costo comparable cuando el correo no existe.
        private const string HashDummy = "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy";
        private readonly IUsuarioRepository _repo;
        private readonly IGenericRepository<Rol> _rolRepo;
        private readonly IMapper _mapper;

        public UsuarioService(
            IUsuarioRepository repo,
            IGenericRepository<Rol> rolRepo,
            IMapper mapper)
        {
            _repo = repo;
            _rolRepo = rolRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UsuarioDTO>> ObtenerTodosAsync()
        {
            var usuarios = await _repo.ObtenerConRolesYEstadosAsync();
            return _mapper.Map<IEnumerable<UsuarioDTO>>(usuarios);
        }

        public async Task<UsuarioDTO?> ObtenerPorIdAsync(int id)
        {
            var usuario = await _repo.ObtenerPorIdAsync(id);
            return usuario == null ? null : _mapper.Map<UsuarioDTO>(usuario);
        }

        public async Task<UsuarioDTO?> ValidarLoginAsync(string correo, string contrasena)
        {
            var usuario = await _repo.ObtenerPorCorreoAsync(correo);
            if (usuario == null)
            {
                BCrypt.Net.BCrypt.Verify(contrasena, HashDummy);
                return null;
            }

            if (usuario.IdEstadoUsuarioNavigation?.Nombre != "Activo"
                || usuario.BloqueadoHasta > DateTime.UtcNow)
            {
                BCrypt.Net.BCrypt.Verify(contrasena, HashDummy);
                return null;
            }

            bool esValida = false;
            if (!string.IsNullOrEmpty(usuario.Contrasena) &&
                (usuario.Contrasena.StartsWith("$2a$") || usuario.Contrasena.StartsWith("$2b$") || usuario.Contrasena.StartsWith("$2y$")))
            {
                try
                {
                    esValida = BCrypt.Net.BCrypt.Verify(contrasena, usuario.Contrasena);
                }
                catch
                {
                    esValida = false;
                    BCrypt.Net.BCrypt.Verify(contrasena, HashDummy);
                }
            }
            else
            {
                BCrypt.Net.BCrypt.Verify(contrasena, HashDummy);
            }

            if (!esValida)
            {
                await _repo.RegistrarIntentoFallidoAsync(
                    usuario.IdUsuario, MaxIntentosFallidos, DateTime.UtcNow.Add(DuracionBloqueo));
                return null;
            }

            if (usuario.IntentosFallidos > 0 || usuario.BloqueadoHasta.HasValue)
                await _repo.ReiniciarIntentosFallidosAsync(usuario.IdUsuario);

            return _mapper.Map<UsuarioDTO>(usuario);
        }

        public async Task CrearAsync(UsuarioDTO dto)
        {
            var usuario = _mapper.Map<Usuario>(dto);
            if (!string.IsNullOrWhiteSpace(dto.Contrasena))
            {
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena);
            }
            await _repo.AgregarAsync(usuario);
        }

        public async Task EditarAsync(int id, UsuarioDTO dto)
        {
            var usuario = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

            var contrasenaActual = usuario.Contrasena;
            _mapper.Map(dto, usuario);

            usuario.Contrasena = !string.IsNullOrWhiteSpace(dto.Contrasena)
                ? BCrypt.Net.BCrypt.HashPassword(dto.Contrasena)
                : contrasenaActual;

            await _repo.ActualizarAsync(usuario);
        }

        public async Task CambiarEstadoAsync(int id)
        {
            var usuario = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

            int nuevoEstado = usuario.IdEstadoUsuario == 1 ? 2 : 1;
            await _repo.CambiarEstadoAsync(id, nuevoEstado);
        }
    }
}

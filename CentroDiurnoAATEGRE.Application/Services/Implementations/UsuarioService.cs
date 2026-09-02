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
            if (usuario == null) return null;
            if (usuario.IdEstadoUsuarioNavigation?.Nombre != "Activo") return null;

            bool esValida = false;

            // Verificar si la contraseña almacenada está en formato hash BCrypt
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
                }
            }
            else
            {
                // Compatibilidad hacia atrás para contraseñas existentes antes de la encriptación
                if (usuario.Contrasena == contrasena)
                {
                    esValida = true;
                    // Auto-migrar la contraseña a hash BCrypt en la base de datos
                    usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(contrasena);
                    await _repo.ActualizarAsync(usuario);
                }
            }

            if (!esValida) return null;

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

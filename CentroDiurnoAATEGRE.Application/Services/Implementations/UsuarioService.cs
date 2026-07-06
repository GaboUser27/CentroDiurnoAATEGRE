using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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
            if (usuario.EstadoUsuario?.Nombre != "Activo") return null;
            if (usuario.Contrasena != contrasena) return null;

            return _mapper.Map<UsuarioDTO>(usuario);
        }

        public async Task CrearAsync(UsuarioDTO dto)
        {
            var usuario = _mapper.Map<Usuario>(dto);
            await _repo.AgregarAsync(usuario);
        }

        public async Task EditarAsync(int id, UsuarioDTO dto)
        {
            var usuario = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

            var contrasenaActual = usuario.Contrasena;
            _mapper.Map(dto, usuario);

            usuario.Contrasena = !string.IsNullOrWhiteSpace(dto.Contrasena)
                ? dto.Contrasena
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

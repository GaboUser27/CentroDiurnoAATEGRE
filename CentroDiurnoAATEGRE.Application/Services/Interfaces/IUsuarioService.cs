using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Application.DTOs;

namespace CentroDiurnoAATEGRE.Application.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioDTO>> ObtenerTodosAsync();
        Task<UsuarioDTO?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteCorreoAsync(string correo, int? excluirIdUsuario = null);
        Task<DateTime?> ObtenerBloqueoActivoAsync(string correo);
        Task<bool> SesionVigenteAsync(int idUsuario, string rol);
        Task<UsuarioDTO?> ValidarLoginAsync(string correo, string contrasena);
        Task CrearAsync(UsuarioDTO dto);
        Task EditarAsync(int id, UsuarioDTO dto);
        Task CambiarEstadoAsync(int id);
    }
}

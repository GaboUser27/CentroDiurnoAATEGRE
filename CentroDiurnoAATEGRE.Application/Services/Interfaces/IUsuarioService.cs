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
        Task<UsuarioDTO?> ValidarLoginAsync(string correo, string contrasena);
        Task CrearAsync(UsuarioDTO dto);
        Task EditarAsync(int id, UsuarioDTO dto);
        Task CambiarEstadoAsync(int id);
    }
}

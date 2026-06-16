using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;

namespace CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces
{
    public interface IUsuarioRepository : IGenericRepository<Usuario>
    {
        Task<Usuario?> ObtenerPorCorreoAsync(string correo);
        Task<IEnumerable<Usuario>> ObtenerConRolesYEstadosAsync();
        Task CambiarEstadoAsync(int idUsuario, int idNuevoEstado);
    }
}

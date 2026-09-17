using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Application.DTOs;

namespace CentroDiurnoAATEGRE.Application.Services.Interfaces
{
    public interface IAvisoService
    {
        Task<IEnumerable<AvisoDTO>> ObtenerTodosAsync();
        Task<IEnumerable<AvisoDTO>> ObtenerVigentesAsync();
        Task<AvisoDTO?> ObtenerPorIdAsync(int id);
        Task CrearAsync(AvisoDTO dto);
        Task EditarAsync(int id, AvisoDTO dto);
        Task EliminarAsync(int id);

        /// Cantidad de avisos actualmente marcados como activos.
        Task<int> ContarActivosAsync();

        /// Inactiva los avisos cuya fecha de expiracion ya paso.
        /// Devuelve cuantos avisos se inactivaron.
        Task<int> DesactivarVencidosAsync();
    }
}

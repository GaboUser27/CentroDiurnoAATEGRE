using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;

namespace CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces
{
    public interface IAvisoRepository : IGenericRepository<Aviso>
    {
        Task<IEnumerable<Aviso>> ObtenerActivosAsync();
        Task<IEnumerable<Aviso>> ObtenerVigentesAsync(); // activos y no expirados
    }
}

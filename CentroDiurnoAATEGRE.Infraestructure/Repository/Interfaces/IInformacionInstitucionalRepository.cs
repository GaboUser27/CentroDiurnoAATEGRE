using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;

namespace CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces
{
    public interface IInformacionInstitucionalRepository : IGenericRepository<InformacionInstitucional>
    {
        Task<InformacionInstitucional?> ObtenerPrimeraAsync(); // Solo hay un registro
    }
}

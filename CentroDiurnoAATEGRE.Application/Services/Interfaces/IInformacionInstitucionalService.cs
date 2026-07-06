using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Application.DTOs;

namespace CentroDiurnoAATEGRE.Application.Services.Interfaces
{
    public interface IInformacionInstitucionalService
    {
        Task<InformacionInstitucionalDTO?> ObtenerPrimeraAsync();
        Task<InformacionInstitucionalDTO?> ObtenerPorIdAsync(int id);
        Task CrearAsync(InformacionInstitucionalDTO dto);
        Task EditarAsync(int id, InformacionInstitucionalDTO dto);
    }
}

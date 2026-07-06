using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using CentroDiurnoAATEGRE.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CentroDiurnoAATEGRE.Infraestructure.Repository.Implementations
{
    public class InformacionInstitucionalRepository
        : GenericRepository<InformacionInstitucional>, IInformacionInstitucionalRepository
    {
        public InformacionInstitucionalRepository(AategreeDbContext context) : base(context) { }

        public async Task<InformacionInstitucional?> ObtenerPrimeraAsync() =>
            await _context.InformacionInstitucional.FirstOrDefaultAsync();
    }
}

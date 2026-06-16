using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using CentroDiurnoAATEGRE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CentroDiurnoAATEGRE.Infraestructure.Repository.Implementations
{
    public class AvisoRepository : GenericRepository<Aviso>, IAvisoRepository
    {
        public AvisoRepository(AategreeDbContext context) : base(context) { }

        public async Task<IEnumerable<Aviso>> ObtenerActivosAsync() =>
            await _context.Avisos
                .Where(a => a.Activo)
                .OrderByDescending(a => a.FechaPublicacion)
                .ToListAsync();

        public async Task<IEnumerable<Aviso>> ObtenerVigentesAsync() =>
            await _context.Avisos
                .Where(a => a.Activo &&
                            (a.FechaExpiracion == null || a.FechaExpiracion >= DateTime.Now))
                .OrderByDescending(a => a.FechaPublicacion)
                .ToListAsync();
    }
}

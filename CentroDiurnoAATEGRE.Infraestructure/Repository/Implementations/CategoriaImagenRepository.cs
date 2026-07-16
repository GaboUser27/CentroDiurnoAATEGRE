
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
    public class CategoriaImagenRepository : GenericRepository<CategoriaImagen>, ICategoriaImagenRepository
    {
        public CategoriaImagenRepository(AATEGREContext context) : base(context) { }

        public async Task<IEnumerable<CategoriaImagen>> ObtenerConImagenesAsync() =>
            await _context.CategoriaImagen
                .Include(c => c.Imagen)
                .ToListAsync();
    }
}

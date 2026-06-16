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
    public class ImagenRepository : GenericRepository<Imagen>, IImagenRepository
    {
        public ImagenRepository(AategreeDbContext context) : base(context) { }

        public async Task<IEnumerable<Imagen>> ObtenerPorCategoriaAsync(int idCategoria) =>
            await _context.Imagenes
                .Include(i => i.CategoriaImagen)
                .Where(i => i.IdCategoriaImagen == idCategoria)
                .OrderByDescending(i => i.FechaImagen)
                .ToListAsync();

        public async Task<IEnumerable<Imagen>> ObtenerConCategoriaAsync() =>
            await _context.Imagenes
                .Include(i => i.CategoriaImagen)
                .OrderByDescending(i => i.FechaImagen)
                .ToListAsync();
    }
}

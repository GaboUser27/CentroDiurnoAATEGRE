using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Data;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CentroDiurnoAATEGRE.Infraestructure.Repository.Implementations
{
    public class ImagenRepository : GenericRepository<Imagen>, IImagenRepository
    {
        public ImagenRepository(AATEGREContext context) : base(context) { }

        public async Task<IEnumerable<Imagen>> ObtenerPorCategoriaAsync(int idCategoria) =>
            await _context.Imagen
                .Where(i => i.IdCategoriaImagen == idCategoria)
                .OrderByDescending(i => i.FechaImagen)
                .Select(i => new Imagen
                {
                    IdImagen = i.IdImagen,
                    Titulo = i.Titulo,
                    Descripcion = i.Descripcion,
                    FechaImagen = i.FechaImagen,
                    IdCategoriaImagen = i.IdCategoriaImagen,
                    TieneImagen = i.Imagen1 != null,
                    IdCategoriaImagenNavigation = new CategoriaImagen
                    {
                        IdCategoriaImagen = i.IdCategoriaImagenNavigation.IdCategoriaImagen,
                        Nombre = i.IdCategoriaImagenNavigation.Nombre,
                        Descripcion = i.IdCategoriaImagenNavigation.Descripcion
                    }
                })
                .ToListAsync();

        public async Task<IEnumerable<Imagen>> ObtenerConCategoriaAsync() =>
            await _context.Imagen
                .OrderByDescending(i => i.FechaImagen)
                .Select(i => new Imagen
                {
                    IdImagen = i.IdImagen,
                    Titulo = i.Titulo,
                    Descripcion = i.Descripcion,
                    FechaImagen = i.FechaImagen,
                    IdCategoriaImagen = i.IdCategoriaImagen,
                    TieneImagen = i.Imagen1 != null,
                    IdCategoriaImagenNavigation = new CategoriaImagen
                    {
                        IdCategoriaImagen = i.IdCategoriaImagenNavigation.IdCategoriaImagen,
                        Nombre = i.IdCategoriaImagenNavigation.Nombre,
                        Descripcion = i.IdCategoriaImagenNavigation.Descripcion
                    }
                })
                .ToListAsync();

        public async Task<byte[]?> ObtenerBytesImagenAsync(int id) =>
            await _context.Imagen.Where(i => i.IdImagen == id).Select(i => i.Imagen1).FirstOrDefaultAsync();
    }
}

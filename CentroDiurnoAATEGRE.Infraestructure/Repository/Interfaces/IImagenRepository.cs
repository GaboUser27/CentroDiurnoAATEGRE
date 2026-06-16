using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;

namespace CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces
{
    public interface IImagenRepository : IGenericRepository<Imagen>
    {
        Task<IEnumerable<Imagen>> ObtenerPorCategoriaAsync(int idCategoria);
        Task<IEnumerable<Imagen>> ObtenerConCategoriaAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;

namespace CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces
{
    public interface ICategoriaImagenRepository : IGenericRepository<CategoriaImagen>
    {
        Task<IEnumerable<CategoriaImagen>> ObtenerConImagenesAsync();
    }
}

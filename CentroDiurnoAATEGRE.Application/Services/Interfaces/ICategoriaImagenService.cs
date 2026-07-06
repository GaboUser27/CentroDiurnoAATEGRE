using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Application.DTOs;

namespace CentroDiurnoAATEGRE.Application.Services.Interfaces
{
    public interface ICategoriaImagenService
    {
        Task<IEnumerable<CategoriaImagenDTO>> ObtenerTodosAsync();
        Task<IEnumerable<CategoriaImagenDTO>> ObtenerConImagenesAsync();
        Task<CategoriaImagenDTO?> ObtenerPorIdAsync(int id);
        Task CrearAsync(CategoriaImagenDTO dto);
        Task EditarAsync(int id, CategoriaImagenDTO dto);
        Task<bool> EliminarAsync(int id); // false si tiene imágenes asociadas
    }
}

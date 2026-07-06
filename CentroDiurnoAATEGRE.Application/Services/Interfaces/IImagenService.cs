using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Application.DTOs;

namespace CentroDiurnoAATEGRE.Application.Services.Interfaces
{
    public interface IImagenService
    {
        Task<IEnumerable<ImagenDTO>> ObtenerTodosAsync();
        Task<IEnumerable<ImagenDTO>> ObtenerConCategoriaAsync();
        Task<IEnumerable<ImagenDTO>> ObtenerPorCategoriaAsync(int idCategoria);
        Task<ImagenDTO?> ObtenerPorIdAsync(int id);
        Task CrearAsync(ImagenDTO dto, byte[]? imagenBytes);
        Task EditarAsync(int id, ImagenDTO dto, byte[]? imagenBytes);
        Task EliminarAsync(int id);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Application.Services.Interfaces;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;

namespace CentroDiurnoAATEGRE.Application.Services.Implementations
{
    public class ImagenService : IImagenService
    {
        private readonly IImagenRepository _repo;
        private readonly IMapper _mapper;

        public ImagenService(IImagenRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ImagenDTO>> ObtenerTodosAsync()
        {
            var imagenes = await _repo.ObtenerTodosAsync();
            return _mapper.Map<IEnumerable<ImagenDTO>>(imagenes);
        }

        public async Task<IEnumerable<ImagenDTO>> ObtenerConCategoriaAsync()
        {
            var imagenes = await _repo.ObtenerConCategoriaAsync();
            return _mapper.Map<IEnumerable<ImagenDTO>>(imagenes);
        }

        public async Task<IEnumerable<ImagenDTO>> ObtenerPorCategoriaAsync(int idCategoria)
        {
            var imagenes = await _repo.ObtenerPorCategoriaAsync(idCategoria);
            return _mapper.Map<IEnumerable<ImagenDTO>>(imagenes);
        }

        public async Task<ImagenDTO?> ObtenerPorIdAsync(int id)
        {
            var imagen = await _repo.ObtenerPorIdAsync(id);
            return imagen == null ? null : _mapper.Map<ImagenDTO>(imagen);
        }

        public async Task CrearAsync(ImagenDTO dto, byte[]? imagenBytes)
        {
            var imagen = _mapper.Map<Imagen>(dto);
            imagen.ImagenBytes = imagenBytes;
            await _repo.AgregarAsync(imagen);
        }

        public async Task EditarAsync(int id, ImagenDTO dto, byte[]? imagenBytes)
        {
            var imagen = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Imagen {id} no encontrada.");

            var bytesAnteriores = imagen.ImagenBytes;
            _mapper.Map(dto, imagen);

            // Solo reemplaza si se subió una nueva imagen
            imagen.ImagenBytes = imagenBytes ?? bytesAnteriores;

            await _repo.ActualizarAsync(imagen);
        }

        public async Task EliminarAsync(int id) =>
            await _repo.EliminarAsync(id);
    }
}

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
    public class CategoriaImagenService : ICategoriaImagenService
    {
        private readonly ICategoriaImagenRepository _repo;
        private readonly IMapper _mapper;

        public CategoriaImagenService(ICategoriaImagenRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoriaImagenDTO>> ObtenerTodosAsync()
        {
            var cats = await _repo.ObtenerTodosAsync();
            return _mapper.Map<IEnumerable<CategoriaImagenDTO>>(cats);
        }

        public async Task<IEnumerable<CategoriaImagenDTO>> ObtenerConImagenesAsync()
        {
            var cats = await _repo.ObtenerConImagenesAsync();
            return _mapper.Map<IEnumerable<CategoriaImagenDTO>>(cats);
        }

        public async Task<CategoriaImagenDTO?> ObtenerPorIdAsync(int id)
        {
            var cat = await _repo.ObtenerPorIdAsync(id);
            return cat == null ? null : _mapper.Map<CategoriaImagenDTO>(cat);
        }

        public async Task CrearAsync(CategoriaImagenDTO dto)
        {
            var cat = _mapper.Map<CategoriaImagen>(dto);
            await _repo.AgregarAsync(cat);
        }

        public async Task EditarAsync(int id, CategoriaImagenDTO dto)
        {
            var cat = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Categoría {id} no encontrada.");
            _mapper.Map(dto, cat);
            await _repo.ActualizarAsync(cat);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var cats = await _repo.ObtenerConImagenesAsync();
            var categoria = cats.FirstOrDefault(c => c.IdCategoriaImagen == id);
            if (categoria == null) return false;
            if (categoria.Imagenes.Any()) return false; // tiene imágenes asociadas

            await _repo.EliminarAsync(id);
            return true;
        }
    }
}

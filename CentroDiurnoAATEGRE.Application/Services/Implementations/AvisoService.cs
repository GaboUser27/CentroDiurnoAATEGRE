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
    public class AvisoService : IAvisoService
    {
        private readonly IAvisoRepository _repo;
        private readonly IMapper _mapper;

        public AvisoService(IAvisoRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AvisoDTO>> ObtenerTodosAsync()
        {
            var avisos = await _repo.ObtenerTodosAsync();
            return _mapper.Map<IEnumerable<AvisoDTO>>(
                avisos.OrderByDescending(a => a.FechaPublicacion));
        }

        public async Task<IEnumerable<AvisoDTO>> ObtenerVigentesAsync()
        {
            var avisos = await _repo.ObtenerVigentesAsync();
            return _mapper.Map<IEnumerable<AvisoDTO>>(avisos);
        }

        public async Task<AvisoDTO?> ObtenerPorIdAsync(int id)
        {
            var aviso = await _repo.ObtenerPorIdAsync(id);
            return aviso == null ? null : _mapper.Map<AvisoDTO>(aviso);
        }

        public async Task CrearAsync(AvisoDTO dto)
        {
            var aviso = _mapper.Map<Aviso>(dto);
            await _repo.AgregarAsync(aviso);
        }

        public async Task EditarAsync(int id, AvisoDTO dto)
        {
            var aviso = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Aviso {id} no encontrado.");
            _mapper.Map(dto, aviso);
            await _repo.ActualizarAsync(aviso);
        }

        public async Task EliminarAsync(int id) =>
            await _repo.EliminarAsync(id);
    }
}

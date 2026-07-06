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
    public class InformacionInstitucionalService : IInformacionInstitucionalService
    {
        private readonly IInformacionInstitucionalRepository _repo;
        private readonly IMapper _mapper;

        public InformacionInstitucionalService(
            IInformacionInstitucionalRepository repo,
            IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<InformacionInstitucionalDTO?> ObtenerPrimeraAsync()
        {
            var info = await _repo.ObtenerPrimeraAsync();
            return info == null ? null : _mapper.Map<InformacionInstitucionalDTO>(info);
        }

        public async Task<InformacionInstitucionalDTO?> ObtenerPorIdAsync(int id)
        {
            var info = await _repo.ObtenerPorIdAsync(id);
            return info == null ? null : _mapper.Map<InformacionInstitucionalDTO>(info);
        }

        public async Task CrearAsync(InformacionInstitucionalDTO dto)
        {
            var info = _mapper.Map<InformacionInstitucional>(dto);
            await _repo.AgregarAsync(info);
        }

        public async Task EditarAsync(int id, InformacionInstitucionalDTO dto)
        {
            var info = await _repo.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"Información {id} no encontrada.");
            _mapper.Map(dto, info);
            await _repo.ActualizarAsync(info);
        }
    }
}

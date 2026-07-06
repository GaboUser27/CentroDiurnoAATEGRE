using AutoMapper;
using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentroDiurnoAATEGRE.Application.Profiles
{
    public class HorarioProfile : Profile
    {
        public HorarioProfile()
        {

            CreateMap<InformacionInstitucional, InformacionInstitucionalDTO>()
                            .ReverseMap();

            CreateMap<Horario, HorarioDTO>()
                .ForMember(d => d.InformacionInstitucional,
                    o => o.MapFrom(s => s.InformacionInstitucional));

            CreateMap<HorarioDTO, Horario>()
                .ForMember(d => d.InformacionInstitucional,
                    o => o.Ignore());

        }
    }
}

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
    public class EstadoUsuarioProfile : Profile
    {
        public EstadoUsuarioProfile()
        {

            CreateMap<EstadoUsuario, EstadoUsuarioDTO>()
                .ForMember(d => d.Usuario,
                    o => o.MapFrom(s => s.Usuario));

            CreateMap<EstadoUsuarioDTO, EstadoUsuario>()
                .ForMember(d => d.Usuario,
                    o => o.Ignore());

        }
    }
}

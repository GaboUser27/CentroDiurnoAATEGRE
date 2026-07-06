using AutoMapper;
using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Infraestructure.Models;

namespace CentroDiurnoAATEGRE.Application.Profiles
{
    public class RolProfile : Profile
    {
        public RolProfile()
        {
            CreateMap<Rol, RolDTO>()
                .ForMember(d => d.Usuario,
                    o => o.MapFrom(s => s.Usuario));

            CreateMap<RolDTO, Rol>()
                .ForMember(d => d.Usuario,
                    o => o.Ignore());
        }
    }
}
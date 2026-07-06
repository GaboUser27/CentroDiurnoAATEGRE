using AutoMapper;
using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Infraestructure.Models;

namespace CentroDiurnoAATEGRE.Application.Profiles
{
    public class UsuarioProfile : Profile
    {
        public UsuarioProfile()
        {
            CreateMap<EstadoUsuario, EstadoUsuarioDTO>();
            CreateMap<Rol, RolDTO>();

            CreateMap<Usuario, UsuarioDTO>()
                .ForMember(d => d.IdEstadoUsuarioNavigation,
                    o => o.MapFrom(s => s.IdEstadoUsuarioNavigation))
                .ForMember(d => d.IdRolNavigation,
                    o => o.MapFrom(s => s.IdRolNavigation));

            CreateMap<UsuarioDTO, Usuario>()
                .ForMember(d => d.IdEstadoUsuarioNavigation,
                    o => o.Ignore())
                .ForMember(d => d.IdRolNavigation,
                    o => o.Ignore());
        }
    }
}
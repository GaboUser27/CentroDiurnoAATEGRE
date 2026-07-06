using AutoMapper;
using CentroDiurnoAATEGRE.Application.DTOs;
using CentroDiurnoAATEGRE.Infraestructure.Models;
namespace CentroDiurnoAATEGRE.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ── Aviso ──────────────────────────────────────────────────
            CreateMap<Aviso, AvisoDTO>().ReverseMap();

            // ── CategoriaImagen ────────────────────────────────────────
            CreateMap<CategoriaImagen, CategoriaImagenDTO>()
                .ForMember(dest => dest.TotalImagenes,
                           opt => opt.MapFrom(src => src.Imagenes.Count));

            CreateMap<CategoriaImagenDTO, CategoriaImagen>()
                .ForMember(dest => dest.Imagenes, opt => opt.Ignore());

            // ── Imagen ─────────────────────────────────────────────────────────
            CreateMap<Imagen, ImagenDTO>()
                .ForMember(dest => dest.NombreCategoria,
                           opt => opt.MapFrom(src => src.CategoriaImagen != null
                               ? src.CategoriaImagen.Nombre : null))
                .ForMember(dest => dest.ImagenBytes,
                           opt => opt.MapFrom(src => src.ImagenBytes));

            CreateMap<ImagenDTO, Imagen>()
                .ForMember(dest => dest.CategoriaImagen, opt => opt.Ignore())
                .ForMember(dest => dest.ImagenBytes, opt => opt.Ignore());

            // ── Usuario ────────────────────────────────────────────────
            CreateMap<Usuario, UsuarioDTO>()
                .ForMember(dest => dest.NombreRol,
                           opt => opt.MapFrom(src => src.Rol != null ? src.Rol.Nombre : null))
                .ForMember(dest => dest.NombreEstado,
                           opt => opt.MapFrom(src => src.EstadoUsuario != null ? src.EstadoUsuario.Nombre : null))
                .ForMember(dest => dest.Contrasena, opt => opt.Ignore());

            CreateMap<UsuarioDTO, Usuario>()
                .ForMember(dest => dest.Rol, opt => opt.Ignore())
                .ForMember(dest => dest.EstadoUsuario, opt => opt.Ignore())
                .ForMember(dest => dest.Contrasena, opt => opt.Ignore());

            // ── InformacionInstitucional ───────────────────────────────
            CreateMap<InformacionInstitucional, InformacionInstitucionalDTO>().ReverseMap();
        }
    }
}
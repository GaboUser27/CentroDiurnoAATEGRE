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
    public class CategoriaImagenProfile : Profile
    {
        public CategoriaImagenProfile()
        {
            CreateMap<CategoriaImagenDTO, CategoriaImagen>().ReverseMap();

            // Entidad → DTO
            CreateMap<CategoriaImagen, CategoriaImagenDTO>()
                .ForMember(d => d.Imagen,
                    o => o.MapFrom(s => s.Imagen));

            // DTO → Entidad
            CreateMap<CategoriaImagenDTO, CategoriaImagen>()
                .ForMember(d => d.Imagen,
                    o => o.Ignore());

        }
    }
}

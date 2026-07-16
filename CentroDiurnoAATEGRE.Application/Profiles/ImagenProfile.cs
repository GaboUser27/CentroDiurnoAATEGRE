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
    public class ImagenProfile : Profile
    {
        public ImagenProfile()
        {

            // Entidad → DTO
            CreateMap<Imagen, ImagenDTO>()
                .ForMember(d => d.IdCategoriaImagenNavigation,
                    o => o.MapFrom(s => s.IdCategoriaImagenNavigation));

            // DTO → Entidad
            CreateMap<ImagenDTO, Imagen>()
                .ForMember(d => d.IdCategoriaImagenNavigation,
                    o => o.Ignore());

        }
    }
}

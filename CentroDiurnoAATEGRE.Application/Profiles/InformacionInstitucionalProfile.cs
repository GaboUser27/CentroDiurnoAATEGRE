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
    public class InformacionInstitucionalProfile : Profile
    {
        public InformacionInstitucionalProfile()
        {
            CreateMap<InformacionInstitucionalDTO, InformacionInstitucional>().ReverseMap();
        }
    }
}

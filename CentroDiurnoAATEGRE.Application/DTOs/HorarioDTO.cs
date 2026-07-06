using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;

namespace CentroDiurnoAATEGRE.Application.DTOs
{
    public class HorarioDTO
    {
        public int IdHorario { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public List<InformacionInstitucionalDTO> InformacionInstitucional { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;

namespace CentroDiurnoAATEGRE.Application.DTOs
{
    public class RolDTO
    {
        public int IdRol { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public virtual List<UsuarioDTO> Usuario { get; set; } = new();
    }
}

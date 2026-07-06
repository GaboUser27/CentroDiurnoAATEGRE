using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;

namespace CentroDiurnoAATEGRE.Application.DTOs
{
    public class EstadoUsuarioDTO
    {
        public int IdEstadoUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public List<UsuarioDTO> Usuario { get; set; } = new();
    }
}

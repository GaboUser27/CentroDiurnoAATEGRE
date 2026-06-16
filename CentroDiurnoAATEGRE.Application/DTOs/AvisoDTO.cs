using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentroDiurnoAATEGRE.Application.DTOs
{
    public class AvisoDTO
    {
        public int IdAviso { get; set; }

        [Required(ErrorMessage = "El título es requerido.")]
        [MaxLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contenido es requerido.")]
        [Display(Name = "Contenido")]
        public string Contenido { get; set; } = string.Empty;

        [Display(Name = "Fecha de publicación")]
        public DateTime FechaPublicacion { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de expiración")]
        public DateTime? FechaExpiracion { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }
}

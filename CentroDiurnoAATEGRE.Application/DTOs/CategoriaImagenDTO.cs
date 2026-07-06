using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;

namespace CentroDiurnoAATEGRE.Application.DTOs
{
    public class CategoriaImagenDTO
    {
        public int IdCategoriaImagen { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [MaxLength(100)]
        [Display(Name = "Nombre de la categoría")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        public List<ImagenDTO> Imagen { get; set; } = new();
    }
}

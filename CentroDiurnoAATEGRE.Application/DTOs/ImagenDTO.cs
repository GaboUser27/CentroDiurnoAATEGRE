using System.ComponentModel.DataAnnotations;

namespace CentroDiurnoAATEGRE.Application.DTOs
{
    public class ImagenDTO
    {
        public int IdImagen { get; set; }

        [Required(ErrorMessage = "El título es requerido.")]
        [MaxLength(150)]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required]
        [Display(Name = "Fecha")]
        public DateTime FechaImagen { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Seleccione una categoría.")]
        [Display(Name = "Categoría")]
        public int IdCategoriaImagen { get; set; }

        public byte[] Imagen { get; set; } = Array.Empty<byte>();

    }
}
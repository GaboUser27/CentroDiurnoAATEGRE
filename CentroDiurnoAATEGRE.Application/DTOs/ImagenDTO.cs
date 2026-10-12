using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

        public byte[]? Imagen1 { get; set; }

        public bool TieneImagen { get; set; }

        // Solo se usa para mostrar el nombre de la categoría en listados;
        // nunca se envía desde el formulario, así que no debe validarse.
        [ValidateNever]
        public virtual CategoriaImagenDTO? IdCategoriaImagenNavigation { get; set; }

        public string? ImagenUrl => TieneImagen || Imagen1 is { Length: > 0 }
            ? $"/Imagen/Ver/{IdImagen}"
            : null;
    }
}

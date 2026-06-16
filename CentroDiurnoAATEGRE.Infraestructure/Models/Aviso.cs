using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentroDiurnoAATEGRE.Infraestructure.Models;

public class Aviso
{
    [Key]
    public int IdAviso { get; set; }

    [Required, MaxLength(150)]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Contenido")]
    public string Contenido { get; set; } = string.Empty;

    [Display(Name = "Fecha de publicación")]
    public DateTime FechaPublicacion { get; set; } = DateTime.Now;

    [Display(Name = "Fecha de expiración")]
    public DateTime? FechaExpiracion { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;
}

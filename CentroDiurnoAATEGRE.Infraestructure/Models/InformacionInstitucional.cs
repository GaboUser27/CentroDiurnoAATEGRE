using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentroDiurnoAATEGRE.Infraestructure.Models;

public class InformacionInstitucional
{
    [Key]
    public int IdInformacion { get; set; }

    [Required, MaxLength(150)]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Contenido")]
    public string Contenido { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [MaxLength(100), EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string? Correo { get; set; }

    [MaxLength(300)]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [MaxLength(255)]
    public string? Facebook { get; set; }

    [MaxLength(255)]
    public string? Instagram { get; set; }
}
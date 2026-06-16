using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CentroDiurnoAATEGRE.Infraestructure.Models;

public class Imagen
{
    [Key]
    public int IdImagen { get; set; }

    [Required, MaxLength(150)]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Display(Name = "Fecha de la imagen")]
    public DateTime FechaImagen { get; set; }

    [Display(Name = "Categoría")]
    public int IdCategoriaImagen { get; set; }

    // Ruta del archivo guardado en servidor
    [MaxLength(500)]
    public string? RutaArchivo { get; set; }

    [ForeignKey(nameof(IdCategoriaImagen))]
    public CategoriaImagen? CategoriaImagen { get; set; }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentroDiurnoAATEGRE.Infraestructure.Models;

public class CategoriaImagen
{
    [Key]
    public int IdCategoriaImagen { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Descripcion { get; set; }

    public ICollection<Imagen> Imagenes { get; set; } = new List<Imagen>();
}
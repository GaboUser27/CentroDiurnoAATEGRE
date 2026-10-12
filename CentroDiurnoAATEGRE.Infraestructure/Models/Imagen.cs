using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CentroDiurnoAATEGRE.Infraestructure.Models;

public partial class Imagen
{
    public int IdImagen { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime FechaImagen { get; set; }

    public int IdCategoriaImagen { get; set; }

    public byte[]? Imagen1 { get; set; }

    [NotMapped]
    public bool TieneImagen { get; set; }

    public virtual CategoriaImagen IdCategoriaImagenNavigation { get; set; } = null!;
}

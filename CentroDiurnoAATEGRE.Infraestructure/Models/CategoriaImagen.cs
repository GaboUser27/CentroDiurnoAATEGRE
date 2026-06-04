using System;
using System.Collections.Generic;

namespace CentroDiurnoAATEGRE.Infraestructure.Models;

public partial class CategoriaImagen
{
    public int IdCategoriaImagen { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Imagen> Imagen { get; set; } = new List<Imagen>();
}

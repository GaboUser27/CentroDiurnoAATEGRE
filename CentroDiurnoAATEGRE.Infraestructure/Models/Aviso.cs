using System;
using System.Collections.Generic;

namespace CentroDiurnoAATEGRE.Infraestructure.Models;

public partial class Aviso
{
    public int IdAviso { get; set; }

    public string Titulo { get; set; } = null!;

    public string Contenido { get; set; } = null!;

    public DateTime FechaPublicacion { get; set; }

    public DateTime? FechaExpiracion { get; set; }

    public bool Activo { get; set; }
}

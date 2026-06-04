using System;
using System.Collections.Generic;

namespace CentroDiurnoAATEGRE.Infraestructure.Models;

public partial class Horario
{
    public int IdHorario { get; set; }

    public string Titulo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<InformacionInstitucional> InformacionInstitucional { get; set; } = new List<InformacionInstitucional>();
}

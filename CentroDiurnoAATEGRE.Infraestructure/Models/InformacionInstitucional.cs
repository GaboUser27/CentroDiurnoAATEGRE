using System;
using System.Collections.Generic;

namespace CentroDiurnoAATEGRE.Infraestructure.Models;

public partial class InformacionInstitucional
{
    public int IdInformacion { get; set; }

    public string Titulo { get; set; } = null!;

    public string Contenido { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public string? Direccion { get; set; }

    public string? Facebook { get; set; }

    public string? Instagram { get; set; }

    public int? IdHorario { get; set; }

    public virtual Horario? IdHorarioNavigation { get; set; }
}

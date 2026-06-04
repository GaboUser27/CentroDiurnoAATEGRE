using System;
using System.Collections.Generic;

namespace CentroDiurnoAATEGRE.Infraestructure.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public int IdRol { get; set; }

    public int IdEstadoUsuario { get; set; }

    public virtual EstadoUsuario IdEstadoUsuarioNavigation { get; set; } = null!;

    public virtual Rol IdRolNavigation { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentroDiurnoAATEGRE.Infraestructure.Models;

public class Rol
{
    [Key]
    public int IdRol { get; set; }

    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}

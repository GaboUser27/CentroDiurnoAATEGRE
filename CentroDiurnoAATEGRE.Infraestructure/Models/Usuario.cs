using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CentroDiurnoAATEGRE.Infraestructure.Models;

public class Usuario
{
    [Key]
    public int IdUsuario { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(100), EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;

    [Display(Name = "Rol")]
    public int IdRol { get; set; }

    [Display(Name = "Estado")]
    public int IdEstadoUsuario { get; set; }

    [ForeignKey(nameof(IdRol))]
    public Rol? Rol { get; set; }

    [ForeignKey(nameof(IdEstadoUsuario))]
    public EstadoUsuario? EstadoUsuario { get; set; }
}

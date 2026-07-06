using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;

namespace CentroDiurnoAATEGRE.Application.DTOs
{
    public class UsuarioDTO
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [MaxLength(100)]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [MaxLength(100)]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres.")]
        public string? Contrasena { get; set; } = string.Empty;

        public int IdRol { get; set; }

        public int IdEstadoUsuario { get; set; }

        public EstadoUsuarioDTO IdEstadoUsuarioNavigation { get; set; } = new();

        public RolDTO IdRolNavigation { get; set; } = new();
    }
}

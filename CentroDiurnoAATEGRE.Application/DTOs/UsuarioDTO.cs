using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string? Contrasena { get; set; }

        [Required(ErrorMessage = "Seleccione un rol.")]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        [Required]
        [Display(Name = "Estado")]
        public int IdEstadoUsuario { get; set; }

        public string? NombreRol { get; set; }
        public string? NombreEstado { get; set; }
    }
}

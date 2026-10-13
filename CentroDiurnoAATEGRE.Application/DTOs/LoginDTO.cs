using System.ComponentModel.DataAnnotations;

namespace CentroDiurnoAATEGRE.Application.DTOs
{
    /// <summary>
    /// Datos del formulario de inicio de sesión. Es un DTO propio (en lugar de UsuarioDTO)
    /// para que la validación del modelo solo considere Correo y Contraseña.
    /// </summary>
    public class LoginDTO
    {
        [Required, MaxLength(100)]
        public string Correo { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Contrasena { get; set; } = string.Empty;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using CentroDiurnoAATEGRE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CentroDiurnoAATEGRE.Infraestructure.Repository.Implementations
{
    public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(AategreeDbContext context) : base(context) { }

        public async Task<Usuario?> ObtenerPorCorreoAsync(string correo) =>
            await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.EstadoUsuario)
                .FirstOrDefaultAsync(u => u.Correo == correo);

        public async Task<IEnumerable<Usuario>> ObtenerConRolesYEstadosAsync() =>
            await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.EstadoUsuario)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

        public async Task CambiarEstadoAsync(int idUsuario, int idNuevoEstado)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario != null)
            {
                usuario.IdEstadoUsuario = idNuevoEstado;
                await _context.SaveChangesAsync();
            }
        }
    }
}

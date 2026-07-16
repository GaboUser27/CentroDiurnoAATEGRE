using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using CentroDiurnoAATEGRE.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CentroDiurnoAATEGRE.Infraestructure.Repository.Implementations
{
    public class UsuarioRepository : GenericRepository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(AATEGREContext context) : base(context) { }

        public async Task<Usuario?> ObtenerPorCorreoAsync(string correo) =>
            await _context.Usuario
                .Include(u => u.IdRolNavigation)
                .Include(u => u.IdEstadoUsuarioNavigation)
                .FirstOrDefaultAsync(u => u.Correo == correo);

        public async Task<IEnumerable<Usuario>> ObtenerConRolesYEstadosAsync() =>
            await _context.Usuario
                .Include(u => u.IdRolNavigation)
                .Include(u => u.IdEstadoUsuarioNavigation)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

        public async Task CambiarEstadoAsync(int idUsuario, int idNuevoEstado)
        {
            var usuario = await _context.Usuario.FindAsync(idUsuario);
            if (usuario != null)
            {
                usuario.IdEstadoUsuario = idNuevoEstado;
                await _context.SaveChangesAsync();
            }
        }
    }
}

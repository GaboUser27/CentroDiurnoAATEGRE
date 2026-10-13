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

        public async Task<Usuario?> ObtenerPorIdConRolYEstadoAsync(int id) =>
            await _context.Usuario
                .AsNoTracking()
                .Include(u => u.IdRolNavigation)
                .Include(u => u.IdEstadoUsuarioNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

        // Sin tracking: RegistrarIntentoFallidoAsync actualiza con ExecuteUpdate (directo en BD),
        // así que una entidad ya rastreada en este scope tendría el valor viejo.
        public async Task<DateTime?> ObtenerBloqueadoHastaAsync(string correo) =>
            await _context.Usuario
                .AsNoTracking()
                .Where(u => u.Correo == correo)
                .Select(u => u.BloqueadoHasta)
                .FirstOrDefaultAsync();

        public async Task<bool> ExisteCorreoAsync(string correo, int? excluirIdUsuario = null) =>
            await _context.Usuario.AnyAsync(u => u.Correo == correo
                && (excluirIdUsuario == null || u.IdUsuario != excluirIdUsuario));

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

        public async Task RegistrarIntentoFallidoAsync(int idUsuario, int limiteIntentos, DateTime bloqueadoHasta)
        {
            await _context.Usuario
                .Where(u => u.IdUsuario == idUsuario)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(u => u.IntentosFallidos, u => u.IntentosFallidos + 1)
                    .SetProperty(u => u.BloqueadoHasta,
                        u => u.IntentosFallidos + 1 >= limiteIntentos ? bloqueadoHasta : u.BloqueadoHasta));
        }

        public async Task ReiniciarIntentosFallidosAsync(int idUsuario)
        {
            await _context.Usuario
                .Where(u => u.IdUsuario == idUsuario)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(u => u.IntentosFallidos, 0)
                    .SetProperty(u => u.BloqueadoHasta, (DateTime?)null));
        }
    }
}

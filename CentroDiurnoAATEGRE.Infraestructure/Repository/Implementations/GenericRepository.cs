using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentroDiurnoAATEGRE.Infraestructure.Repository.Interfaces;
using CentroDiurnoAATEGRE.Infraestructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CentroDiurnoAATEGRE.Infraestructure.Repository.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AATEGREContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AATEGREContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> ObtenerTodosAsync() =>
            await _dbSet.ToListAsync();

        public async Task<T?> ObtenerPorIdAsync(int id) =>
            await _dbSet.FindAsync(id);

        public async Task AgregarAsync(T entidad)
        {
            await _dbSet.AddAsync(entidad);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(T entidad)
        {
            _dbSet.Update(entidad);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var entidad = await ObtenerPorIdAsync(id);
            if (entidad != null)
            {
                _dbSet.Remove(entidad);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExisteAsync(int id) =>
            await _dbSet.FindAsync(id) != null;
    }
}
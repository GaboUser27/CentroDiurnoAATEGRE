using CentroDiurnoAATEGRE.Infraestructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CentroDiurnoAATEGRE.Infrastructure.Data
{
    public class AategreeDbContext : DbContext
    {
        public AategreeDbContext(DbContextOptions<AategreeDbContext> options) : base(options) { }

        public DbSet<EstadoUsuario> EstadosUsuario { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<CategoriaImagen> CategoriasImagen { get; set; }
        public DbSet<Aviso> Avisos { get; set; }
        public DbSet<InformacionInstitucional> InformacionInstitucional { get; set; }
        public DbSet<Imagen> Imagenes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nombres de tabla exactos del script SQL
            modelBuilder.Entity<EstadoUsuario>().ToTable("EstadoUsuario");
            modelBuilder.Entity<Rol>().ToTable("Rol");
            modelBuilder.Entity<CategoriaImagen>().ToTable("CategoriaImagen");
            modelBuilder.Entity<Aviso>().ToTable("Aviso");
            modelBuilder.Entity<InformacionInstitucional>().ToTable("InformacionInstitucional");
            modelBuilder.Entity<Imagen>().ToTable("Imagen");
            modelBuilder.Entity<Usuario>().ToTable("Usuario");

            // Unicidad de correo
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            // Seed de datos iniciales
            modelBuilder.Entity<EstadoUsuario>().HasData(
                new EstadoUsuario { IdEstadoUsuario = 1, Nombre = "Activo" },
                new EstadoUsuario { IdEstadoUsuario = 2, Nombre = "Inactivo" }
            );

            modelBuilder.Entity<Rol>().HasData(
                new Rol { IdRol = 1, Nombre = "Administrador" },
                new Rol { IdRol = 2, Nombre = "Colaborador" }
            );

            modelBuilder.Entity<CategoriaImagen>().HasData(
                new CategoriaImagen { IdCategoriaImagen = 1, Nombre = "Actividades Deportivas", Descripcion = "Ejercicios, terapia física y actividades al aire libre." },
                new CategoriaImagen { IdCategoriaImagen = 2, Nombre = "Celebraciones Navideñas", Descripcion = "Momentos especiales de la temporada navideña." },
                new CategoriaImagen { IdCategoriaImagen = 3, Nombre = "Manualidades", Descripcion = "Talleres de arte, tejido y trabajo manual." },
                new CategoriaImagen { IdCategoriaImagen = 4, Nombre = "Convivencia", Descripcion = "Compartiendo momentos con amigos y familia." }
            );
        }
    }
}
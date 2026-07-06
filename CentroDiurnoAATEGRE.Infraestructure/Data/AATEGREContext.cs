using System;
using System.Collections.Generic;
using CentroDiurnoAATEGRE.Infraestructure.Models;
using Microsoft.EntityFrameworkCore;

namespace CentroDiurnoAATEGRE.Infraestructure.Data;

public partial class AATEGREContext : DbContext
{
    public AATEGREContext(DbContextOptions<AATEGREContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aviso> Aviso { get; set; }

    public virtual DbSet<CategoriaImagen> CategoriaImagen { get; set; }

    public virtual DbSet<EstadoUsuario> EstadoUsuario { get; set; }

    public virtual DbSet<Imagen> Imagen { get; set; }

    public virtual DbSet<InformacionInstitucional> InformacionInstitucional { get; set; }

    public virtual DbSet<Rol> Rol { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aviso>(entity =>
        {
            entity.HasKey(e => e.IdAviso).HasName("PK__Aviso__5CBDD9A7E776F6F1");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Contenido).IsUnicode(false);
            entity.Property(e => e.FechaExpiracion).HasColumnType("datetime");
            entity.Property(e => e.FechaPublicacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CategoriaImagen>(entity =>
        {
            entity.HasKey(e => e.IdCategoriaImagen).HasName("PK__Categori__BD46940E32ED2F33");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EstadoUsuario>(entity =>
        {
            entity.HasKey(e => e.IdEstadoUsuario).HasName("PK__EstadoUs__C8A30BCDEEB9D6E4");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Imagen>(entity =>
        {
            entity.HasKey(e => e.IdImagen).HasName("PK__Imagen__B42D8F2A9AEE0BAE");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FechaImagen).HasColumnType("datetime");
            entity.Property(e => e.Imagen1).HasColumnName("Imagen");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCategoriaImagenNavigation).WithMany(p => p.Imagen)
                .HasForeignKey(d => d.IdCategoriaImagen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Imagen_Categoria");
        });

        modelBuilder.Entity<InformacionInstitucional>(entity =>
        {
            entity.HasKey(e => e.IdInformacion).HasName("PK__Informac__6062641140A746D6");

            entity.Property(e => e.Contenido).IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Facebook)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Instagram)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__2A49584C9B369B0F");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__5B65BF9726DBEABF");

            entity.HasIndex(e => e.Correo, "UQ__Usuario__60695A19AB5EBC89").IsUnique();

            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdEstadoUsuarioNavigation).WithMany(p => p.Usuario)
                .HasForeignKey(d => d.IdEstadoUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Estado");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuario)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

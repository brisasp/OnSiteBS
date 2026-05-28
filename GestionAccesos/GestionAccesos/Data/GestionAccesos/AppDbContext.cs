using System;
using System.Collections.Generic;
using GestionAccesos.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionAccesos.Data.GestionAccesos;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcuerdosFirmado> AcuerdosFirmados { get; set; }

    public virtual DbSet<AusenciasTrabajador> AusenciasTrabajadors { get; set; }

    public virtual DbSet<ContratosTrabajadore> ContratosTrabajadores { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<FichajesTrabajador> FichajesTrabajadors { get; set; }

    public virtual DbSet<PausaFichaje> PausasFichaje { get; set; }

    public virtual DbSet<ParametrosWorker> ParametrosWorkers { get; set; }

    public virtual DbSet<PersonasAvisitar> PersonasAvisitars { get; set; }

    public virtual DbSet<TiposAusencium> TiposAusencia { get; set; }

    public virtual DbSet<Trabajadore> Trabajadores { get; set; }

    public virtual DbSet<Visita> Visitas { get; set; }

    public virtual DbSet<Visitante> Visitantes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcuerdosFirmado>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<AusenciasTrabajador>(entity =>
        {
            entity.Property(e => e.IdAusencia).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<ContratosTrabajadore>(entity =>
        {
            entity.Property(e => e.IdContrato).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.Property(e => e.IdEtt).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<FichajesTrabajador>(entity =>
        {
            entity.Property(e => e.IdFichaje).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<PausaFichaje>(entity =>
        {
            entity.Property(e => e.IdPausa).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<ParametrosWorker>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(1);
            entity.Property(e => e.IdParametro).HasColumnName("Id");
        });

        modelBuilder.Entity<PersonasAvisitar>(entity =>
        {
            entity.Property(e => e.IdPersona).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<TiposAusencium>(entity =>
        {
            entity.Property(e => e.IdTipoAusencia).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Trabajadore>(entity =>
        {
            entity.Property(e => e.IdTrabajador).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Visita>(entity =>
        {
            entity.Property(e => e.IdVisita).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Visitante>(entity =>
        {
            entity.Property(e => e.IdVisitante).ValueGeneratedOnAdd();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

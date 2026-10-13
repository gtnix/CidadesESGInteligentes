using GestaoResiduosESG.Domain.Entities;
using GestaoResiduosESG.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestaoResiduosESG.Infrastructure.Data.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public virtual DbSet<Residuo> Residuos { get; set; } = null!;
    public virtual DbSet<PontoColeta> PontosColeta { get; set; } = null!;
    public virtual DbSet<PontoResiduo> PontosResiduo { get; set; } = null!;
    public virtual DbSet<Descarte> Descartes { get; set; } = null!;
    public virtual DbSet<LimiteArmazenamento> LimitesArmazenamento { get; set; } = null!;
    public virtual DbSet<Alerta> Alertas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- RESIDUO ---
        modelBuilder.Entity<Residuo>(e =>
        {
            e.ToTable("RESIDUO");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("CD_RESIDUO").ValueGeneratedOnAdd();
            e.Property(r => r.Nome).HasColumnName("NM_RESIDUO").HasMaxLength(100).IsRequired();
            e.Property(r => r.Categoria)
                .HasColumnName("DS_CATEGORIA")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            e.Property(r => r.UnidadeMedida)
                .HasColumnName("DS_UNIDADE_MEDIDA")
                .HasMaxLength(20)
                .IsRequired();
        });

        // --- PONTO_COLETA ---
        modelBuilder.Entity<PontoColeta>(e =>
        {
            e.ToTable("PONTO_COLETA");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("CD_PONTO").ValueGeneratedOnAdd();
            e.Property(p => p.Nome).HasColumnName("NM_PONTO").HasMaxLength(100).IsRequired();
            e.Property(p => p.Localizacao).HasColumnName("DS_LOCALIZACAO").HasMaxLength(200).IsRequired();
            e.Property(p => p.CapacidadeMaxima)
                .HasColumnName("VL_CAPACIDADE_MAXIMA")
                .HasColumnType("decimal(10,2)")
                .IsRequired();
            e.Property(p => p.QuantidadeAtual)
                .HasColumnName("VL_QUANTIDADE_ATUAL")
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0m);
            e.ToTable(t => t.HasCheckConstraint("CK_capacidade_maxima", "VL_CAPACIDADE_MAXIMA > 0"));
            e.ToTable(t => t.HasCheckConstraint("CK_quantidade_atual", "VL_QUANTIDADE_ATUAL >= 0"));
        });

        // --- PONTO_RESIDUO (N:N — chave composta) ---
        modelBuilder.Entity<PontoResiduo>(e =>
        {
            e.ToTable("PONTO_RESIDUO");
            e.HasKey(pr => new { pr.PontoColetaId, pr.ResiduoId });
            e.Property(pr => pr.PontoColetaId).HasColumnName("CD_PONTO");
            e.Property(pr => pr.ResiduoId).HasColumnName("CD_RESIDUO");

            e.HasOne(pr => pr.PontoColeta)
                .WithMany(p => p.PontosResiduo)
                .HasForeignKey(pr => pr.PontoColetaId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(pr => pr.Residuo)
                .WithMany(r => r.PontosResiduo)
                .HasForeignKey(pr => pr.ResiduoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- DESCARTE ---
        modelBuilder.Entity<Descarte>(e =>
        {
            e.ToTable("DESCARTE");
            e.HasKey(d => d.Id);
            e.Property(d => d.Id).HasColumnName("CD_DESCARTE").ValueGeneratedOnAdd();
            e.Property(d => d.ResiduoId).HasColumnName("CD_RESIDUO").IsRequired();
            e.Property(d => d.PontoColetaId).HasColumnName("CD_PONTO").IsRequired();
            e.Property(d => d.DataDescarte).HasColumnName("DT_DESCARTE").IsRequired();
            e.Property(d => d.Quantidade)
                .HasColumnName("VL_QUANTIDADE")
                .HasColumnType("decimal(10,2)")
                .IsRequired();
            e.Property(d => d.Responsavel).HasColumnName("NM_RESPONSAVEL").HasMaxLength(100).IsRequired();
            e.ToTable(t => t.HasCheckConstraint("CK_descarte_quantidade", "VL_QUANTIDADE > 0"));

            e.HasOne(d => d.Residuo)
                .WithMany(r => r.Descartes)
                .HasForeignKey(d => d.ResiduoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(d => d.PontoColeta)
                .WithMany(p => p.Descartes)
                .HasForeignKey(d => d.PontoColetaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- LIMITE_ARMAZENAMENTO ---
        modelBuilder.Entity<LimiteArmazenamento>(e =>
        {
            e.ToTable("LIMITE_ARMAZENAMENTO");
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).HasColumnName("CD_LIMITE").ValueGeneratedOnAdd();
            e.Property(l => l.PontoColetaId).HasColumnName("CD_PONTO").IsRequired();
            e.Property(l => l.ResiduoId).HasColumnName("CD_RESIDUO").IsRequired();
            e.Property(l => l.LimiteMinimo)
                .HasColumnName("VL_LIMITE_MINIMO")
                .HasColumnType("decimal(10,2)")
                .IsRequired();
            e.Property(l => l.LimiteMaximo)
                .HasColumnName("VL_LIMITE_MAXIMO")
                .HasColumnType("decimal(10,2)")
                .IsRequired();
            e.Property(l => l.PercentualAlerta)
                .HasColumnName("VL_PERCENTUAL_ALERTA")
                .HasDefaultValue(80);

            e.HasIndex(l => new { l.PontoColetaId, l.ResiduoId }).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_limite_maximo", "VL_LIMITE_MAXIMO > VL_LIMITE_MINIMO"));
            e.ToTable(t => t.HasCheckConstraint("CK_percentual_alerta", "VL_PERCENTUAL_ALERTA BETWEEN 1 AND 100"));

            e.HasOne(l => l.PontoColeta)
                .WithMany(p => p.LimitesArmazenamento)
                .HasForeignKey(l => l.PontoColetaId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(l => l.Residuo)
                .WithMany(r => r.LimitesArmazenamento)
                .HasForeignKey(l => l.ResiduoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- ALERTA ---
        modelBuilder.Entity<Alerta>(e =>
        {
            e.ToTable("ALERTA");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("CD_ALERTA").ValueGeneratedOnAdd();
            e.Property(a => a.PontoColetaId).HasColumnName("CD_PONTO").IsRequired();
            e.Property(a => a.ResiduoId).HasColumnName("CD_RESIDUO").IsRequired();
            e.Property(a => a.DataAlerta)
                .HasColumnName("DT_ALERTA")
                .HasDefaultValueSql("GETDATE()");
            e.Property(a => a.Tipo)
                .HasColumnName("DS_TIPO_ALERTA")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
            e.Property(a => a.Mensagem).HasColumnName("DS_MENSAGEM").HasMaxLength(500).IsRequired();
            e.Property(a => a.Status)
                .HasColumnName("ST_ALERTA")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(StatusAlerta.Pendente);

            e.HasOne(a => a.PontoColeta)
                .WithMany(p => p.Alertas)
                .HasForeignKey(a => a.PontoColetaId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Residuo)
                .WithMany(r => r.Alertas)
                .HasForeignKey(a => a.ResiduoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

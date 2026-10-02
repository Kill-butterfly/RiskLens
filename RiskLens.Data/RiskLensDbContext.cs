using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RiskLens.Data.Entities;

namespace RiskLens.Data
{
    public class RiskLensDbContext(DbContextOptions<RiskLensDbContext> options) : DbContext(options)
    {
        public DbSet<CompanyEntity> Companies => Set<CompanyEntity>();
        public DbSet<FinancialSnapshotEntity> Snapshots => Set<FinancialSnapshotEntity>();
        public DbSet<ExtractionRecordEntity> ExtractionRecords => Set<ExtractionRecordEntity>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<CompanyEntity>()
              .HasIndex(c => c.Name).IsUnique();
            mb.Entity<FinancialSnapshotEntity>()
              .HasOne(s => s.Company).WithMany(c => c.Snapshots)
              .HasForeignKey(s => s.CompanyId).OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>供 dotnet ef 命令使用的设计时工厂（本项目用 EnsureCreated，实际上不需要迁移，保留以防扩展）。</summary>
    public class DesignTimeDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<RiskLensDbContext>
    {
        public RiskLensDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<RiskLensDbContext>()
                .UseSqlite("Data Source=risklens-design.db").Options;
            return new RiskLensDbContext(options);
        }
    }
}

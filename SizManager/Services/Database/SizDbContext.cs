using Microsoft.EntityFrameworkCore;
using SizManager.Helpers;
using SizManager.Models;

namespace SizManager.Services.Database;

public class SizDbContext : DbContext
{
    private readonly string _dbPath;

    public DbSet<Profession> Professions => Set<Profession>();
    public DbSet<ProfessionSIZ> ProfessionSIZ => Set<ProfessionSIZ>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeSIZ> EmployeeSIZ => Set<EmployeeSIZ>();

    public SizDbContext(string? dbPath = null)
    {
        _dbPath = dbPath ?? AppPaths.DatabasePath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Profession
        modelBuilder.Entity<Profession>(e =>
        {
            e.HasIndex(p => p.Number).IsUnique();
            e.HasIndex(p => p.Name);
            e.Property(p => p.Name).IsRequired();
            e.Property(p => p.Number).IsRequired();
        });

        // ProfessionSIZ
        modelBuilder.Entity<ProfessionSIZ>(e =>
        {
            e.HasIndex(s => s.ProfessionId);
            e.HasOne(s => s.Profession)
                .WithMany(p => p.SizList)
                .HasForeignKey(s => s.ProfessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Employee
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasIndex(emp => emp.LastName);
            e.HasIndex(emp => emp.PersonnelNumber);
            e.Property(emp => emp.ProfessionName).IsRequired();
            e.HasOne(emp => emp.Profession)
                .WithMany()
                .HasForeignKey(emp => emp.ProfessionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // EmployeeSIZ
        modelBuilder.Entity<EmployeeSIZ>(e =>
        {
            e.HasIndex(s => s.EmployeeId);
            e.HasOne(s => s.Employee)
                .WithMany(emp => emp.SizList)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

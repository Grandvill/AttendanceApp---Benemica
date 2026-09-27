using AttendanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceApp.Data;

// penghubung antara aplikasi .NET dengan database SQL Server

// mengelola koneksi dan interaksi aplikasi dengan database (EF Core)
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Attendance> Attendances => Set<Attendance>();

    //mengatur struktur tabel
    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EmployeeId)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.EmployeeName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.AttendanceDate)
                .HasColumnType("date");

            entity.Property(x => x.AttendanceIn)
                .HasColumnType("time");

            entity.Property(x => x.AttendanceOut)
                .HasColumnType("time");

            entity.Property(x => x.IsLeave)
                .HasDefaultValue(false);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        });
    }
}
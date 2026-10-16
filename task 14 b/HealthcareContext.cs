
using Microsoft.EntityFrameworkCore;

public class HealthcareContext : DbContext
{
    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Doctor> Doctors => Set<Doctor>();

    public DbSet<Appointment> Appointments
        => Set<Appointment>();

    protected override void OnConfiguring(
        DbContextOptionsBuilder options)
    {
        options.UseSqlServer(
            @"Server=.\SQLEXPRESS;Database=HealthcareDB;Trusted_Connection=True;TrustServerCertificate=True");
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>()
            .HasKey(a => new
            {
                a.PatientId,
                a.DoctorId,
                a.AppointmentDate
            });

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId);
    }
}

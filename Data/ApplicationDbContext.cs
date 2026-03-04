using Microsoft.EntityFrameworkCore;
using PracticeFlow.Models;

namespace PracticeFlow.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Database tables
        public DbSet<User> Users { get; set; }  
        public DbSet<Appointment> Appointments { get; set; }  
        public DbSet<MedicalRecord> MedicalRecords { get; set; }  
        public DbSet<VitalsLog> VitalsLogs { get; set; }  
        public DbSet<HealthProblem> HealthProblems { get; set; }  
        public DbSet<TreatmentGoal> TreatmentGoals { get; set; }  
        public DbSet<Prescription> Prescriptions { get; set; }  

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Appointment relationships to prevent cascade delete
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)  
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId) 
                .OnDelete(DeleteBehavior.Restrict);

            // Configure MedicalRecord relationships to prevent cascade delete
            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Patient)
                .WithMany()
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Doctor)
                .WithMany()
                .HasForeignKey(m => m.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
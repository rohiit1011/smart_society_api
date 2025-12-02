using Microsoft.EntityFrameworkCore;
using SocietyManagementAPI.Model;
using System.Net.Sockets;

namespace SocietyManagementAPI.Data
{
    public class SocietyContext : DbContext
    {
        public SocietyContext(DbContextOptions<SocietyContext> options) : base(options) { }
        public DbSet<Societies> societies { get; set; }
        public DbSet<UserModel> users { get; set; }
        public DbSet<UserRole> userRoles { get; set; }
        public DbSet<SocietyDetails> societyDetails { get; set; }
        public DbSet<SocietyMaintenanceSettings> societyMaintenanceSettings { get; set; }
        public DbSet<SocietyWings> societyWings { get; set; }
        public DbSet<SocietyDocument> societyDocuments { get; set; }
        public DbSet<DocumentType> documentTypes { get; set; }
        public DbSet<MembershipApplication> MembershipApplications { get; set; }
        public DbSet<DocumentCategories> documentCategories { get; set; }
        public DbSet<ResidentInfo> residentInfo { get; set; }
        public DbSet<ResidentFlat> residentFlats { get; set; }
        public DbSet<FamilyMembers> familyMembers { get; set; }
        public DbSet<ResidentVehicles> residentVehicles { get; set; }
        public DbSet<ResidentDocuments> residentDocuments { get; set; }
        public DbSet<Roles> roles { get; set; }

        public DbSet<MaintenanceHeads> maintenanceHeads { get; set; }

        public DbSet<MaintenanceBillRuns> maintenanceBillRuns { get; set; }

        public DbSet<MaintenanceBillLines> maintenanceBillLines { get; set; }

        public DbSet<MaintenanceBills> maintenanceBills { get; set; }
        public DbSet<MaintenanceAdjustments> maintenanceAdjustments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("master");
            modelBuilder.Entity<SocietyDetails>()
                .Property(d => d.updated_at)
                .HasColumnType("timestamp");

            modelBuilder.Entity<SocietyDetails>()
                .Property(d => d.registration_date)
                .HasColumnType("timestamp");
            base.OnModelCreating(modelBuilder);

            // Configure other composite keys and relationships here
        }
    }
}

using EMO.Models.DBModels.DBTables;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels.DBTables;
using System.Linq;
using System.Net;
using APIProduct.Models.DBModels.DBTables;

namespace EMO.Models.DBModels
{
    public class DBUserManagementContext : DbContext
    {
        public DBUserManagementContext(DbContextOptions<DBUserManagementContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            #region Has Data

            var userType = new tbl_user_type
            {
                user_type_id = Guid.NewGuid(),
                user_type_name = "System Admin",
                is_active = true,
                user_type_level = 0,
            };
            modelBuilder.Entity<tbl_user_type>().HasData(userType);
            var user = new tbl_user
            {
                user_id = Guid.NewGuid(),
                user_name = "Administrator",
                user_official_email = "admin@enexol.com",
                fk_user_type = userType.user_type_id
            };
            modelBuilder.Entity<tbl_user>().HasData(user);
            #endregion
            #region Has RelationShip
            modelBuilder.Entity<tbl_user>()
                .HasOne(p => p.user_type)
                .WithMany(b => b.users)
                .HasForeignKey(p => p.fk_user_type)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_business>()
               .HasOne(p => p.user)
               .WithMany(b => b.businesses)
               .HasForeignKey(p => p.fk_user)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_facility>()
               .HasOne(p => p.business)
               .WithMany(b => b.facilities)
               .HasForeignKey(p => p.fk_business)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_building>()
             .HasOne(p => p.facility)
             .WithMany(b => b.buildings)
             .HasForeignKey(p => p.fk_facility)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_floor>()
               .HasOne(p => p.building)
               .WithMany(b => b.floors)
               .HasForeignKey(p => p.fk_building)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_section>()
             .HasOne(p => p.floor)
             .WithMany(b => b.sections)
             .HasForeignKey(p => p.fk_floor)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_office>()
               .HasOne(p => p.section)
               .WithMany(b => b.offices)
               .HasForeignKey(p => p.fk_section)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_device>()
              .HasOne(p => p.office)
              .WithMany(b => b.devices)
              .HasForeignKey(p => p.fk_office)
              .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_device>()
              .HasOne(p => p.control_type)
              .WithMany(b => b.devices)
              .HasForeignKey(p => p.fk_control_type)
              .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_device>()
             .HasOne(p => p.device_type)
             .WithMany(b => b.devices)
             .HasForeignKey(p => p.fk_device_type)
             .OnDelete(DeleteBehavior.Restrict);
            #endregion

        }
        public DbSet<tbl_user> tbl_user { get; set; }
        public DbSet<tbl_user_type> tbl_user_type { get; set; }
        public DbSet<tbl_device> tbl_device { get; set; }
        public DbSet<tbl_device_type> tbl_device_type { get; set; }
        public DbSet<tbl_control_type> tbl_control_type { get; set; }
        public DbSet<tbl_business> tbl_business { get; set; }
        public DbSet<tbl_facility> tbl_facility { get; set; }
        public DbSet<tbl_building> tbl_building { get; set; }
        public DbSet<tbl_floor> tbl_floor { get; set; }
        public DbSet<tbl_section> tbl_section { get; set; }
        public DbSet<tbl_office> tbl_office { get; set; }
        public DbSet<tbl_poc> tbl_poc { get; set; }
        public DbSet<tbl_contact_person> tbl_contact_person { get; set; }
        public DbSet<tbl_singal_phase_data> tbl_singal_phase_data { get; set; }
        public DbSet<tbl_sub_type> tbl_sub_type { get; set; }
        public DbSet<tbl_tenant> tbl_tenant { get; set; }
        public DbSet<tbl_utility> tbl_utility {  get; set; }
        public DbSet<tbl_sensor> tbl_sensor { get; set; }




    }
}

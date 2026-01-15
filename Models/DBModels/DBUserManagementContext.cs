using EMO.Models.DBModels.DBTables;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels.DBTables;
using System.Linq;
using System.Net;

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
            var userTypeTwo = new tbl_user_type
            {
                user_type_id = Guid.NewGuid(),
                user_type_name = "Business Admin",
                is_active = true,
                user_type_level = 1,
            };

            modelBuilder.Entity<tbl_user_type>().HasData(userTypeTwo);
            var userTypeThree = new tbl_user_type
            {
                user_type_id = Guid.NewGuid(),
                user_type_name = "Tenant",
                is_active = true,
                user_type_level = 2,
            };
            modelBuilder.Entity<tbl_user_type>().HasData(userTypeThree);

            var user = new tbl_user
            {
                user_id = Guid.NewGuid(),
                name = "Administrator",
                user_name = "admin@enexol.com",
                fk_user_type = userType.user_type_id
            };
            modelBuilder.Entity<tbl_user>().HasData(user);
            #endregion

            #region Has RelationShip

            #region User
            modelBuilder.Entity<tbl_user>()
                .HasOne(p => p.user_type)
                .WithMany(b => b.users)
                .HasForeignKey(p => p.fk_user_type)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Business
            modelBuilder.Entity<tbl_business>()
               .HasOne(p => p.user)
               .WithMany(b => b.businesses)
               .HasForeignKey(p => p.fk_user)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_business>()
               .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            modelBuilder.Entity<tbl_business>()
                .Property<DateTime?>("updated_at").HasColumnType("timestamp without time zone");
            #endregion

            #region Facility

            modelBuilder.Entity<tbl_facility>()
               .HasOne(p => p.business)
               .WithMany(b => b.facilities)
               .HasForeignKey(p => p.fk_business)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_facility>()
             .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            modelBuilder.Entity<tbl_facility>()
                .Property<DateTime?>("updated_at").HasColumnType("timestamp without time zone");
            #endregion

            #region Building

            modelBuilder.Entity<tbl_building>()
             .HasOne(p => p.facility)
             .WithMany(b => b.buildings)
             .HasForeignKey(p => p.fk_facility)
             .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_building>()
             .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            modelBuilder.Entity<tbl_building>()
                .Property<DateTime?>("updated_at").HasColumnType("timestamp without time zone");
            #endregion

            #region Floor

            modelBuilder.Entity<tbl_floor>()
               .HasOne(p => p.building)
               .WithMany(b => b.floors)
               .HasForeignKey(p => p.fk_building)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_floor>()
             .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            modelBuilder.Entity<tbl_floor>()
                .Property<DateTime?>("updated_at").HasColumnType("timestamp without time zone");
            #endregion

            #region Section

            modelBuilder.Entity<tbl_section>()
             .HasOne(p => p.floor)
             .WithMany(b => b.sections)
             .HasForeignKey(p => p.fk_floor)
             .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_section>()
             .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            modelBuilder.Entity<tbl_section>()
                .Property<DateTime?>("updated_at").HasColumnType("timestamp without time zone");
            #endregion

            #region Office

            modelBuilder.Entity<tbl_office>()
               .HasOne(p => p.section)
               .WithMany(b => b.offices)
               .HasForeignKey(p => p.fk_section)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_office>()
             .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            modelBuilder.Entity<tbl_office>()
                .Property<DateTime?>("updated_at").HasColumnType("timestamp without time zone");

            #endregion

            #region Singal Phase Data
            modelBuilder.Entity<tbl_singal_phase_data>()
               .HasOne(p => p.sensor)
               .WithMany(b => b.singal_phase_data)
               .HasForeignKey(p => p.fk_sensor)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_singal_phase_data>()
            .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");

            #endregion

            #region Agreement

            modelBuilder.Entity<tbl_agreement>()
               .HasOne(p => p.office)
               .WithMany(b => b.agreements)
               .HasForeignKey(p => p.fk_office)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_agreement>()
               .HasOne(p => p.tenant)
               .WithMany(b => b.agreements)
               .HasForeignKey(p => p.fk_tenant)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_agreement>()
             .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            modelBuilder.Entity<tbl_agreement>()
                .Property<DateTime?>("updated_at").HasColumnType("timestamp without time zone");
            modelBuilder.Entity<tbl_agreement>()
           .Property<DateTime>("agreement_start_date").HasColumnType("timestamp without time zone");
            modelBuilder.Entity<tbl_agreement>()
                .Property<DateTime?>("agreement_end_date").HasColumnType("timestamp without time zone");

            #endregion

            #region Contact Person

            modelBuilder.Entity<tbl_contact_person>()
               .HasOne(p => p.tenant)
               .WithMany(b => b.contact_persons)
               .HasForeignKey(p => p.fk_tenant)
               .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Sensor

            modelBuilder.Entity<tbl_sensor>()
               .HasOne(p => p.sensor_type)
               .WithMany(b => b.sensors)
               .HasForeignKey(p => p.fk_sensor_type)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_sensor>()
             .HasOne(p => p.utility)
             .WithMany(b => b.sensors)
             .HasForeignKey(p => p.fk_utility)
             .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_sensor>()
             .HasOne(p => p.device)
             .WithMany(b => b.sensors)
             .HasForeignKey(p => p.fk_device)
             .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_sensor>()
             .HasOne(p => p.office)
             .WithMany(b => b.sensors)
             .HasForeignKey(p => p.fk_office)
             .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Tenant
            modelBuilder.Entity<tbl_tenant>()
             .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            modelBuilder.Entity<tbl_tenant>()
                .Property<DateTime?>("updated_at").HasColumnType("timestamp without time zone");

            #endregion

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
        public DbSet<tbl_contact_person> tbl_contact_person { get; set; }
        public DbSet<tbl_singal_phase_data> tbl_singal_phase_data { get; set; }
        public DbSet<tbl_tenant> tbl_tenant { get; set; }
        public DbSet<tbl_utility> tbl_utility {  get; set; }
        public DbSet<tbl_sensor> tbl_sensor { get; set; }
        public DbSet<tbl_sensor_type> tbl_sensor_type { get; set; }

    }
}

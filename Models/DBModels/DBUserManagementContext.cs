using EMO.Models.DBModels.DBTables;
using Microsoft.EntityFrameworkCore;
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
            #region User Type
            var systemAdmin = new tbl_user_type
            {
                user_type_id = Guid.NewGuid(),
                user_type_name = "System Admin",
                is_active = true,
                user_type_level = 0,
            };
            modelBuilder.Entity<tbl_user_type>().HasData(systemAdmin);

            var BusinessAdmin = new tbl_user_type
            {
                user_type_id = Guid.NewGuid(),
                user_type_name = "Business Admin",
                is_active = true,
                user_type_level = 1,
            };

            modelBuilder.Entity<tbl_user_type>().HasData(BusinessAdmin);
            var TenantuserType = new tbl_user_type
            {

                    user_type_id = Guid.NewGuid(),
                    user_type_name = "Tenant",
                    is_active = true,
                    user_type_level = 2
            };

            modelBuilder.Entity<tbl_user_type>().HasData(TenantuserType);
            var subUserTypes = new List<tbl_sub_user_type>
            {
                new tbl_sub_user_type
                {
                    sub_user_type_id = Guid.NewGuid(),
                    sub_user_type_name = "Root",
                    is_active = true,
                    sub_user_type_level = 0,
                    fk_user_type = systemAdmin.user_type_id
                },
                new tbl_sub_user_type
                {

                    sub_user_type_id = Guid.NewGuid(),
                    sub_user_type_name = "Manager",
                    is_active = true,
                    sub_user_type_level = 1,
                    fk_user_type = systemAdmin.user_type_id
                }
            };
            modelBuilder.Entity<tbl_sub_user_type>().HasData(subUserTypes);
            var businessSubUserTypes = new List<tbl_sub_user_type>
            {
                new tbl_sub_user_type
                {
                    sub_user_type_id = Guid.NewGuid(),
                    sub_user_type_name = "Root",
                    is_active = true,
                    sub_user_type_level = 0,
                    fk_user_type = BusinessAdmin.user_type_id
                },
                new tbl_sub_user_type
                {

                    sub_user_type_id = Guid.NewGuid(),
                    sub_user_type_name = "Manager",
                    is_active = true,
                    sub_user_type_level = 1,
                    fk_user_type = BusinessAdmin.user_type_id
                }
            };
            modelBuilder.Entity<tbl_sub_user_type>().HasData(businessSubUserTypes);


            var tenantSubUserTypes = new List<tbl_sub_user_type>
            {
                new tbl_sub_user_type
                {
                    sub_user_type_id = Guid.NewGuid(),
                    sub_user_type_name = "Root",
                    is_active = true,
                    sub_user_type_level = 0,
                    fk_user_type = TenantuserType.user_type_id
                }
            };
            modelBuilder.Entity<tbl_sub_user_type>().HasData(tenantSubUserTypes);

            #endregion

            #region Gender
            var genders = new List<tbl_gender>
            {
                new tbl_gender
                {
                    gender_id = Guid.NewGuid(),
                    gender_name = "Male"
                },
                new tbl_gender
                {
                    gender_id = Guid.NewGuid(),
                    gender_name = "Female"
                },
                new tbl_gender
                {
                    gender_id = Guid.NewGuid(),
                    gender_name = "Prefer not to say"
                },
                new tbl_gender
                {
                    gender_id = Guid.NewGuid(),
                    gender_name = "Other"
                }
            };

            modelBuilder.Entity<tbl_gender>().HasData(genders);
            #endregion
            #region Admin User
            var user = new tbl_user
            {
                user_id = Guid.NewGuid(),
                name = "Administrator",
                user_name = "admin@emo.com",
                fk_gender = genders.Where(x => x.gender_name == "Prefer not to say").First().gender_id,
                fk_sub_user_type = subUserTypes.Where(x=>x.sub_user_type_name == "Root").First().sub_user_type_id,
            };
            modelBuilder.Entity<tbl_user>().HasData(user);
            #endregion


            #endregion
            #region Has RelationShip

            #region User
            modelBuilder.Entity<tbl_user>()
                .HasOne(p => p.sub_user_type)
                .WithMany(b => b.users)
                .HasForeignKey(p => p.fk_sub_user_type)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_user>()
           .HasOne(p => p.handler)
           .WithMany(b => b.users)
           .HasForeignKey(p => p.fk_handler)
           .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_user>()
                .HasOne(p => p.gender)
                .WithMany(b => b.users)
                .HasForeignKey(p => p.fk_gender)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_user>()
              .HasOne(p => p.businesses)
              .WithMany(b => b.users)
              .HasForeignKey(p => p.fk_business)
              .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_user>()
           .Property<DateTime?>("last_activity_at")
           .HasColumnType("DATETIME(6)");
            #endregion

            #region Business
            //modelBuilder.Entity<tbl_business>()
            //   .HasOne(p => p.user)
            //   .WithMany(b => b.businesses)
            //   .HasForeignKey(p => p.fk_user)
            //   .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_business>()
               .Property<DateTime>("created_at")
               .HasColumnType("DATETIME(6)");

            modelBuilder.Entity<tbl_business>()
                .Property<DateTime>("updated_at")
                .HasColumnType("DATETIME(6)");
            #endregion

            #region Facility
            modelBuilder.Entity<tbl_facility>()
               .HasOne(p => p.business)
               .WithMany(b => b.facilities)
               .HasForeignKey(p => p.fk_business)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_facility>()
             .Property<DateTime>("created_at")
             .HasColumnType("DATETIME(6)");

            modelBuilder.Entity<tbl_facility>()
                .Property<DateTime>("updated_at")
                .HasColumnType("DATETIME(6)");
            #endregion

            #region Building
            modelBuilder.Entity<tbl_building>()
             .HasOne(p => p.facility)
             .WithMany(b => b.buildings)
             .HasForeignKey(p => p.fk_facility)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_building>()
             .Property<DateTime>("created_at")
             .HasColumnType("DATETIME(6)");

            modelBuilder.Entity<tbl_building>()
                .Property<DateTime>("updated_at")
                .HasColumnType("DATETIME(6)");
            modelBuilder.Entity<tbl_building>()
           .HasOne(p => p.business)
           .WithMany(b => b.buildings)
           .HasForeignKey(p => p.fk_business)
           .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Floor
            modelBuilder.Entity<tbl_floor>()
               .HasOne(p => p.building)
               .WithMany(b => b.floors)
               .HasForeignKey(p => p.fk_building)
               .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_floor>()
           .HasOne(p => p.business)
           .WithMany(b => b.floors)
           .HasForeignKey(p => p.fk_business)
           .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<tbl_floor>()
             .Property<DateTime>("created_at")
             .HasColumnType("DATETIME(6)");

            modelBuilder.Entity<tbl_floor>()
                .Property<DateTime>("updated_at")
                .HasColumnType("DATETIME(6)");
            #endregion

            #region Section
            modelBuilder.Entity<tbl_section>()
             .HasOne(p => p.floor)
             .WithMany(b => b.sections)
             .HasForeignKey(p => p.fk_floor)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_section>()
         .HasOne(p => p.business)
         .WithMany(b => b.sections)
         .HasForeignKey(p => p.fk_business)
         .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_section>()
             .Property<DateTime>("created_at")
             .HasColumnType("DATETIME(6)");

            modelBuilder.Entity<tbl_section>()
                .Property<DateTime>("updated_at")
                .HasColumnType("DATETIME(6)");
            #endregion

            #region Office
            modelBuilder.Entity<tbl_office>()
               .HasOne(p => p.section)
               .WithMany(b => b.offices)
               .HasForeignKey(p => p.fk_section)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_office>()
        .HasOne(p => p.business)
        .WithMany(b => b.offices)
        .HasForeignKey(p => p.fk_business)
        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_office>()
             .Property<DateTime>("created_at")
             .HasColumnType("DATETIME(6)");

            modelBuilder.Entity<tbl_office>()
                .Property<DateTime>("updated_at")
                .HasColumnType("DATETIME(6)");
            #endregion

            #region Singal Phase Data
            modelBuilder.Entity<tbl_singal_phase_data>()
               .HasOne(p => p.sensor)
               .WithMany(b => b.singal_phase_data)
               .HasForeignKey(p => p.fk_sensor)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_singal_phase_data>()
            .Property<DateTime>("created_at")
            .HasColumnType("DATETIME(6)");
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
             .Property<DateTime>("created_at")
             .HasColumnType("DATETIME(6)");

            modelBuilder.Entity<tbl_agreement>()
                .Property<DateTime>("updated_at")
                .HasColumnType("DATETIME(6)");

            modelBuilder.Entity<tbl_agreement>()
            .Property<DateTime>("agreement_start_date")
            .HasColumnType("DATETIME(6)");

            modelBuilder.Entity<tbl_agreement>()
                .Property<DateTime>("agreement_end_date")
                .HasColumnType("DATETIME(6)");
            #endregion

            #region Contact Person
            modelBuilder.Entity<tbl_contact_person>()
               .HasOne(p => p.tenant)
               .WithMany(b => b.contact_persons)
               .HasForeignKey(p => p.fk_tenant)
               .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region User Image
            modelBuilder.Entity<tbl_user_image>()
                .HasOne(u => u.user)
                     .WithOne(img => img.user_image)
                     .HasForeignKey<tbl_user_image>(img => img.fk_user)
                     .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region Sensor
            //modelBuilder.Entity<tbl_sensor>()
            //   .HasOne(p => p.sensor_type)
            //   .WithMany(b => b.sensors)
            //   .HasForeignKey(p => p.fk_sensor_type)
            //   .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_sensor>()
             .HasOne(p => p.office)
             .WithMany(b => b.sensors)
             .HasForeignKey(p => p.fk_office)
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
            #endregion

            #region Device
            //modelBuilder.Entity<tbl_sensor>()
            //   .HasOne(p => p.sensor_type)
            //   .WithMany(b => b.sensors)
            //   .HasForeignKey(p => p.fk_sensor_type)
            //   .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<tbl_device>()
             .HasOne(p => p.business)
             .WithMany(b => b.devices)
             .HasForeignKey(p => p.fk_business)
             .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<tbl_device>()
            // .HasOne(p => p.office)
            // .WithMany(b => b.devices)
            // .HasForeignKey(p => p.fk_office)
            // .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region Tenant
            modelBuilder.Entity<tbl_tenant>()
             .Property<DateTime>("created_at")
             .HasColumnType("DATETIME(6)");

            modelBuilder.Entity<tbl_tenant>()
                .Property<DateTime>("updated_at")
                .HasColumnType("DATETIME(6)");
            #endregion

            #region SubUser Type
            modelBuilder.Entity<tbl_sub_user_type>()
               .HasOne(p => p.user_type)
               .WithMany(b => b.subuser_types)
               .HasForeignKey(p => p.fk_user_type)
               .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #endregion

            // #region Has RelationShip

            // #region User
            // modelBuilder.Entity<tbl_user>()
            //     .HasOne(p => p.user_type)
            //     .WithMany(b => b.users)
            //     .HasForeignKey(p => p.fk_user_type)
            //     .OnDelete(DeleteBehavior.Restrict);
            // #endregion

            // #region Business
            // modelBuilder.Entity<tbl_business>()
            //    .HasOne(p => p.user)
            //    .WithMany(b => b.businesses)
            //    .HasForeignKey(p => p.fk_user)
            //    .OnDelete(DeleteBehavior.Restrict);
            // modelBuilder.Entity<tbl_business>()
            //    .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            // modelBuilder.Entity<tbl_business>()
            //     .Property<DateTime>("updated_at").HasColumnType("timestamp without time zone");
            // #endregion

            // #region Facility

            // modelBuilder.Entity<tbl_facility>()
            //    .HasOne(p => p.business)
            //    .WithMany(b => b.facilities)
            //    .HasForeignKey(p => p.fk_business)
            //    .OnDelete(DeleteBehavior.Restrict);
            // modelBuilder.Entity<tbl_facility>()
            //  .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            // modelBuilder.Entity<tbl_facility>()
            //     .Property<DateTime>("updated_at").HasColumnType("timestamp without time zone");
            // #endregion

            // #region Building

            // modelBuilder.Entity<tbl_building>()
            //  .HasOne(p => p.facility)
            //  .WithMany(b => b.buildings)
            //  .HasForeignKey(p => p.fk_facility)
            //  .OnDelete(DeleteBehavior.Restrict);
            // modelBuilder.Entity<tbl_building>()
            //  .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            // modelBuilder.Entity<tbl_building>()
            //     .Property<DateTime>("updated_at").HasColumnType("timestamp without time zone");
            // #endregion

            // #region Floor

            // modelBuilder.Entity<tbl_floor>()
            //    .HasOne(p => p.building)
            //    .WithMany(b => b.floors)
            //    .HasForeignKey(p => p.fk_building)
            //    .OnDelete(DeleteBehavior.Restrict);
            // modelBuilder.Entity<tbl_floor>()
            //  .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            // modelBuilder.Entity<tbl_floor>()
            //     .Property<DateTime>("updated_at").HasColumnType("timestamp without time zone");
            // #endregion

            // #region Section

            // modelBuilder.Entity<tbl_section>()
            //  .HasOne(p => p.floor)
            //  .WithMany(b => b.sections)
            //  .HasForeignKey(p => p.fk_floor)
            //  .OnDelete(DeleteBehavior.Restrict);
            // modelBuilder.Entity<tbl_section>()
            //  .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            // modelBuilder.Entity<tbl_section>()
            //     .Property<DateTime>("updated_at").HasColumnType("timestamp without time zone");
            // #endregion

            // #region Office

            // modelBuilder.Entity<tbl_office>()
            //    .HasOne(p => p.section)
            //    .WithMany(b => b.offices)
            //    .HasForeignKey(p => p.fk_section)
            //    .OnDelete(DeleteBehavior.Restrict);
            // modelBuilder.Entity<tbl_office>()
            //  .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            // modelBuilder.Entity<tbl_office>()
            //     .Property<DateTime>("updated_at").HasColumnType("timestamp without time zone");

            // #endregion

            // #region Singal Phase Data
            // modelBuilder.Entity<tbl_singal_phase_data>()
            //    .HasOne(p => p.sensor)
            //    .WithMany(b => b.singal_phase_data)
            //    .HasForeignKey(p => p.fk_sensor)
            //    .OnDelete(DeleteBehavior.Restrict);
            // modelBuilder.Entity<tbl_singal_phase_data>()
            // .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");

            // #endregion

            // #region Agreement

            // modelBuilder.Entity<tbl_agreement>()
            //    .HasOne(p => p.office)
            //    .WithMany(b => b.agreements)
            //    .HasForeignKey(p => p.fk_office)
            //    .OnDelete(DeleteBehavior.Restrict);
            // modelBuilder.Entity<tbl_agreement>()
            //    .HasOne(p => p.tenant)
            //    .WithMany(b => b.agreements)
            //    .HasForeignKey(p => p.fk_tenant)
            //    .OnDelete(DeleteBehavior.Restrict);
            // modelBuilder.Entity<tbl_agreement>()
            //  .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            // modelBuilder.Entity<tbl_agreement>()
            //     .Property<DateTime>("updated_at").HasColumnType("timestamp without time zone");
            // modelBuilder.Entity<tbl_agreement>()
            //.Property<DateTime>("agreement_start_date").HasColumnType("timestamp without time zone");
            // modelBuilder.Entity<tbl_agreement>()
            //     .Property<DateTime>("agreement_end_date").HasColumnType("timestamp without time zone");

            // #endregion

            // #region Contact Person

            // modelBuilder.Entity<tbl_contact_person>()
            //    .HasOne(p => p.tenant)
            //    .WithMany(b => b.contact_persons)
            //    .HasForeignKey(p => p.fk_tenant)
            //    .OnDelete(DeleteBehavior.Restrict);

            // #endregion

            // #region Sensor

            // modelBuilder.Entity<tbl_sensor>()
            //    .HasOne(p => p.sensor_type)
            //    .WithMany(b => b.sensors)
            //    .HasForeignKey(p => p.fk_sensor_type)
            //    .OnDelete(DeleteBehavior.Restrict);
            // modelBuilder.Entity<tbl_sensor>()
            //  .HasOne(p => p.utility)
            //  .WithMany(b => b.sensors)
            //  .HasForeignKey(p => p.fk_utility)
            //  .OnDelete(DeleteBehavior.Restrict);
            // modelBuilder.Entity<tbl_sensor>()
            //  .HasOne(p => p.device)
            //  .WithMany(b => b.sensors)
            //  .HasForeignKey(p => p.fk_device)
            //  .OnDelete(DeleteBehavior.Restrict);
            // modelBuilder.Entity<tbl_sensor>()
            //  .HasOne(p => p.office)
            //  .WithMany(b => b.sensors)
            //  .HasForeignKey(p => p.fk_office)
            //  .OnDelete(DeleteBehavior.Restrict);

            // #endregion

            // #region Tenant
            // modelBuilder.Entity<tbl_tenant>()
            //  .Property<DateTime>("created_at").HasColumnType("timestamp without time zone");
            // modelBuilder.Entity<tbl_tenant>()
            //     .Property<DateTime>("updated_at").HasColumnType("timestamp without time zone");

            // #endregion

            // #endregion

        }
        public DbSet<tbl_user> tbl_user { get; set; }
        public DbSet<tbl_user_type> tbl_user_type { get; set; }
        public DbSet<tbl_device> tbl_device { get; set; }
        //public DbSet<tbl_device_type> tbl_device_type { get; set; }
        //public DbSet<tbl_control_type> tbl_control_type { get; set; }
        public DbSet<tbl_business> tbl_business { get; set; }
        public DbSet<tbl_facility> tbl_facility { get; set; }
        public DbSet<tbl_agreement> tbl_agreement { get; set; }
        public DbSet<tbl_building> tbl_building { get; set; }
        public DbSet<tbl_floor> tbl_floor { get; set; }
        public DbSet<tbl_section> tbl_section { get; set; }
        public DbSet<tbl_office> tbl_office { get; set; }
        public DbSet<tbl_contact_person> tbl_contact_person { get; set; }
        public DbSet<tbl_singal_phase_data> tbl_singal_phase_data { get; set; }
        //public DbSet<tbl_tenant> tbl_tenant { get; set; }
        public DbSet<tbl_utility> tbl_utility {  get; set; }
        public DbSet<tbl_sensor> tbl_sensor { get; set; }
        //public DbSet<tbl_sensor_type> tbl_sensor_type { get; set; }
        public DbSet<tbl_gender> tbl_gender { get; set; }
        public DbSet<tbl_sub_user_type> tbl_sub_user_type { get; set; }
        public DbSet<tbl_user_image> tbl_user_image { get; set; }

    }
}

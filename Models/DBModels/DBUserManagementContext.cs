using APIProduct.Models.DBModels.DBTables;
using Microsoft.EntityFrameworkCore;
using P3AHR.Models.DBModels.DBTables;
using System.Linq;
using System.Net;

namespace P3AHR.Models.DBModels
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
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
           // modelBuilder.ApplyConfiguration(new tbl_user_type_config());
           // modelBuilder.ApplyConfiguration(new tbl_user_Config());

        }
        public DbSet<tbl_user> tbl_user { get; set; }
        public DbSet<tbl_user_type> tbl_user_type { get; set; }
       
      
    }
}

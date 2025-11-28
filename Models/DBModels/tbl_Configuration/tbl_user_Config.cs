using APIProduct.Models.DBModels.DBTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P3AHR.Models.DBModels.DBTables;

namespace APIProduct.Models.DBModels.tbl_Configuration
{
    /* public class tbl_user_Config: IEntityTypeConfiguration<tbl_user>
     {
         public void Configure(EntityTypeBuilder<tbl_user> builder)
         {
             builder.ToTable("tbl_user");

             builder.Property(n => n.user_name).IsRequired().HasMaxLength(200);

             var userType = new List<tbl_user_type>();
             var userTypeId = userType.Where(x => x.user_type_name == "System Admin").FirstOrDefault().user_type_id;
             builder.HasData(new  tbl_user()
             {
                 user_id = Guid.NewGuid(),
                 user_name = "Administrator",
                 user_official_email = "admin@enexol.com",
                 fk_user_type = userTypeId

             });
             builder.HasOne(p => p.user_type)
                 .WithMany(b => b.users)
                 .HasForeignKey(p => p.fk_user_type)
                 .OnDelete(DeleteBehavior.Cascade);
         }
     }*/
}

using APIProduct.Models.DBModels.DBTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P3AHR.Models.DBModels.DBTables;

namespace APIProduct.Models.DBModels.tbl_Configuration
{
   /* public class tbl_user_type_config : IEntityTypeConfiguration<tbl_user_type>
    {
        public void Configure(EntityTypeBuilder<tbl_user_type> builder)
        {
            builder.ToTable("tbl_user_type");
            builder.HasData(new tbl_user_type()
            {
                user_type_id = Guid.NewGuid(),
                user_type_name = "System Admin",
                is_active = true,
                user_type_level = 0,
            });
        }
    }*/
}

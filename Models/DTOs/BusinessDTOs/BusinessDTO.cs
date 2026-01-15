using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.BusinessDTOs
{
    public class AddBusinessDTO
    {
        [Required]
        public string businessName { get; set; } = string.Empty;
        [Required]
        public string fkUser { get; set; } = string.Empty;
    }
    public class UpdateBusinessDTO
    {
        [Required]
        public string businessId { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
        public string fkUser { get; set; } = string.Empty;
    }
    public class BusinessResponseDTO
    {
        public string businessId { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public string created_at { get; set; } = string.Empty;
        public string updated_at { get; set; } = string.Empty;
        public bool is_active { get; set; } = false;
        public string fkUser { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
    }
}

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
        public string fkUser { get; set; } = string.Empty;
    }
    public class BusinessResponseDTO
    {
        public string businessId { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public string fkUser { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
    }
}

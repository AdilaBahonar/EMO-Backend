namespace EMO.Models.DTOs.TenantDTOs
{
    using System.ComponentModel.DataAnnotations;

    namespace EMO.Models.DTOs.TenantDTOs
    {
        public class AddTenantDTO
        {
            [Required]
            public string tenantName { get; set; } = string.Empty;
            [Required]
            public string tenantNtn { get; set; } = string.Empty;
            [Required]
            public string tenantAddress { get; set; } = string.Empty;
            [Required]
            public string tenantCoin { get; set; } = string.Empty;
        }

        public class UpdateTenantDTO
        {
            [Required]
            public string tenantId { get; set; } = string.Empty;
            public string tenantName { get; set; } = string.Empty;
            public string tenantNtn { get; set; } = string.Empty;
            public string tenantAddress { get; set; } = string.Empty;
            public string tenantCoin { get; set; } = string.Empty;
            public bool isActive { get; set; } = false;
        }

        public class TenantResponseDTO
        {
            [Required]
            public string tenantId { get; set; } = string.Empty;
            public string tenantName { get; set; } = string.Empty;
            public string tenantNtn { get; set; } = string.Empty;
            public string tenantAddress { get; set; } = string.Empty;
            public string tenantCoin { get; set; } = string.Empty;
            public string createdAt { get; set; } = string.Empty;
            public string updatedAt { get; set; } = string.Empty;
            public bool isActive { get; set; } = false;
        }
    }

}

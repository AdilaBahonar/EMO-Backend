
    using EMO.Models.DTOs.AgreementDTOs;
    using EMO.Models.DTOs.ContactPersonDTOs;
    using EMO.Models.DTOs.UserDTOs;
using Npgsql.Replication;
using System.ComponentModel.DataAnnotations;

    namespace EMO.Models.DTOs.TenantDTOs
    {
        public class AssignTenantDTO
        {
            public string fkTenant { get; set; } = string.Empty; 
            public List<string> fkOffices { get; set; } = new List<string>();
            public AddAgreementDTO agreement {get; set; } = new AddAgreementDTO();
            public AddUserDTO tenant { get; set; } = new AddUserDTO();
            public List<AddContactPersonDTO>? contactPerson { get; set; } = new List<AddContactPersonDTO>();

        }

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

        public class tenantResponseDTO
        {
             public string tenantId { get; set; } = string.Empty;
             public string tenantName { get; set; } = string.Empty;
             public string tenantUserName { get; set; } = string.Empty;
             public string tenantEmail { get; set; } = string.Empty;
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

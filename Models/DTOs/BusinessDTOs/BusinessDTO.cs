using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.BusinessDTOs
{
    public class AddBusinessDTO
    {
        [Required]
        public string businessName { get; set; } = string.Empty;
        public string businessEmail{ get; set; } = string.Empty;
        public string businessContact { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
    }

    public class AddBusinessAndAdminDTO
    {
        [Required]
        public string businessName { get; set; } = string.Empty;
        public string businessEmail { get; set; } = string.Empty;
        public string businessContact { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        [Required]
        public string userName { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string userEmail { get; set; } = string.Empty;
        public string userPhoneNo { get; set; } = string.Empty;
        public string? fkBusiness { get; set; } = null;
        public string userPassword { get; set; } = string.Empty;
        public string fkSubUserType { get; set; } = string.Empty;
        public bool businessIsActive { get; set; } = false;
        public Guid fkGender { get; set; } = Guid.Empty;
        public string? fkHandler { get; set; } = null;
        public string? imageBase64 { get; set; } = null;
    }
    public class UpdateBusinessDTO
    {
        [Required]
        public string businessId { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        public string businessEmail { get; set; } = string.Empty;
        public string businessContact { get; set; } = string.Empty;
        public string fkUser { get; set; } = string.Empty;
    }
    public class BusinessResponseDTO
    {
        public string businessId { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;
        public string businessEmail { get; set; } = string.Empty;
        public string businessContact { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        public string fkUser { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
    }
}

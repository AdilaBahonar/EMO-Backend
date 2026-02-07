using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.ContactPersonDTOs
{
    public class AddContactPersonDTO
    {
        [Required]
        public string contactPersonName { get; set; } = string.Empty;

        [Required]
        public string contactPersonPhone { get; set; } = string.Empty;

        [Required]
        public string contactPersonEmail { get; set; } = string.Empty;

        public string fkTenant { get; set; } = string.Empty;
    }

    public class UpdateContactPersonDTO
    {
        [Required]
        public string contactPersonId { get; set; } = string.Empty;

        public string contactPersonName { get; set; } = string.Empty;
        public string contactPersonPhone { get; set; } = string.Empty;
        public string contactPersonEmail { get; set; } = string.Empty;

        public bool isActive { get; set; } = false;
        public string fkTenant { get; set; } = string.Empty;
    }

    public class ContactPersonResponseDTO
    {
        [Required]
        public string contactPersonId { get; set; } = string.Empty;

        public string contactPersonName { get; set; } = string.Empty;
        public string contactPersonPhone { get; set; } = string.Empty;
        public string contactPersonEmail { get; set; } = string.Empty;

        public string tenantName { get; set; } = string.Empty;

        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        public string fkTenant { get; set; } = string.Empty;
    }
}

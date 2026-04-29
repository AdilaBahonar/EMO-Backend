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

        public string fkAgreement { get; set; } = string.Empty;
    }

    public class UpdateContactPersonDTO
    {
        [Required]
        public string contactPersonId { get; set; } = string.Empty;

        public string contactPersonName { get; set; } = string.Empty;
        public string contactPersonPhone { get; set; } = string.Empty;
        public string contactPersonEmail { get; set; } = string.Empty;

        public bool isActive { get; set; } = false;
        public string fkAgreement { get; set; } = string.Empty;
    }

    public class ContactPersonResponseDTO
    {
        public string contactPersonId { get; set; } = string.Empty;

        public string contactPersonName { get; set; } = string.Empty;
        public string contactPersonPhone { get; set; } = string.Empty;
        public string contactPersonEmail { get; set; } = string.Empty;

        public string agreementName { get; set; } = string.Empty;

        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        public string fkAgreement { get; set; } = string.Empty;
    }
}

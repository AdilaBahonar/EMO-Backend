using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.AgreementDTOs
{

        public class AddAgreementDTO
        {
            [Required]
            public string agreementName { get; set; } = string.Empty;

            public string agreementDescription { get; set; } = string.Empty;

            [Required]
            public string agreementStartDate { get; set; } = string.Empty;

            [Required]
            public string agreementEndDate { get; set; } = string.Empty;

            [Required]
            public string fkTenant { get; set; } = string.Empty;

            [Required]
            public string fkOffice { get; set; } = string.Empty;
        }

        public class UpdateAgreementDTO
        {
            [Required]
            public string agreementId { get; set; } = string.Empty;

            public string agreementName { get; set; } = string.Empty;
            public string agreementDescription { get; set; } = string.Empty;
            public string agreementStartDate { get; set; } = string.Empty;
            public string agreementEndDate { get; set; } = string.Empty;
            public bool isActive { get; set; } = false;
            public string fkTenant { get; set; } = string.Empty;
            public string fkOffice { get; set; } = string.Empty;
        }

    public class AgreementResponseDTO
    {
        public string agreementId { get; set; } = string.Empty;
        public string agreementName { get; set; } = string.Empty;
        public string agreementDescription { get; set; } = string.Empty;
        public string agreementStartDate { get; set; } = string.Empty;
        public string agreementEndDate { get; set; } = string.Empty;

        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;
        public bool isActive { get; set; }

        // Foreign Keys
        public string fkTenant { get; set; } = string.Empty;
        public string tenantName { get; set; } = string.Empty;

        public string fkOffice { get; set; } = string.Empty;
        public string officeName { get; set; } = string.Empty;
    }


}

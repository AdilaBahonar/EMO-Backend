namespace EMO.Models.DTOs.UtilityDTOs
{
    using System.ComponentModel.DataAnnotations;

    namespace EMO.Models.DTOs.UtilityDTOs
    {
        public class AddUtilityDTO
        {
            [Required]
            public string utilityName { get; set; } = string.Empty;
            public bool isActive { get; set; } = false;
        }

        public class UpdateUtilityDTO
        {
            [Required]
            public string utilityId { get; set; } = string.Empty;
            public string utilityName { get; set; } = string.Empty;
            public bool isActive { get; set; } = false;
        }

        public class UtilityResponseDTO
        {
            [Required]
            public string utilityId { get; set; } = string.Empty;
            public string utilityName { get; set; } = string.Empty;
            public bool isActive { get; set; } = false;
        }
    }

}

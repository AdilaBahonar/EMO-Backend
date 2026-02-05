using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.GenderDTOs
{
    public class AddGenderDTO
    {
        [Required]
        public string genderName { get; set; } = string.Empty;
    }
    public class UpdateGenderDTO
    {
        [Required]
        public string genderId { get; set; } = string.Empty;  // GUID of the gender to update

        [Required]
        public string genderName { get; set; } = string.Empty;
    }
    public class GenderResponseDTO
    {
        public string genderId { get; set; } = string.Empty;
        public string genderName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

}

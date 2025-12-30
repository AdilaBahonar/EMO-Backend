using System.ComponentModel.DataAnnotations;

namespace APIProduct.Models.DTOs.PocDTOs
{
    public class AddPocDTO
    {
        public string pocName { get; set; } = string.Empty;
        public string pocEmail { get; set; } = string.Empty;
        public string pocPhoneNo { get; set; } = string.Empty;
    }

    public class UpdatePocDTO
    {
        [Required]
        public string pocId { get; set; } = string.Empty;
        public string pocName { get; set; } = string.Empty;
        public string pocEmail { get; set; } = string.Empty;
        public string pocPhoneNo { get; set; } = string.Empty;
    }

    public class PocResponseDTO
    {
        public string pocId { get; set; } = string.Empty;
        public string pocName { get; set; } = string.Empty;
        public string pocEmail { get; set; } = string.Empty;
        public string pocPhoneNo { get; set; } = string.Empty;
    }
}

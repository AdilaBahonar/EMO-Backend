using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;

namespace EMO.Models.DTOs.ControlTypeDTOs
{
    public class AddControlTypeDTO
    {
        [Required]
        public string controlTypeName { get; set; } = string.Empty;

    }

    public class UpdateControlTypeDTO
    {
        [Required]
        public string controlTypeId { get; set; } = string.Empty;
        public string controlTypeName { get; set; } = string.Empty;

    }

    public class ControlTypeResponseDTO
    {
        public string controlTypeId { get; set; } = string.Empty;
        public string controlTypeName { get; set; } = string.Empty;
    }
}

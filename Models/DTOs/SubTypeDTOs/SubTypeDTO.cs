namespace EMO.Models.DTOs.SubTypeDTOs
{
    using System.ComponentModel.DataAnnotations;

    namespace EMO.Models.DTOs.SubTypeDTOs
    {
        public class AddSubTypeDTO
        {
            [Required]
            public string subTypeName { get; set; } = string.Empty;

            [Required]
            public string subTypeLevel { get; set; } = string.Empty;

            [Required]
            public string fkUserType { get; set; } = string.Empty;
        }

        public class UpdateSubTypeDTO
        {
            [Required]
            public string subTypeId { get; set; } = string.Empty;

            public string subTypeName { get; set; } = string.Empty;

            public string subTypeLevel { get; set; } = string.Empty;

            public string fkUserType { get; set; } = string.Empty;
        }

        public class SubTypeResponseDTO
        {
            [Required]
            public string subTypeId { get; set; } = string.Empty;

            public string subTypeName { get; set; } = string.Empty;

            public string subTypeLevel { get; set; } = string.Empty;

            public string userTypeName { get; set; } = string.Empty;

            public string fkUserType { get; set; } = string.Empty;
        }
    }

}

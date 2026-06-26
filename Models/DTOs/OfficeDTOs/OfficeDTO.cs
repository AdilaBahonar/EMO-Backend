using System.ComponentModel.DataAnnotations;

namespace EMO.Models.DTOs.OfficeDTOs
{
    public class AddOfficeDTO
    {
        [Required]
        public string officeName { get; set; } = string.Empty;

        [Required]
        public string fkSection { get; set; } = string.Empty;

        [Required]
        public string fkBusiness { get; set; } = string.Empty;

        public bool isActive { get; set; } = false;
        public bool isOcuppied { get; set; } = false;

        // Format from frontend: HH:mm e.g. 09:00, 18:00
        public string openingTime { get; set; } = "09:00";
        public string closingTime { get; set; } = "18:00";
        public string workingDays { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";
        public bool is24Hours { get; set; } = false;
        public bool afterHoursAlertEnabled { get; set; } = true;
    }

    public class UpdateOfficeDTO
    {
        [Required]
        public string officeId { get; set; } = string.Empty;

        public string fkBusiness { get; set; } = string.Empty;
        public string officeName { get; set; } = string.Empty;
        public string fkSection { get; set; } = string.Empty;

        public bool isActive { get; set; } = false;
        public bool isOcuppied { get; set; } = false;

        // Format from frontend: HH:mm e.g. 09:00, 18:00
        public string openingTime { get; set; } = string.Empty;
        public string closingTime { get; set; } = string.Empty;
        public string workingDays { get; set; } = string.Empty;
        public bool is24Hours { get; set; } = false;
        public bool afterHoursAlertEnabled { get; set; } = true;
    }

    public class OfficeResponseDTO
    {
        public string officeId { get; set; } = string.Empty;

        public string officeName { get; set; } = string.Empty;
        public string fkSection { get; set; } = string.Empty;
        public string fkBusiness { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public string sectionName { get; set; } = string.Empty;

        public string openingTime { get; set; } = string.Empty;
        public string closingTime { get; set; } = string.Empty;
        public string workingDays { get; set; } = string.Empty;
        public bool is24Hours { get; set; } = false;
        public bool afterHoursAlertEnabled { get; set; } = true;

        public string createdAt { get; set; } = string.Empty;
        public string updatedAt { get; set; } = string.Empty;
        public bool isActive { get; set; } = false;
        public bool isOcuppied { get; set; } = false;
    }
}

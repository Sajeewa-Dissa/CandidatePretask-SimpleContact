using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SimpleContact.Models
{
    public class EmailData
    {
        [StringLength(128, MinimumLength = 3)]
        [Required]
        public string? Name { get; set; }

        [StringLength(256, MinimumLength = 3)]
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(2048, MinimumLength = 3)]
        [Required]
        public string? Message { get; set; }


        [DisplayName("Attachments (.jpeg and .gif)")]
        public IFormFile[]? Attachments { get; set; }
    }
}

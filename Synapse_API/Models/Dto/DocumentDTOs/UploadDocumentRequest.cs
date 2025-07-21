using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Dto.DocumentDTOs
{
    public class UploadDocumentRequest
    {
        [Required]
        public IFormFile File { get; set; }

        public int UserId { get; set; } 
        public string DocumentName { get; set; } = ""; 
    }
}

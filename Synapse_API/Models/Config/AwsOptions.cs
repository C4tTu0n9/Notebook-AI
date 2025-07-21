using System.ComponentModel.DataAnnotations;

namespace Synapse_API.Models.Config
{
    public class AwsOptions
    {
        [Required]
        public string AccessKey { get; set; }

        [Required]
        public string SecretKey { get; set; }

        [Required]
        public string Region { get; set; }

        // S3
        public string Bucket { get; set; }

        // SES
        public string SesSender { get; set; }
    }
}

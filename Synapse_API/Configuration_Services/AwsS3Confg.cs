using Amazon.S3;
using Microsoft.Extensions.Options;
using Synapse_API.Models.Config;

namespace Synapse_API.Configuration_Services
{
    public class AwsS3Confg
    {
        public static void AddAmazonS3(IServiceCollection services, IConfiguration configuration)
        {
            // Đọc config AWS section vào DI
            services.Configure<AwsOptions>(configuration.GetSection("AWS"));

            // Đăng ký IAmazonS3 client cho DI
            services.AddSingleton<IAmazonS3>(sp =>
            {
                var awsOptions = sp.GetRequiredService<IOptions<AwsOptions>>().Value;
                return new AmazonS3Client(
                    awsOptions.AccessKey,
                    awsOptions.SecretKey,
                    Amazon.RegionEndpoint.GetBySystemName(awsOptions.Region)
                );
            });
        }
    }
}

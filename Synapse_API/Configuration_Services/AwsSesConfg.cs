using Amazon.Runtime;
using Amazon.SimpleEmail;
using Microsoft.Extensions.Options;
using Synapse_API.Models.Config;

namespace Synapse_API.Configuration_Services
{
    public class AwsSesConfg
    {
        public static void AddAmazonSes(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AwsOptions>(configuration.GetSection("AWS"));

            services.AddSingleton<IAmazonSimpleEmailService>(sp =>
            {
                var awsOptions = sp.GetRequiredService<IOptions<AwsOptions>>().Value;
                return new AmazonSimpleEmailServiceClient(
                    awsOptions.AccessKey,
                    awsOptions.SecretKey,
                    Amazon.RegionEndpoint.GetBySystemName(awsOptions.Region)
                );
            });
        }
    }
}

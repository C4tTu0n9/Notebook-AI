using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Web;

namespace Synapse_API.Configuration_Services
{
    public class GeminiConfg
    {
        public static void AddGemini(IServiceCollection services, IConfiguration configuration)
        {
            services.AddGenerativeAI(configuration.GetSection("Gemini"));
        }
        public static void AddGoogleAI(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<GoogleAI>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return new GoogleAI(
                    apiKey: config["Gemini:Credentials:ApiKey"],
                    accessToken: null,
                    apiVersion: null,
                    logger: provider.GetService<ILogger<GoogleAI>>()
                );
            });
        }
    }
}

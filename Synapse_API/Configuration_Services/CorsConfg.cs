namespace Synapse_API.Configuration_Services
{
    public class CorsConfg
    {
        public static void AddCors(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("https://localhost:7777")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
        }
    }
}

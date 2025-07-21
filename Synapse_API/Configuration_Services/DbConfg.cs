using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;

namespace Synapse_API.Configuration_Services
{
    public class DbConfg
    {
        public static void AddDbContext(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SynapseDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("MyCnn")));
        }
    }
}

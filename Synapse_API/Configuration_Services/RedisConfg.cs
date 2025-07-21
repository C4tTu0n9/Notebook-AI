using StackExchange.Redis;

namespace Synapse_API.Configuration_Services
{
    public class RedisConfg
    {
        public static void AddRedis(IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetSection("Redis:ConnectionString").Value;
            if (string.IsNullOrEmpty(redisConnectionString))
            {
                throw new Exception("Redis connection string is not set");
            }
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConnectionString)
            );

        }
    }
}

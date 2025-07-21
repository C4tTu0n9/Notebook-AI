using Synapse_API.Repositories;
using Synapse_API.Services.AIServices;
using Synapse_API.Services.AmazonServices;
using Synapse_API.Services.DatabaseServices;
using Synapse_API.Services.EventServices;
using Synapse_API.Services.JwtServices;
using Synapse_API.Services;
using Synapse_API.Repositories.Profile;
using Synapse_API.Repositories.Course;
using Synapse_API.Repositories.Event;
using Synapse_API.Services.CourseServices.QuizServices;
using Synapse_API.Repositories.Course.Quiz;
using Synapse_API.Services.CourseServices;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Synapse_API.Services.DocumentServices.Implementations;
using Synapse_API.Services.DocumentServices.Interfaces;
namespace Synapse_API.Configuration_Services
{
    public class DIConfg
    {
        public static void AddDI(IServiceCollection services)
        {
            services.AddScoped<GeminiService>();
            //AWS
            services.AddScoped<MyS3Service>();
            //services.AddTransient<EmailService>();
            services.AddScoped<EmailService>();
            //Event
            services.AddScoped<EventService>();
            services.AddScoped<EventRepository>();
            services.AddScoped<EventReminderService>();
            services.AddScoped<ReminderRepository>();
            services.AddHostedService<EventReminderBackgroundService>();
            //Database
            services.AddScoped<RedisService>();
            //Jwt
            services.AddScoped<JwtService>();
            //User
            services.AddScoped<UserRepository>();
            services.AddScoped<UserService>();
            services.AddScoped<UserProfileRepository>();
            //Course
            services.AddScoped<CourseRepository>();
            services.AddScoped<TopicRepository>();

            //LearningActivity
            services.AddScoped<LearningActivityRepository>();
            services.AddScoped<LearningActivityService>();

            //Quiz
            services.AddScoped<QuizService>();
            services.AddScoped<QuizRepository>();
            services.AddScoped<QuestionService>();
            services.AddScoped<QuestionRepository>();
            services.AddScoped<OptionService>();
            services.AddScoped<OptionRepository>();
            services.AddScoped<QuizAttemptService>();
            services.AddScoped<UserQuizAttemptRepository>();
            services.AddScoped<UserAnswerRepository>();
            //Topic
            services.AddScoped<TopicRepository>();
            services.AddScoped<TopicService>();

            //Analytics
            services.AddScoped<AnalyticsRepository>();  
            services.AddScoped<AnalyticsService>();

            //PerformanceMetric
            services.AddScoped<PerformanceMetricRepository>();
            services.AddScoped<PerformanceMetricService>();

            //Course
            services.AddScoped<CourseRepository>();
            services.AddScoped<CourseService>();
            
            //ChatBot
            services.AddScoped<ChatbotService>();
            services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

            // QdrantClient 
            services.AddSingleton<QdrantClient>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var host = config["Qdrant:Host"] ?? "localhost";
                var port = config.GetValue<int>("Qdrant:Port", 6334); // Mặc định 6334 cho gRPC
                return new QdrantClient(host, port);
            });
        }
    }
}

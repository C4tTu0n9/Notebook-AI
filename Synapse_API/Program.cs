using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Mscc.GenerativeAI;
using QuestPDF.Infrastructure;
using Synapse_API.Configuration_Services;
using Synapse_API.Services.AmazonServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Thêm dịch vụ DbContext
DbConfg.AddDbContext(builder.Services, builder.Configuration);
// Thêm CORS policy
CorsConfg.AddCors(builder.Services);
//GEMINI
GeminiConfg.AddGemini(builder.Services, builder.Configuration);
//GoogleAI
GeminiConfg.AddGoogleAI(builder.Services, builder.Configuration);
//AWS S3 (luu file)
AwsS3Confg.AddAmazonS3(builder.Services, builder.Configuration);
//AWS SES (Gui email)
AwsSesConfg.AddAmazonSes(builder.Services, builder.Configuration);
//DI
DIConfg.AddDI(builder.Services);
//Redis
RedisConfg.AddRedis(builder.Services, builder.Configuration);
//Jwt
JwtConfg.AddJwt(builder.Services, builder.Configuration);
//Auto mapper
builder.Services.AddAutoMapper(typeof(Program));
//Configuration Login Google Account
builder.Services.AddAuthentication()
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Quan trọng: chỉ định scheme cho OAuth
});

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Dùng CORS policy
app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

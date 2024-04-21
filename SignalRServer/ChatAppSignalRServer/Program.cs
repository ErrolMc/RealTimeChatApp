using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ChatAppSignalRServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using ChatAppSignalRServer.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ChatApp.Shared.Keys;
using ChatApp.Shared.Constants;

namespace ChatAppSignalRServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Add SignalR services to the container
            builder.Services.AddSignalR();

            // Register the custom IUserIdProvider
            builder.Services.AddSingleton<IUserIdProvider, CustomUserIDProvider>();

            // Configure CORS to allow specific origins
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder
                    .WithOrigins(NetworkConstants.FUNCTIONS_URI)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            // This registers JWT bearer token services to the DI container and sets up authentication.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // ... other parameters like issuer, audience, signing key, etc.

                        // This line tells ASP.NET Core which claim in the JWT should be used to name the user.
                        // It's how you tell ASP.NET to use a particular claim from the JWT as the user's identity.
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = NetworkConstants.FUNCTIONS_URI,
                        ValidAudience = NetworkConstants.SIGNALR_URI,
                        NameClaimType = "userid",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Keys.SIGNALR_AUTH_ISSUER_SIGNING_KEY)),
                    };
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseHttpsRedirection();

            app.UseCors("CorsPolicy"); // Apply the CORS policy

            app.UseAuthentication();
            app.UseAuthorization();

            // Map controllers
            app.MapControllers();

            // Map SignalR hubs
            app.MapHub<NotificationHub>("/NotificationHub");

            app.Run();
        }
    }
}
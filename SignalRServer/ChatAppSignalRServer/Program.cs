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

namespace ChatAppSignalRServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var backendUri = Environment.GetEnvironmentVariable("services__backend__https__0")
                ?? builder.Configuration["ServiceUrls:BackendUri"];
            var signalRUri = Environment.GetEnvironmentVariable("services__signalr-server__https__0")
                ?? builder.Configuration["ServiceUrls:SignalRUri"];
            var webAppUri = Environment.GetEnvironmentVariable("services__browser-frontend__http__0")
                ?? builder.Configuration["ServiceUrls:WebAppUri"];

            // Add services to the container.
            builder.Services.AddControllers();

            // Add SignalR services to the container
            builder.Services.AddSignalR().AddMessagePackProtocol();

            // Register the custom IUserIdProvider
            builder.Services.AddSingleton<IUserIdProvider, CustomUserIDProvider>();

            // Configure CORS to allow specific origins
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder
                    .WithOrigins(backendUri, webAppUri)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    );
            });

            // This registers JWT bearer token services to the DI container and sets up authentication.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = backendUri,
                        ValidAudience = signalRUri,
                        NameClaimType = "userid",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Keys.SIGNALR_AUTH_ISSUER_SIGNING_KEY))
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // Check if the request is for a SignalR hub and contains a token.
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/NotificationHub"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
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

            app.UseHttpsRedirection();
            app.UseCors("CorsPolicy"); // Apply the CORS policy

            app.UseRouting();
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
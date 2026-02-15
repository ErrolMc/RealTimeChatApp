using ChatApp.Backend.Services;
using ChatApp.Backend.Repositories;

var builder = WebApplication.CreateBuilder(args);

var signalRUri = Environment.GetEnvironmentVariable("services__signalr-server__https__0")
    ?? builder.Configuration["ServiceUrls:SignalRUri"];
var webAppUri = Environment.GetEnvironmentVariable("services__browser-frontend__http__0")
    ?? builder.Configuration["ServiceUrls:WebAppUri"];

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<QueryService>();
builder.Services.AddHttpClient<NotificationService>(client =>
{
    client.BaseAddress = new Uri(signalRUri);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

builder.Services.AddScoped<LoginRepository>();
builder.Services.AddScoped<FriendsRepository>();
builder.Services.AddScoped<GroupsRepository>();
builder.Services.AddScoped<MessagesRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow both http and https origins (Azure Container Apps upgrades to HTTPS)
var corsOrigins = new List<string> { webAppUri };
if (webAppUri.StartsWith("http://"))
    corsOrigins.Add(webAppUri.Replace("http://", "https://"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .WithOrigins(corsOrigins.ToArray())
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();
app.Run();

using ChatApp.Backend.Services;
using ChatApp.Backend.Repositories;
using ChatApp.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<QueryService>();
builder.Services.AddHttpClient<NotificationService>(client =>
{
    client.BaseAddress = new Uri(NetworkConstants.SIGNALR_URI);
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();
app.Run();

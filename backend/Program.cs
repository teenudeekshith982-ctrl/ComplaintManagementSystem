using System.Text;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Azure.Identity;
using ComplaintManagementSystem.Contexts;
using ComplaintManagementSystem.Hubs;
using ComplaintManagementSystem.Interfaces;
using ComplaintManagementSystem.Mappings;
using ComplaintManagementSystem.Middlewares;
using ComplaintManagementSystem.Repositories;
using ComplaintManagementSystem.Services;
using ComplaintManagementSystem.Models.Dtos.Requests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var vaultUri = builder.Configuration["AzureKeyVault:VaultUri"];
if (!string.IsNullOrEmpty(vaultUri) && vaultUri != "https://YOUR_KEY_VAULT_NAME.vault.azure.net/")
{
    Console.WriteLine($"DEBUG: Attempting to connect to Key Vault at: {vaultUri}");
    builder.Configuration.AddAzureKeyVault(
        new Uri(vaultUri), 
        new DefaultAzureCredential());
}

Console.WriteLine($"DEBUG: ConnectionStrings:Default is: '{builder.Configuration.GetConnectionString("Default")}'");
Console.WriteLine($"DEBUG: AI:GroqApiKey is: '{builder.Configuration["AI:GroqApiKey"]}'");

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ComplaintManagementSystemContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddSignalR();



Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

#region Repositories
builder.Services.AddScoped<IUserRepository,UserRepository>();
builder.Services.AddScoped<IComplaintRepository,ComplaintRepository>();
builder.Services.AddScoped<IComplaintHistoryRepository,ComplaintHistoryRepository>();
builder.Services.AddScoped<IComplaintAttachmentRepository,ComplaintAttachmentRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ISlaRepository, SlaRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IEscalationRepository, EscalationRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
#endregion

#region Services
builder.Services.AddHttpClient();
builder.Services.Configure<AISettings>(builder.Configuration.GetSection("AI"));

var aiProvider = builder.Configuration["AI:Provider"];
if (aiProvider == "Groq")
{
    builder.Services.AddScoped<IAIService, GroqProvider>();
}

builder.Services.AddScoped<ITokenService,TokenService>();
builder.Services.AddScoped<IAuthService,AuthService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();

var storageConnectionString = builder.Configuration["AzureBlobStorage:ConnectionString"];
if (!string.IsNullOrEmpty(storageConnectionString) && storageConnectionString != "YOUR_AZURE_CONNECTION_STRING_HERE")
{
    builder.Services.AddSingleton(x => new BlobServiceClient(storageConnectionString));
}
else
{
    var serviceUri = builder.Configuration["AzureBlobStorage:ServiceUri"];
    if (!string.IsNullOrEmpty(serviceUri))
    {
        builder.Services.AddSingleton(x => new BlobServiceClient(new Uri(serviceUri), new DefaultAzureCredential()));
    }
    else
    {
        builder.Services.AddSingleton(x => new BlobServiceClient("UseDevelopmentStorage=true"));
    }
}

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEscalationService, EscalationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IComplaintAIService, ComplaintAIService>();
builder.Services.AddHostedService<AutoEscalationBackgroundService>();
#endregion

#region Mappings
builder.Services.AddAutoMapper(
    typeof(ApplicationMapperProfile));

#endregion




var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AngularPolicy");
app.UseMiddleware < GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
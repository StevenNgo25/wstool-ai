using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.HostedServices;
using WhatsAppCampaignManager.Services;
using WhatsAppCampaignManager.Services.Implements;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP Client for WHAPI
builder.Services.AddHttpClient<IWhapiService, WhapiService>();

// Services
builder.Services.AddScoped<IWhapiService, WhapiService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInstanceService, InstanceService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IUserService, UserService>();

// Background Services
builder.Services.AddHostedService<GroupSyncService>();
builder.Services.AddHostedService<UserAnalyticsService>();
builder.Services.AddHostedService<MessageSenderService>();
builder.Services.AddHostedService<MessageValidationService>();
builder.Services.AddHostedService<FileCleanupService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "WhatsApp Campaign Manager API", 
        Version = "v1",
        Description = "API for managing WhatsApp campaigns via WHAPI.cloud"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WhatsApp Campaign Manager API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Serve static files
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.EnsureCreated();
        app.Logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error initializing database");
    }
}

app.Run();

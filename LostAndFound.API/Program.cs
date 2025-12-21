using LostAndFound.Application.Interfaces;
using LostAndFound.Application.Interfaces.Cases;
using LostAndFound.Application.Interfaces.Claims;
using LostAndFound.Application.Interfaces.FoundItems;
using LostAndFound.Application.Interfaces.LostReports;
using LostAndFound.Application.Interfaces.MasterData;
using LostAndFound.Application.Interfaces.Reports;
using LostAndFound.Application.Interfaces.ReturnReceipts;
using LostAndFound.Application.Interfaces.SecurityVerification;
using LostAndFound.Application.Services;
using LostAndFound.Application.Services.Cases;
using LostAndFound.Application.Services.Claims;
using LostAndFound.Application.Services.FoundItems;
using LostAndFound.Application.Services.LostReports;
using LostAndFound.Application.Services.MasterData;
using LostAndFound.Application.Services.Reports;
using LostAndFound.Application.Services.ReturnReceipts;
using LostAndFound.Application.Services.SecurityVerification;
using LostAndFound.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lost and Found API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter your token (Swagger will automatically add 'Bearer' prefix)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LostAndFoundDb")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWT");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
var issuer = jwtSettings["ValidIssuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
var audience = jwtSettings["ValidAudience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role // Map với claim type trong JWT token
    };
});

builder.Services.AddAuthorization();

// Register Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<ICampusService, CampusService>();
builder.Services.AddScoped<IItemCategoryService, ItemCategoryService>();
builder.Services.AddScoped<IStudentLostReportService, StudentLostReportService>();
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<IStaffFoundItemService, StaffFoundItemService>();
builder.Services.AddScoped<IStudentClaimService, StudentClaimService>();
builder.Services.AddScoped<ISecurityVerificationService, SecurityVerificationService>();
builder.Services.AddScoped<IStaffReturnReceiptService, StaffReturnReceiptService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<LostAndFound.Application.Interfaces.Notifications.INotificationService, LostAndFound.Application.Services.Notifications.NotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

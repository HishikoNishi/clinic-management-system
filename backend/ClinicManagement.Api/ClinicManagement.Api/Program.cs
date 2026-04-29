using System.Text;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Repositories;
using ClinicManagement.Api.Services;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new ApiErrorResponse
        {
            Code = "validation_error",
            Message = "Validation failed.",
            Details = errors
        });
    };
});

// Load billing.json (optional override)
builder.Configuration.AddJsonFile("billing.json", optional: true, reloadOnChange: true);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ClinicManagement API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT theo dạng: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? "Server=DESKTOP-0Q90AUE;Database=ClinicManagement;Trusted_Connection=true;";

builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUserRepository, EFUserRepository>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddSingleton<IPricingProvider, PricingProvider>();
builder.Services.AddScoped<BillingService>();
builder.Services.AddSingleton<FakeInsuranceService>();
builder.Services.AddScoped<DoctorScheduleService>();
builder.Services.AddScoped<AppointmentBookingService>();
builder.Services.Configure<PayOsOptions>(configuration.GetSection("PayOs"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddHostedService<NoShowBackgroundService>();
builder.Services.AddHttpContextAccessor();

var jwtKey = configuration["Jwt:Key"]
    ?? throw new ArgumentNullException("Jwt:Key is missing in appsettings.json");

var issuer = configuration["Jwt:Issuer"];
var audience = configuration["Jwt:Audience"];

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

        ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
        ValidIssuer = issuer,

        ValidateAudience = !string.IsNullOrWhiteSpace(audience),
        ValidAudience = audience,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});
builder.Services.AddScoped<MedicalRecordService>();
builder.Services.AddScoped<PrescriptionService>();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
var app = builder.Build();
app.UseCors("AllowAll");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
    dbContext.Database.Migrate();
    // Chuẩn hóa dữ liệu cũ: set InvoiceType mặc định = Clinic nếu đang null/empty
    try
    {
        dbContext.Database.ExecuteSqlRaw("UPDATE Invoices SET InvoiceType = 'Clinic' WHERE InvoiceType IS NULL OR InvoiceType = ''");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed to normalize InvoiceType values during startup.");
    }

    // Bổ sung cột cho hồ sơ khám trên DB hiện có
    try
    {
        dbContext.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('dbo.MedicalRecords', 'DetailedSymptoms') IS NULL ALTER TABLE dbo.MedicalRecords ADD DetailedSymptoms NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.MedicalRecords', 'PastMedicalHistory') IS NULL ALTER TABLE dbo.MedicalRecords ADD PastMedicalHistory NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.MedicalRecords', 'Allergies') IS NULL ALTER TABLE dbo.MedicalRecords ADD Allergies NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.MedicalRecords', 'Occupation') IS NULL ALTER TABLE dbo.MedicalRecords ADD Occupation NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.MedicalRecords', 'Habits') IS NULL ALTER TABLE dbo.MedicalRecords ADD Habits NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.MedicalRecords', 'HeightCm') IS NULL ALTER TABLE dbo.MedicalRecords ADD HeightCm DECIMAL(5,2) NULL;
IF COL_LENGTH('dbo.MedicalRecords', 'WeightKg') IS NULL ALTER TABLE dbo.MedicalRecords ADD WeightKg DECIMAL(5,2) NULL;
IF COL_LENGTH('dbo.MedicalRecords', 'Bmi') IS NULL ALTER TABLE dbo.MedicalRecords ADD Bmi DECIMAL(5,2) NULL;
IF COL_LENGTH('dbo.MedicalRecords', 'HeartRate') IS NULL ALTER TABLE dbo.MedicalRecords ADD HeartRate INT NULL;
IF COL_LENGTH('dbo.MedicalRecords', 'BloodPressure') IS NULL ALTER TABLE dbo.MedicalRecords ADD BloodPressure NVARCHAR(20) NULL;
IF COL_LENGTH('dbo.MedicalRecords', 'Temperature') IS NULL ALTER TABLE dbo.MedicalRecords ADD Temperature DECIMAL(4,1) NULL;
IF COL_LENGTH('dbo.MedicalRecords', 'Spo2') IS NULL ALTER TABLE dbo.MedicalRecords ADD Spo2 INT NULL;
");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed to apply MedicalRecords compatibility columns.");
    }

    // Bổ sung cột cho InvoiceLines (dùng cho hóa đơn thuốc)
    try
    {
        dbContext.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('dbo.InvoiceLines', 'Dosage') IS NULL
BEGIN
    ALTER TABLE dbo.InvoiceLines ADD Dosage NVARCHAR(MAX) NOT NULL CONSTRAINT DF_InvoiceLines_Dosage DEFAULT ('');
END
ELSE
BEGIN
    UPDATE dbo.InvoiceLines SET Dosage = '' WHERE Dosage IS NULL;
END

IF COL_LENGTH('dbo.InvoiceLines', 'Duration') IS NULL
BEGIN
    ALTER TABLE dbo.InvoiceLines ADD Duration INT NOT NULL CONSTRAINT DF_InvoiceLines_Duration DEFAULT (0);
END
ELSE
BEGIN
    UPDATE dbo.InvoiceLines SET Duration = 0 WHERE Duration IS NULL;
END
");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed to apply InvoiceLines compatibility columns.");
    }

    // Soft delete flags compatibility (Add IsDeleted columns if missing)
    try
    {
        dbContext.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('dbo.Doctors', 'IsDeleted') IS NULL ALTER TABLE dbo.Doctors ADD IsDeleted bit NOT NULL CONSTRAINT DF_Doctors_IsDeleted DEFAULT(0);
IF COL_LENGTH('dbo.Staffs', 'IsDeleted') IS NULL ALTER TABLE dbo.Staffs ADD IsDeleted bit NOT NULL CONSTRAINT DF_Staffs_IsDeleted DEFAULT(0);
IF COL_LENGTH('dbo.Departments', 'IsDeleted') IS NULL ALTER TABLE dbo.Departments ADD IsDeleted bit NOT NULL CONSTRAINT DF_Departments_IsDeleted DEFAULT(0);
IF COL_LENGTH('dbo.Specialties', 'IsDeleted') IS NULL ALTER TABLE dbo.Specialties ADD IsDeleted bit NOT NULL CONSTRAINT DF_Specialties_IsDeleted DEFAULT(0);
IF COL_LENGTH('dbo.Rooms', 'IsDeleted') IS NULL ALTER TABLE dbo.Rooms ADD IsDeleted bit NOT NULL CONSTRAINT DF_Rooms_IsDeleted DEFAULT(0);
IF COL_LENGTH('dbo.Medicines', 'IsDeleted') IS NULL ALTER TABLE dbo.Medicines ADD IsDeleted bit NOT NULL CONSTRAINT DF_Medicines_IsDeleted DEFAULT(0);
IF COL_LENGTH('dbo.InsurancePlans', 'IsDeleted') IS NULL ALTER TABLE dbo.InsurancePlans ADD IsDeleted bit NOT NULL CONSTRAINT DF_InsurancePlans_IsDeleted DEFAULT(0);
");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed to apply soft delete compatibility columns.");
    }
    await SeedData.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClinicManagement API v1");
    });
}

// Dùng HTTP khi chạy ngrok dev; nếu cần HTTPS, bật lại dòng dưới
// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

using CrmWorkTrack.Application.Jobs.DTOs;
using CrmWorkTrack.Infrastructure;
using CrmWorkTrack.Infrastructure.Persistence;
using CrmWorkTrack.WebApi.Auth;
using CrmWorkTrack.WebApi.Auth.Authorization;
using CrmWorkTrack.WebApi.Auth.Authorization.Permissions;
using CrmWorkTrack.WebApi.Common.Models;
using CrmWorkTrack.WebApi.Middlewares;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// PORT SABİT
builder.WebHost.UseUrls("http://localhost:5002");

// Controllers + Validation 400 standardization
builder.Services.AddControllers(options =>
{
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
})
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Invalid request value."
                        : e.ErrorMessage)
                    .ToArray()
            );

        var payload = ApiResponse<object>.Fail(new ApiErrorDto
        {
            Code = "validation.failed",
            Message = "Validation failed.",
            TraceId = context.HttpContext.TraceIdentifier,
            Details = errors
        });

        return new BadRequestObjectResult(payload);
    };
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(CrmWorkTrack.Application.Jobs.DTOs.CreateJobRequest).Assembly);

// Middlewares
builder.Services.AddTransient<GlobalExceptionMiddleware>();
builder.Services.AddTransient<RequestLoggingMiddleware>();
builder.Services.AddTransient<StatusCodeMiddleware>();

// ===================== JWT CONFIG (FAIL-FAST) =====================
var jwtSection = builder.Configuration.GetSection("Jwt");

var jwtKeyStr = jwtSection["Key"];
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

if (string.IsNullOrWhiteSpace(jwtKeyStr))
    throw new InvalidOperationException("Jwt:Key is missing. Check appsettings.json / appsettings.Development.json or env var Jwt__Key");

if (string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException("Jwt:Issuer is missing. Check appsettings.json / appsettings.Development.json or env var Jwt__Issuer");

if (string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("Jwt:Audience is missing. Check appsettings.json / appsettings.Development.json or env var Jwt__Audience");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeyStr));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// ===================== AUTHZ =====================
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

// Auth query + token service
builder.Services.AddScoped<IUserAuthQuery, UserAuthQuery>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ===================== CORS =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
// ===================== SWAGGER =====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CRM WorkTrack API",
        Version = "v1",
        Description = "CRM & Work Tracking API built with .NET 8, Clean Architecture, JWT Authentication and Permission-based Authorization.",
        Contact = new OpenApiContact
        {
            Name = "CRM WorkTrack",
            Email = "dev@crmworktrack.local"
        }
    });

    // XML comments varsa ekle
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // Aynı isimli DTO/class çakışmalarını önler
    c.CustomSchemaIds(type => type.FullName);

    // Endpointleri controller adına göre grupla
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
            return new[] { api.GroupName };

        if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            return new[] { controllerActionDescriptor.ControllerName };

        return new[] { api.RelativePath ?? "Endpoints" };
    });

    // Swagger'da alfabetik sıralama
    c.OrderActionsBy(api => $"{api.GroupName}_{api.HttpMethod}_{api.RelativePath}");

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token değerini gir. Örnek: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
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

var app = builder.Build();

    // uygulama başlarken seed
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await CrmWorkTrack.Infrastructure.Persistence.Seed.AppDbSeeder.SeedAsync(db);
    }

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM WorkTrack API v1");
        c.DocumentTitle = "CRM WorkTrack Swagger";
        c.DisplayRequestDuration();
        c.EnablePersistAuthorization();
    });
}

app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseCors("AllowReact");

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<StatusCodeMiddleware>();

    app.MapGet("/", () => "CRM API is running...");
    app.MapControllers();

    app.Run();

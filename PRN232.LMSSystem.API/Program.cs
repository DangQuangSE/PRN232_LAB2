using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using PRN232.LMSSystem.Repositories.Data;
using PRN232.LMSSystem.Repositories.Interfaces;
using PRN232.LMSSystem.Repositories.Repositories;
using PRN232.LMSSystem.Services.Interfaces;
using PRN232.LMSSystem.Services.Services;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.API.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Reflection;
using DotNetEnv;
using Asp.Versioning;
using FluentValidation;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
Env.Load();

// Configure routing to enforce lowercase URLs and lowercase query strings
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Configure Content Negotiation (JSON, XML formatters and HTTP 406 behavior)
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.ReturnHttpNotAcceptable = true;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
})
.AddXmlSerializerFormatters();

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new QueryStringApiVersionReader("api-version")
    );
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configure JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
                ?? builder.Configuration["Jwt:Secret"] 
                ?? "YourSuperSecretKeyGoesHereOfMinimumLengthOf32BytesForHS256Algorithm";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
                ?? builder.Configuration["Jwt:Issuer"] 
                ?? "LmsIssuer";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
                  ?? builder.Configuration["Jwt:Audience"] 
                  ?? "LmsAudience";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LMSDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register DataShaper for DTO responses
builder.Services.AddScoped(typeof(IDataShaper<>), typeof(DataShaper<>));

// Register Repositories
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Register Services
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register FluentValidation Validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PRN232 LMS System RESTful API v1",
        Version = "v1",
        Description = "An ASP.NET Core Web API for learning management system using a 3-layer architecture (Version 1)."
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "PRN232 LMS System RESTful API v2",
        Version = "v2",
        Description = "An ASP.NET Core Web API for learning management system using a 3-layer architecture (Version 2)."
    });

    // Configure Swagger JWT Authentication Testing
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token vào ô bên dưới (KHÔNG cần gõ 'Bearer ', Swagger tự thêm)."
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

    options.ParameterFilter<LowercaseQueryParameterFilter>();
    options.OperationFilter<DefaultResponsesOperationFilter>();
    options.OperationFilter<ExpandOptionsOperationFilter>();

    // Include XML comments from API project
    var apiXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
    if (File.Exists(apiXmlPath))
        options.IncludeXmlComments(apiXmlPath);

    // Include XML comments from Services project (Request/Response/Query models)
    var servicesXmlPath = Path.Combine(AppContext.BaseDirectory, "PRN232.LMSSystem.Services.xml");
    if (File.Exists(servicesXmlPath))
        options.IncludeXmlComments(servicesXmlPath);
});

var app = builder.Build();

// Custom Global Exception Middleware (must be first in pipeline)
app.UseMiddleware<PRN232.LMSSystem.API.Middleware.GlobalExceptionMiddleware>();

// Custom Request Logging Middleware
app.UseMiddleware<PRN232.LMSSystem.API.Middleware.LoggingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PRN232 LMS API v1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "PRN232 LMS API v2");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

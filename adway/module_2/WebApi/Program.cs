using System.Text;
using System.Text.RegularExpressions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using WebApi.Auth;
using WebApi.Common;
using WebApi.Data;
using WebApi.Mapping;
using WebApi.Middleware;
using WebApi.Models;
using WebApi.Repositories.Implementations;
using WebApi.Repositories.Interfaces;
using WebApi.Services.Implementations;
using WebApi.Services.Interfaces;

const string CorsPolicyName = "UniversityClients";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        var response = new ReturnResult<object>
        {
            StatusCode = 400,
            IsSuccess = false,
            ErrorCode = "VALIDATION_ERROR",
            ErrorMessage = string.Join("; ", errors),
            TraceId = context.HttpContext.TraceIdentifier
        };

        return new BadRequestObjectResult(response);
    };
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Authorization", "Content-Type")
            .WithExposedHeaders("api-supported-versions", "api-deprecated-versions");
    });
});

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

var legacyRoute = new Regex(
    "^api/(courses|students|teachers|enrollments)(/|$)",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "University Course Management API",
        Version = "v1",
        Description = "Первая версия API. Контракты курсов совпадают с заданием 1."
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "University Course Management API",
        Version = "v2",
        Description = "Вторая версия API. Курсы содержат преподавателя и количество записанных студентов."
    });

    options.DocInclusionPredicate((documentName, apiDescription) =>
        apiDescription.GroupName == documentName &&
        !legacyRoute.IsMatch(apiDescription.RelativePath ?? string.Empty));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Вставьте Access Token, полученный в POST /api/auth/login"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.User.RequireUniqueEmail = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtSection);

var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.Key) ||
    Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
{
    throw new InvalidOperationException(
        "JWT signing key is not configured. Set Jwt:Key (at least 32 characters) " +
        "with 'dotnet user-secrets set \"Jwt:Key\" \"...\"' or the Jwt__Key environment variable.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = AppClaimTypes.UserName,
            RoleClaimType = AppClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                var (errorCode, message) = context.AuthenticateFailure switch
                {
                    SecurityTokenExpiredException => ("TOKEN_EXPIRED", "Access token has expired."),
                    not null => ("INVALID_TOKEN", "Access token is invalid."),
                    _ => ("UNAUTHORIZED", "Authentication is required.")
                };

                context.Response.Headers.WWWAuthenticate = "Bearer";

                await ErrorResponseWriter.WriteAsync(
                    context.HttpContext,
                    StatusCodes.Status401Unauthorized,
                    errorCode,
                    message);
            },
            OnForbidden = context => ErrorResponseWriter.WriteAsync(
                context.HttpContext,
                StatusCodes.Status403Forbidden,
                "FORBIDDEN",
                "You do not have permission to perform this action.")
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AdminOnly, policy =>
        policy.RequireAuthenticatedUser().RequireRole(Roles.Admin));

    options.AddPolicy(Policies.AdminOrTeacher, policy =>
        policy.RequireAuthenticatedUser().RequireRole(Roles.Admin, Roles.Teacher));

    options.AddPolicy(Policies.StudentDataAccess, policy =>
        policy.AddRequirements(new StudentDataRequirement()));

    options.AddPolicy(Policies.EnrollmentReadAccess, policy =>
        policy.AddRequirements(new EnrollmentReadRequirement()));

    options.AddPolicy(Policies.CourseTeacherAccess, policy =>
        policy.AddRequirements(new CourseTeacherRequirement()));
});

builder.Services.AddSingleton<IAuthorizationHandler, StudentDataHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, EnrollmentReadHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CourseTeacherHandler>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

builder.Services.AddScoped<
    IStudentRepository,
    StudentRepository>();

builder.Services.AddScoped<
    ITeacherRepository,
    TeacherRepository>();

builder.Services.AddScoped<
    ICourseRepository,
    CourseRepository>();

builder.Services.AddScoped<
    IEnrollmentRepository,
    EnrollmentRepository>();

builder.Services.AddScoped<
    IStudentService,
    StudentService>();

builder.Services.AddScoped<
    ITeacherService,
    TeacherService>();

builder.Services.AddScoped<
    ICourseService,
    CourseService>();

builder.Services.AddScoped<
    IEnrollmentService,
    EnrollmentService>();

builder.Services.AddScoped<
    ITokenService,
    TokenService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IUserAdminService,
    UserAdminService>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
    });
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(CorsPolicyName);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await DbSeeder.SeedAsync(app.Services);

app.Run();

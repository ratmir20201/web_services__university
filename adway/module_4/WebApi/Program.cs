using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Common;
using WebApi.Data;
using WebApi.Mapping;
using WebApi.Middleware;
using WebApi.Repositories.Implementations;
using WebApi.Repositories.Interfaces;
using WebApi.Services.Implementations;
using WebApi.Services.Interfaces;

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

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

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
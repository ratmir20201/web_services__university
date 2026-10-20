using AutoMapper;
using WebApi.Common.Exceptions;
using WebApi.DTO.Courses;
using WebApi.Models;
using WebApi.Repositories.Interfaces;
using WebApi.Services.Interfaces;

namespace WebApi.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IMapper _mapper;

    public CourseService(
        ICourseRepository courseRepository,
        IMapper mapper)
    {
        _courseRepository = courseRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CourseDto>> GetAllAsync(
        string? search = null)
    {
        var courses =
            await _courseRepository.GetAllAsync(search);

        return _mapper.Map<IEnumerable<CourseDto>>(courses);
    }

    public async Task<CourseDto?> GetByIdAsync(int id)
    {
        var course =
            await _courseRepository.GetByIdAsync(id);

        if (course == null)
        {
            throw new ApiException(
                404,
                "COURSE_NOT_FOUND",
                "Course not found.");
        }

        return _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto> CreateAsync(
        CourseCreateDto dto)
    {
        var teacherExists =
            await _courseRepository.TeacherExistsAsync(
                dto.TeacherId);

        if (!teacherExists)
        {
            throw new ApiException(
                404,
                "TEACHER_NOT_FOUND",
                "Teacher not found.");
        }

        var course = _mapper.Map<Course>(dto);

        course.CreatedAt = DateTime.UtcNow;

        await _courseRepository.AddAsync(course);

        return _mapper.Map<CourseDto>(course);
    }

    public async Task<bool> UpdateAsync(
        int id,
        CourseUpdateDto dto)
    {
        var course =
            await _courseRepository.GetByIdAsync(id);

        if (course == null)
        {
            throw new ApiException(
                404,
                "COURSE_NOT_FOUND",
                "Course not found.");
        }

        var teacherExists =
            await _courseRepository.TeacherExistsAsync(
                dto.TeacherId);

        if (!teacherExists)
        {
            throw new ApiException(
                404,
                "TEACHER_NOT_FOUND",
                "Teacher not found.");
        }

        _mapper.Map(dto, course);

        await _courseRepository.UpdateAsync(course);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course =
            await _courseRepository.GetByIdAsync(id);

        if (course == null)
        {
            throw new ApiException(
                404,
                "COURSE_NOT_FOUND",
                "Course not found.");
        }

        await _courseRepository.DeleteAsync(course);

        return true;
    }
}
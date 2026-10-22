using AutoMapper;
using WebApi.Common.Exceptions;
using WebApi.DTO.Courses;
using WebApi.DTO.Courses.V2;
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

    public async Task<IEnumerable<CourseV2Dto>> GetAllV2Async(
        string? search = null)
    {
        var courses =
            await _courseRepository.GetAllWithDetailsAsync(search);

        return _mapper.Map<IEnumerable<CourseV2Dto>>(courses);
    }

    public async Task<CourseDto?> GetByIdAsync(int id)
    {
        var course = EnsureFound(
            await _courseRepository.GetByIdAsync(id));

        return _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseV2Dto> GetByIdV2Async(int id)
    {
        var course = EnsureFound(
            await _courseRepository.GetByIdWithDetailsAsync(id));

        return _mapper.Map<CourseV2Dto>(course);
    }

    public async Task<CourseDto> CreateAsync(
        CourseCreateDto dto)
    {
        await EnsureTeacherExistsAsync(dto.TeacherId);

        var course = _mapper.Map<Course>(dto);

        course.CreatedAt = DateTime.UtcNow;

        await _courseRepository.AddAsync(course);

        return _mapper.Map<CourseDto>(course);
    }

    public async Task<bool> UpdateAsync(
        int id,
        CourseUpdateDto dto)
    {
        var course = EnsureFound(
            await _courseRepository.GetByIdAsync(id));

        await EnsureTeacherExistsAsync(dto.TeacherId);

        _mapper.Map(dto, course);

        await _courseRepository.UpdateAsync(course);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course = EnsureFound(
            await _courseRepository.GetByIdAsync(id));

        await _courseRepository.DeleteAsync(course);

        return true;
    }

    private async Task EnsureTeacherExistsAsync(int teacherId)
    {
        var teacherExists =
            await _courseRepository.TeacherExistsAsync(teacherId);

        if (!teacherExists)
        {
            throw new ApiException(
                404,
                "TEACHER_NOT_FOUND",
                "Teacher not found.");
        }
    }

    private static Course EnsureFound(Course? course)
    {
        if (course == null)
        {
            throw new ApiException(
                404,
                "COURSE_NOT_FOUND",
                "Course not found.");
        }

        return course;
    }
}

using AutoMapper;
using WebApi.Common.Exceptions;
using WebApi.DTO.Students;
using WebApi.Models;
using WebApi.Repositories.Interfaces;
using WebApi.Services.Interfaces;

namespace WebApi.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IMapper _mapper;

    public StudentService(
        IStudentRepository studentRepository,
        IMapper mapper)
    {
        _studentRepository = studentRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StudentDto>> GetAllAsync()
    {
        var students =
            await _studentRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<StudentDto>>(
            students);
    }

    public async Task<StudentDto?> GetByIdAsync(int id)
    {
        var student =
            await _studentRepository.GetByIdAsync(id);

        if (student == null)
        {
            throw new ApiException(
                404,
                "STUDENT_NOT_FOUND",
                "Student not found.");
        }

        return _mapper.Map<StudentDto>(student);
    }

    public async Task<IEnumerable<StudentCourseDto>> GetCoursesAsync(
        int id)
    {
        var student =
            await _studentRepository.GetByIdWithCoursesAsync(id);

        if (student == null)
        {
            throw new ApiException(
                404,
                "STUDENT_NOT_FOUND",
                "Student not found.");
        }

        return student.Enrollments
            .Select(x => new StudentCourseDto
            {
                CourseId = x.CourseId,
                CourseName = x.Course!.Name,
                Credits = x.Course.Credits,
                EnrollmentDate = x.EnrollmentDate,
                Grade = x.Grade
            })
            .ToList();
    }

    public async Task<StudentDto> CreateAsync(
        StudentCreateDto dto)
    {
        var emailExists =
            await _studentRepository.EmailExistsAsync(
                dto.Email);

        if (emailExists)
        {
            throw new ApiException(
                409,
                "STUDENT_EMAIL_EXISTS",
                "Student with this email already exists.");
        }

        var student =
            _mapper.Map<Student>(dto);

        student.CreatedAt = DateTime.UtcNow;

        await _studentRepository.AddAsync(student);

        return _mapper.Map<StudentDto>(student);
    }

    public async Task<bool> UpdateAsync(
        int id,
        StudentUpdateDto dto)
    {
        var student =
            await _studentRepository.GetByIdAsync(id);

        if (student == null)
        {
            throw new ApiException(
                404,
                "STUDENT_NOT_FOUND",
                "Student not found.");
        }

        var emailExists =
            await _studentRepository.EmailExistsAsync(
                dto.Email,
                id);

        if (emailExists)
        {
            throw new ApiException(
                409,
                "STUDENT_EMAIL_EXISTS",
                "Student with this email already exists.");
        }

        _mapper.Map(dto, student);

        await _studentRepository.UpdateAsync(student);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student =
            await _studentRepository.GetByIdAsync(id);

        if (student == null)
        {
            throw new ApiException(
                404,
                "STUDENT_NOT_FOUND",
                "Student not found.");
        }

        await _studentRepository.DeleteAsync(student);

        return true;
    }
}
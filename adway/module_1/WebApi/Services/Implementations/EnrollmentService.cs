using AutoMapper;
using WebApi.Common.Exceptions;
using WebApi.DTO.Enrollments;
using WebApi.Models;
using WebApi.Repositories.Interfaces;
using WebApi.Services.Interfaces;

namespace WebApi.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IMapper mapper)
    {
        _enrollmentRepository = enrollmentRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EnrollmentDto>> GetAllAsync()
    {
        var enrollments =
            await _enrollmentRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<EnrollmentDto>>(
            enrollments);
    }

    public async Task<EnrollmentDto?> GetByIdAsync(int id)
    {
        var enrollment =
            await _enrollmentRepository.GetByIdAsync(id);

        if (enrollment == null)
        {
            throw new ApiException(
                404,
                "ENROLLMENT_NOT_FOUND",
                "Enrollment not found.");
        }

        return _mapper.Map<EnrollmentDto>(enrollment);
    }

    public async Task<EnrollmentDto> CreateAsync(
        EnrollmentCreateDto dto)
    {
        var studentExists =
            await _enrollmentRepository.StudentExistsAsync(
                dto.StudentId);

        if (!studentExists)
        {
            throw new ApiException(
                404,
                "STUDENT_NOT_FOUND",
                "Student not found.");
        }

        var courseExists =
            await _enrollmentRepository.CourseExistsAsync(
                dto.CourseId);

        if (!courseExists)
        {
            throw new ApiException(
                404,
                "COURSE_NOT_FOUND",
                "Course not found.");
        }

        var enrollmentExists =
            await _enrollmentRepository.ExistsAsync(
                dto.StudentId,
                dto.CourseId);

        if (enrollmentExists)
        {
            throw new ApiException(
                409,
                "ENROLLMENT_ALREADY_EXISTS",
                "Student is already enrolled in this course.");
        }

        var enrollment =
            _mapper.Map<Enrollment>(dto);

        enrollment.EnrollmentDate =
            DateTime.UtcNow;

        await _enrollmentRepository.AddAsync(enrollment);

        return _mapper.Map<EnrollmentDto>(
            enrollment);
    }

    public async Task<bool> UpdateAsync(
        int id,
        EnrollmentUpdateDto dto)
    {
        var enrollment =
            await _enrollmentRepository.GetByIdAsync(id);

        if (enrollment == null)
        {
            throw new ApiException(
                404,
                "ENROLLMENT_NOT_FOUND",
                "Enrollment not found.");
        }

        _mapper.Map(dto, enrollment);

        await _enrollmentRepository.UpdateAsync(
            enrollment);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var enrollment =
            await _enrollmentRepository.GetByIdAsync(id);

        if (enrollment == null)
        {
            throw new ApiException(
                404,
                "ENROLLMENT_NOT_FOUND",
                "Enrollment not found.");
        }

        await _enrollmentRepository.DeleteAsync(
            enrollment);

        return true;
    }
}
using AutoMapper;
using WebApi.Common.Exceptions;
using WebApi.DTO.Teachers;
using WebApi.Models;
using WebApi.Repositories.Interfaces;
using WebApi.Services.Interfaces;

namespace WebApi.Services.Implementations;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly IMapper _mapper;

    public TeacherService(
        ITeacherRepository teacherRepository,
        IMapper mapper)
    {
        _teacherRepository = teacherRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TeacherDto>> GetAllAsync()
    {
        var teachers = await _teacherRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<TeacherDto>>(teachers);
    }

    public async Task<TeacherDto?> GetByIdAsync(int id)
    {
        var teacher = await _teacherRepository.GetByIdAsync(id);

        if (teacher == null)
        {
            throw new ApiException(
                404,
                "TEACHER_NOT_FOUND",
                "Teacher not found.");
        }

        return _mapper.Map<TeacherDto>(teacher);
    }

    public async Task<TeacherDto> CreateAsync(TeacherCreateDto dto)
    {
        var emailExists =
            await _teacherRepository.EmailExistsAsync(dto.Email);

        if (emailExists)
        {
            throw new ApiException(
                409,
                "TEACHER_EMAIL_EXISTS",
                "Teacher with this email already exists.");
        }

        var teacher = _mapper.Map<Teacher>(dto);

        await _teacherRepository.AddAsync(teacher);

        return _mapper.Map<TeacherDto>(teacher);
    }

    public async Task<bool> UpdateAsync(
        int id,
        TeacherUpdateDto dto)
    {
        var teacher = await _teacherRepository.GetByIdAsync(id);

        if (teacher == null)
        {
            throw new ApiException(
                404,
                "TEACHER_NOT_FOUND",
                "Teacher not found.");
        }

        var emailExists =
            await _teacherRepository.EmailExistsAsync(
                dto.Email,
                id);

        if (emailExists)
        {
            throw new ApiException(
                409,
                "TEACHER_EMAIL_EXISTS",
                "Teacher with this email already exists.");
        }

        _mapper.Map(dto, teacher);

        await _teacherRepository.UpdateAsync(teacher);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var teacher = await _teacherRepository.GetByIdAsync(id);

        if (teacher == null)
        {
            throw new ApiException(
                404,
                "TEACHER_NOT_FOUND",
                "Teacher not found.");
        }

        await _teacherRepository.DeleteAsync(teacher);

        return true;
    }
}
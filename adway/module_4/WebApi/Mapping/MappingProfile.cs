using AutoMapper;
using WebApi.DTO.Courses;
using WebApi.DTO.Enrollments;
using WebApi.DTO.Students;
using WebApi.DTO.Teachers;
using WebApi.Models;

namespace WebApi.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Student, StudentDto>();
        CreateMap<StudentCreateDto, Student>();
        CreateMap<StudentUpdateDto, Student>();

        CreateMap<Teacher, TeacherDto>();
        CreateMap<TeacherCreateDto, Teacher>();
        CreateMap<TeacherUpdateDto, Teacher>();

        CreateMap<Course, CourseDto>();
        CreateMap<CourseCreateDto, Course>();
        CreateMap<CourseUpdateDto, Course>();

        CreateMap<Enrollment, EnrollmentDto>();
        CreateMap<EnrollmentCreateDto, Enrollment>();
        CreateMap<EnrollmentUpdateDto, Enrollment>();
    }
}
using AutoMapper;
using WebApi.DTO.Courses;
using WebApi.DTO.Courses.V2;
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

        CreateMap<Teacher, CourseTeacherV2Dto>()
            .ForMember(
                d => d.FullName,
                o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        CreateMap<Course, CourseV2Dto>()
            .ForMember(
                d => d.EnrolledStudentsCount,
                o => o.MapFrom(s => s.Enrollments.Count));

        CreateMap<Enrollment, EnrollmentDto>();
        CreateMap<EnrollmentCreateDto, Enrollment>();
        CreateMap<EnrollmentUpdateDto, Enrollment>();
    }
}

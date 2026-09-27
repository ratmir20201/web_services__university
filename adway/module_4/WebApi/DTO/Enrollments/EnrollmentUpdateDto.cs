using System.ComponentModel.DataAnnotations;

namespace WebApi.DTO.Enrollments;

public class EnrollmentUpdateDto
{
    [Range(0, 100)]
    public decimal? Grade { get; set; }
}
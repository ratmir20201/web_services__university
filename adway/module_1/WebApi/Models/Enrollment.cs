using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public class Enrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public Student? Student { get; set; }

    public int CourseId { get; set; }

    public Course? Course { get; set; }

    public DateTime EnrollmentDate { get; set; }

    [Range(0, 100)]
    public decimal? Grade { get; set; }
}
using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;

namespace PRN232.LMSSystem.Services.Interfaces;

public interface IEnrollmentService
{
    Task<(IEnumerable<EnrollmentResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams);
    Task<(IEnumerable<EnrollmentOfCourseResponse> Data, PaginationMetadata Pagination)> GetByCourseIdAsync(int courseId, QueryParameters queryParams);
    Task<(IEnumerable<EnrollmentOfStudentResponse> Data, PaginationMetadata Pagination)> GetByStudentIdAsync(int studentId, QueryParameters queryParams);
    Task<EnrollmentResponse> GetByIdAsync(int id, string? expand = null);
    Task<EnrollmentResponse> CreateAsync(EnrollmentRequest request);
    Task UpdateAsync(int id, EnrollmentRequest request);
    Task DeleteAsync(int id);
}

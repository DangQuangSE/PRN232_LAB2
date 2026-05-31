using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;

namespace PRN232.LMSSystem.Services.Interfaces;

public interface IStudentService
{
    Task<(IEnumerable<StudentResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams);
    Task<(IEnumerable<StudentResponse> Data, PaginationMetadata Pagination)> GetStudentsByCourseIdAsync(int courseId, QueryParameters queryParams);
    Task<StudentResponse> GetByIdAsync(int id, string? expand = null);
    Task<StudentResponse> CreateAsync(CreateStudentRequest request);
    Task UpdateAsync(int id, CreateStudentRequest request);
    Task DeleteAsync(int id);
}

using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;

namespace PRN232.LMSSystem.Services.Interfaces;

public interface ISubjectService
{
    Task<(IEnumerable<SubjectResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams);
    Task<SubjectResponse> GetByIdAsync(int id, string? expand = null);
    Task<SubjectResponse> CreateAsync(SubjectRequest request);
    Task UpdateAsync(int id, SubjectRequest request);
    Task DeleteAsync(int id);
}

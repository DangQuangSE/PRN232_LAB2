using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;

namespace PRN232.LMSSystem.Services.Interfaces;

public interface ISemesterService
{
    Task<(IEnumerable<SemesterResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams);
    Task<SemesterResponse> GetByIdAsync(int id, string? expand = null);
    Task<SemesterResponse> CreateAsync(SemesterRequest request);
    Task UpdateAsync(int id, SemesterRequest request);
    Task DeleteAsync(int id);
}

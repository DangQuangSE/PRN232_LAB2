using Microsoft.EntityFrameworkCore;
using PRN232.LMSSystem.Repositories.Entities;
using PRN232.LMSSystem.Repositories.Interfaces;
using PRN232.LMSSystem.Services.Exceptions;
using PRN232.LMSSystem.Services.Helpers;
using PRN232.LMSSystem.Services.Interfaces;
using PRN232.LMSSystem.Services.Models.Business;
using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;
using System.Linq.Expressions;

namespace PRN232.LMSSystem.Services.Services;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _semesterRepository;

    public SemesterService(ISemesterRepository semesterRepository)
    {
        _semesterRepository = semesterRepository;
    }

    public async Task<(IEnumerable<SemesterResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams)
    {
        Expression<Func<Semester, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower().Trim();
            filter = s => s.SemesterName.ToLower().Contains(searchLower);
        }

        int totalItems = await _semesterRepository.CountAsync(filter);
        var pagination = new PaginationMetadata(queryParams.Page, queryParams.PageSize, totalItems);

        Func<IQueryable<Semester>, IOrderedQueryable<Semester>>? orderBy = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
            orderBy = q => (IOrderedQueryable<Semester>)QueryHelper.ApplySort(q, queryParams.Sort);
        else
            orderBy = q => q.OrderBy(s => s.SemesterId);

        var semesters = await _semesterRepository.GetAllAsync(
            filter: filter,
            orderBy: orderBy,
            includeProperties: new List<string> { "Courses" },
            page: queryParams.Page,
            pageSize: queryParams.PageSize
        );

        return (semesters.Select(s => MapToResponse(s, queryParams.Expand)), pagination);
    }

    public async Task<SemesterResponse> GetByIdAsync(int id, string? expand = null)
    {
        var semester = await _semesterRepository.GetByIdAsync(id, new List<string> { "Courses" })
            ?? throw new NotFoundException("Semester", id);

        return MapToResponse(semester, expand);
    }

    public async Task<SemesterResponse> CreateAsync(SemesterRequest request)
    {
        var semester = new Semester
        {
            SemesterName = request.SemesterName,
            StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc)
        };

        await _semesterRepository.AddAsync(semester);
        await _semesterRepository.SaveAsync();

        return MapToResponse(semester);
    }

    public async Task UpdateAsync(int id, SemesterRequest request)
    {
        var semester = await _semesterRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Semester", id);

        semester.SemesterName = request.SemesterName;
        semester.StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
        semester.EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);

        _semesterRepository.Update(semester);
        await _semesterRepository.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var semester = await _semesterRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Semester", id);

        _semesterRepository.Delete(semester);
        await _semesterRepository.SaveAsync();
    }

    private SemesterBM MapToBusinessModel(Semester semester) => new()
    {
        SemesterId = semester.SemesterId,
        SemesterName = semester.SemesterName,
        StartDate = semester.StartDate,
        EndDate = semester.EndDate
    };

    private SemesterResponse MapToResponse(Semester semester, string? expand = null)
    {
        var bm = MapToBusinessModel(semester);
        var response = new SemesterResponse
        {
            SemesterId = bm.SemesterId,
            SemesterName = bm.SemesterName,
            StartDate = bm.StartDate,
            EndDate = bm.EndDate,
            CourseCount = semester.Courses?.Count ?? 0
        };

        if (!string.IsNullOrWhiteSpace(expand))
        {
            var expands = expand.ToLower().Split(',');
            if (expands.Contains("courses") && semester.Courses != null)
            {
                response.Courses = semester.Courses.Select(c => new SemesterCourseResponse
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName
                }).ToList();
            }
        }

        return response;
    }
}

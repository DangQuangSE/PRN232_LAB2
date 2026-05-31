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

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public EnrollmentService(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<(IEnumerable<EnrollmentResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams)
    {
        var includes = new List<string> { "Student", "Course" };

        Expression<Func<Enrollment, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower().Trim();
            filter = e => e.Status.ToLower().Contains(searchLower) ||
                          e.Student.FullName.ToLower().Contains(searchLower) ||
                          e.Course.CourseName.ToLower().Contains(searchLower);
        }

        int totalItems = await _enrollmentRepository.CountAsync(filter);
        var pagination = new PaginationMetadata(queryParams.Page, queryParams.PageSize, totalItems);

        if (!string.IsNullOrWhiteSpace(queryParams.Expand))
        {
            var expands = queryParams.Expand.ToLower().Split(',');
            if (expands.Contains("course"))
                includes.Add("Course.Semester");
        }

        Func<IQueryable<Enrollment>, IOrderedQueryable<Enrollment>>? orderBy = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
            orderBy = q => (IOrderedQueryable<Enrollment>)QueryHelper.ApplySort(q, queryParams.Sort);
        else
            orderBy = q => q.OrderBy(e => e.EnrollmentId);

        var enrollments = await _enrollmentRepository.GetAllAsync(
            filter: filter,
            orderBy: orderBy,
            includeProperties: includes,
            page: queryParams.Page,
            pageSize: queryParams.PageSize
        );

        return (enrollments.Select(e => MapToResponse(e, queryParams.Expand)), pagination);
    }

    public async Task<(IEnumerable<EnrollmentOfCourseResponse> Data, PaginationMetadata Pagination)> GetByCourseIdAsync(int courseId, QueryParameters queryParams)
    {
        var includes = new List<string> { "Student" };

        Expression<Func<Enrollment, bool>> baseFilter = e => e.CourseId == courseId;

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower().Trim();
            baseFilter = e => e.CourseId == courseId &&
                (e.Status.ToLower().Contains(searchLower) ||
                 e.Student.FullName.ToLower().Contains(searchLower));
        }

        int totalItems = await _enrollmentRepository.CountAsync(baseFilter);
        var pagination = new PaginationMetadata(queryParams.Page, queryParams.PageSize, totalItems);

        Func<IQueryable<Enrollment>, IOrderedQueryable<Enrollment>>? orderBy = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
            orderBy = q => (IOrderedQueryable<Enrollment>)QueryHelper.ApplySort(q, queryParams.Sort);
        else
            orderBy = q => q.OrderBy(e => e.EnrollmentId);

        var enrollments = await _enrollmentRepository.GetAllAsync(
            filter: baseFilter,
            orderBy: orderBy,
            includeProperties: includes,
            page: queryParams.Page,
            pageSize: queryParams.PageSize
        );

        var expands = queryParams.Expand?.ToLower().Split(',') ?? [];
        return (enrollments.Select(e => MapToEnrollmentOfCourseResponse(e, expands)), pagination);
    }

    public async Task<(IEnumerable<EnrollmentOfStudentResponse> Data, PaginationMetadata Pagination)> GetByStudentIdAsync(int studentId, QueryParameters queryParams)
    {
        var includes = new List<string> { "Course" };

        if (!string.IsNullOrWhiteSpace(queryParams.Expand))
        {
            var expands = queryParams.Expand.ToLower().Split(',');
            if (expands.Contains("course"))
                includes.Add("Course.Semester");
        }

        Expression<Func<Enrollment, bool>> baseFilter = e => e.StudentId == studentId;

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower().Trim();
            baseFilter = e => e.StudentId == studentId &&
                (e.Status.ToLower().Contains(searchLower) ||
                 e.Course.CourseName.ToLower().Contains(searchLower));
        }

        int totalItems = await _enrollmentRepository.CountAsync(baseFilter);
        var pagination = new PaginationMetadata(queryParams.Page, queryParams.PageSize, totalItems);

        Func<IQueryable<Enrollment>, IOrderedQueryable<Enrollment>>? orderBy = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
            orderBy = q => (IOrderedQueryable<Enrollment>)QueryHelper.ApplySort(q, queryParams.Sort);
        else
            orderBy = q => q.OrderBy(e => e.EnrollmentId);

        var enrollments = await _enrollmentRepository.GetAllAsync(
            filter: baseFilter,
            orderBy: orderBy,
            includeProperties: includes,
            page: queryParams.Page,
            pageSize: queryParams.PageSize
        );

        var expands2 = queryParams.Expand?.ToLower().Split(',') ?? [];
        return (enrollments.Select(e => MapToEnrollmentOfStudentResponse(e, expands2)), pagination);
    }

    private EnrollmentOfStudentResponse MapToEnrollmentOfStudentResponse(Enrollment enrollment, string[] expands)
    {
        var response = new EnrollmentOfStudentResponse
        {
            EnrollmentId = enrollment.EnrollmentId,
            CourseId = enrollment.CourseId,
            CourseName = enrollment.Course?.CourseName,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status
        };

        if (expands.Contains("course") && enrollment.Course != null)
        {
            response.Course = new CourseBriefResponse
            {
                CourseId = enrollment.Course.CourseId,
                CourseName = enrollment.Course.CourseName,
                SemesterId = enrollment.Course.SemesterId,
                SemesterName = enrollment.Course.Semester?.SemesterName,
                EnrollmentCount = enrollment.Course.Enrollments?.Count
            };
        }

        return response;
    }

    private EnrollmentOfCourseResponse MapToEnrollmentOfCourseResponse(Enrollment enrollment, string[] expands)
    {
        var response = new EnrollmentOfCourseResponse
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            StudentName = enrollment.Student?.FullName,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status
        };

        if (expands.Contains("student") && enrollment.Student != null)
        {
            response.Student = new StudentBriefResponse
            {
                StudentId = enrollment.Student.StudentId,
                FullName = enrollment.Student.FullName,
                Email = enrollment.Student.Email,
                DateOfBirth = enrollment.Student.DateOfBirth
            };
        }

        return response;
    }

    public async Task<EnrollmentResponse> GetByIdAsync(int id, string? expand = null)
    {
        var includes = new List<string> { "Student", "Course" };

        if (!string.IsNullOrWhiteSpace(expand))
        {
            var expands = expand.ToLower().Split(',');
            if (expands.Contains("course"))
                includes.Add("Course.Semester");
        }

        var enrollment = await _enrollmentRepository.GetByIdAsync(id, includes)
            ?? throw new NotFoundException("Enrollment", id);

        return MapToResponse(enrollment, expand);
    }

    public async Task<EnrollmentResponse> CreateAsync(EnrollmentRequest request)
    {
        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollDate = DateTime.SpecifyKind(request.EnrollDate, DateTimeKind.Utc),
            Status = request.Status
        };

        await _enrollmentRepository.AddAsync(enrollment);
        await _enrollmentRepository.SaveAsync();

        var loaded = await _enrollmentRepository.GetByIdAsync(enrollment.EnrollmentId, new List<string> { "Student", "Course" });
        return MapToResponse(loaded ?? enrollment, "student,course");
    }

    public async Task UpdateAsync(int id, EnrollmentRequest request)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Enrollment", id);

        enrollment.StudentId = request.StudentId;
        enrollment.CourseId = request.CourseId;
        enrollment.EnrollDate = DateTime.SpecifyKind(request.EnrollDate, DateTimeKind.Utc);
        enrollment.Status = request.Status;

        _enrollmentRepository.Update(enrollment);
        await _enrollmentRepository.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Enrollment", id);

        _enrollmentRepository.Delete(enrollment);
        await _enrollmentRepository.SaveAsync();
    }

    private EnrollmentBM MapToBusinessModel(Enrollment enrollment) => new()
    {
        EnrollmentId = enrollment.EnrollmentId,
        StudentId = enrollment.StudentId,
        CourseId = enrollment.CourseId,
        EnrollDate = enrollment.EnrollDate,
        Status = enrollment.Status
    };

    private EnrollmentResponse MapToResponse(Enrollment enrollment, string? expand = null)
    {
        var bm = MapToBusinessModel(enrollment);

        var response = new EnrollmentResponse
        {
            EnrollmentId = bm.EnrollmentId,
            StudentId = bm.StudentId,
            StudentName = enrollment.Student?.FullName,
            CourseId = bm.CourseId,
            CourseName = enrollment.Course?.CourseName,
            EnrollDate = bm.EnrollDate,
            Status = bm.Status
        };

        if (!string.IsNullOrWhiteSpace(expand))
        {
            var expands = expand.ToLower().Split(',');

            if (expands.Contains("student") && enrollment.Student != null)
            {
                response.Student = new StudentBriefResponse
                {
                    StudentId = enrollment.Student.StudentId,
                    FullName = enrollment.Student.FullName,
                    Email = enrollment.Student.Email,
                    DateOfBirth = enrollment.Student.DateOfBirth
                };
            }

            if (expands.Contains("course") && enrollment.Course != null)
            {
                response.Course = new CourseBriefResponse
                {
                    CourseId = enrollment.Course.CourseId,
                    CourseName = enrollment.Course.CourseName,
                    SemesterId = enrollment.Course.SemesterId,
                    SemesterName = enrollment.Course.Semester?.SemesterName,
                    EnrollmentCount = enrollment.Course.Enrollments?.Count ?? 0
                };
            }
        }

        return response;
    }
}

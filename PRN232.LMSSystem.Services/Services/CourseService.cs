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

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;

    public CourseService(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<(IEnumerable<CourseResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams)
    {
        Expression<Func<Course, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower().Trim();
            filter = c => c.CourseName.ToLower().Contains(searchLower);
        }

        int totalItems = await _courseRepository.CountAsync(filter);
        var pagination = new PaginationMetadata(queryParams.Page, queryParams.PageSize, totalItems);

        Func<IQueryable<Course>, IOrderedQueryable<Course>>? orderBy = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
            orderBy = q => (IOrderedQueryable<Course>)QueryHelper.ApplySort(q, queryParams.Sort);
        else
            orderBy = q => q.OrderBy(c => c.CourseId);

        bool includeEnrollments = !string.IsNullOrWhiteSpace(queryParams.Expand) &&
            queryParams.Expand.ToLower().Split(',').Contains("enrollments");

        var courses = await _courseRepository.GetCoursesWithCountAsync(
            filter: filter,
            orderBy: orderBy,
            page: queryParams.Page,
            pageSize: queryParams.PageSize,
            includeEnrollments: includeEnrollments
        );

        return (courses.Select(c => MapToResponse(c, queryParams.Expand)), pagination);
    }

    public async Task<(IEnumerable<CourseBriefResponse> Data, PaginationMetadata Pagination)> GetBySemesterIdAsync(int semesterId, QueryParameters queryParams)
    {
        Expression<Func<Course, bool>> filter = c => c.SemesterId == semesterId;

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower().Trim();
            filter = c => c.SemesterId == semesterId && c.CourseName.ToLower().Contains(searchLower);
        }

        int totalItems = await _courseRepository.CountAsync(filter);
        var pagination = new PaginationMetadata(queryParams.Page, queryParams.PageSize, totalItems);

        Func<IQueryable<Course>, IOrderedQueryable<Course>>? orderBy = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
            orderBy = q => (IOrderedQueryable<Course>)QueryHelper.ApplySort(q, queryParams.Sort);
        else
            orderBy = q => q.OrderBy(c => c.CourseId);

        var courses = await _courseRepository.GetCoursesWithCountAsync(
            filter: filter,
            orderBy: orderBy,
            page: queryParams.Page,
            pageSize: queryParams.PageSize
        );

        return (courses.Select(c => new CourseBriefResponse
        {
            CourseId = c.Course.CourseId,
            CourseName = c.Course.CourseName,
            SemesterId = c.Course.SemesterId,
            SemesterName = c.Course.Semester?.SemesterName,
            EnrollmentCount = c.EnrollmentCount
        }), pagination);
    }

    public async Task<CourseResponse> GetByIdAsync(int id, string? expand = null)
    {
        bool includeEnrollments = expand?.ToLower().Split(',').Contains("enrollments") ?? false;
        var courseWithCount = await _courseRepository.GetCourseWithCountByIdAsync(id, includeEnrollments)
            ?? throw new NotFoundException("Course", id);

        return MapToResponse(courseWithCount, expand);
    }

    public async Task<CourseResponse> CreateAsync(CourseRequest request)
    {
        var course = new Course
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId
        };

        await _courseRepository.AddAsync(course);
        await _courseRepository.SaveAsync();

        var loaded = await _courseRepository.GetCourseWithCountByIdAsync(course.CourseId);
        return MapToResponse(loaded ?? new CourseWithCount { Course = course, EnrollmentCount = 0 }, null);
    }

    public async Task UpdateAsync(int id, CourseRequest request)
    {
        var course = await _courseRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Course", id);

        course.CourseName = request.CourseName;
        course.SemesterId = request.SemesterId;

        _courseRepository.Update(course);
        await _courseRepository.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var course = await _courseRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Course", id);

        _courseRepository.Delete(course);
        await _courseRepository.SaveAsync();
    }

    private CourseBM MapToBusinessModel(Course course) => new()
    {
        CourseId = course.CourseId,
        CourseName = course.CourseName,
        SemesterId = course.SemesterId
    };

    private CourseResponse MapToResponse(CourseWithCount courseWithCount, string? expand = null)
    {
        var course = courseWithCount.Course;
        var bm = MapToBusinessModel(course);

        var response = new CourseResponse
        {
            CourseId = bm.CourseId,
            CourseName = bm.CourseName,
            SemesterId = bm.SemesterId,
            SemesterName = course.Semester?.SemesterName,
            EnrollmentCount = courseWithCount.EnrollmentCount
        };

        if (!string.IsNullOrWhiteSpace(expand))
        {
            var expands = expand.ToLower().Split(',');

            if (expands.Contains("semester") && course.Semester != null)
            {
                response.Semester = new SemesterBriefResponse
                {
                    SemesterId = course.Semester.SemesterId,
                    SemesterName = course.Semester.SemesterName,
                    StartDate = course.Semester.StartDate,
                    EndDate = course.Semester.EndDate,
                    CourseCount = course.Semester.Courses?.Count ?? 0
                };
            }

            if (expands.Contains("enrollments") && course.Enrollments != null)
            {
                response.Enrollments = course.Enrollments.Select(e => new CourseEnrollmentResponse
                {
                    EnrollmentId = e.EnrollmentId,
                    StudentId = e.StudentId,
                    StudentName = e.Student?.FullName ?? string.Empty,
                    EnrollDate = e.EnrollDate,
                    Status = e.Status
                }).ToList();
            }
        }

        return response;
    }
}

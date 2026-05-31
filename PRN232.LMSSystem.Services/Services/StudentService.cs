using FluentValidation;
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

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IValidator<CreateStudentRequest> _validator;

    public StudentService(IStudentRepository studentRepository, IValidator<CreateStudentRequest> validator)
    {
        _studentRepository = studentRepository;
        _validator = validator;
    }

    public async Task<(IEnumerable<StudentResponse> Data, PaginationMetadata Pagination)> GetAllAsync(QueryParameters queryParams)
    {
        Expression<Func<Student, bool>>? filter = null;
        
        // Custom search: search in FullName, Email, StudentCode, Phone
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower().Trim();
            filter = s => s.FullName.ToLower().Contains(searchLower) 
                       || s.Email.ToLower().Contains(searchLower)
                       || s.StudentCode.ToLower().Contains(searchLower)
                       || s.Phone.ToLower().Contains(searchLower);
        }

        int totalItems = await _studentRepository.CountAsync(filter);
        var pagination = new PaginationMetadata(queryParams.Page, queryParams.PageSize, totalItems);

        var includes = new List<string>();
        if (!string.IsNullOrWhiteSpace(queryParams.Expand))
        {
            var expands = queryParams.Expand.ToLower().Split(',');
            if (expands.Contains("enrollments"))
            {
                includes.Add("Enrollments");
                includes.Add("Enrollments.Course");
            }
        }

        Func<IQueryable<Student>, IOrderedQueryable<Student>>? orderBy = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
            orderBy = q => (IOrderedQueryable<Student>)QueryHelper.ApplySort(q, queryParams.Sort);
        else
            orderBy = q => q.OrderBy(s => s.StudentId);

        var students = await _studentRepository.GetAllAsync(
            filter: filter,
            orderBy: orderBy,
            includeProperties: includes,
            page: queryParams.Page,
            pageSize: queryParams.PageSize
        );

        return (students.Select(s => MapToResponse(s, queryParams.Expand)), pagination);
    }

    public async Task<(IEnumerable<StudentResponse> Data, PaginationMetadata Pagination)> GetStudentsByCourseIdAsync(int courseId, QueryParameters queryParams)
    {
        // Query students enrolled in the given course ID
        Expression<Func<Student, bool>> filter = s => s.Enrollments.Any(e => e.CourseId == courseId);
        
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower().Trim();
            filter = s => s.Enrollments.Any(e => e.CourseId == courseId) && 
                         (s.FullName.ToLower().Contains(searchLower) 
                          || s.Email.ToLower().Contains(searchLower)
                          || s.StudentCode.ToLower().Contains(searchLower)
                          || s.Phone.ToLower().Contains(searchLower));
        }

        int totalItems = await _studentRepository.CountAsync(filter);
        var pagination = new PaginationMetadata(queryParams.Page, queryParams.PageSize, totalItems);

        var includes = new List<string>();
        if (!string.IsNullOrWhiteSpace(queryParams.Expand))
        {
            var expands = queryParams.Expand.ToLower().Split(',');
            if (expands.Contains("enrollments"))
            {
                includes.Add("Enrollments");
                includes.Add("Enrollments.Course");
            }
        }

        Func<IQueryable<Student>, IOrderedQueryable<Student>>? orderBy = null;
        if (!string.IsNullOrWhiteSpace(queryParams.Sort))
            orderBy = q => (IOrderedQueryable<Student>)QueryHelper.ApplySort(q, queryParams.Sort);
        else
            orderBy = q => q.OrderBy(s => s.StudentId);

        var students = await _studentRepository.GetAllAsync(
            filter: filter,
            orderBy: orderBy,
            includeProperties: includes,
            page: queryParams.Page,
            pageSize: queryParams.PageSize
        );

        return (students.Select(s => MapToResponse(s, queryParams.Expand)), pagination);
    }

    public async Task<StudentResponse> GetByIdAsync(int id, string? expand = null)
    {
        var includes = new List<string>();
        if (!string.IsNullOrWhiteSpace(expand))
        {
            var expands = expand.ToLower().Split(',');
            if (expands.Contains("enrollments"))
            {
                includes.Add("Enrollments");
                includes.Add("Enrollments.Course");
            }
        }

        var student = await _studentRepository.GetByIdAsync(id, includes)
            ?? throw new NotFoundException("Student", id);

        return MapToResponse(student, expand);
    }

    public async Task<StudentResponse> CreateAsync(CreateStudentRequest request)
    {
        // 1. Run FluentValidation explicitly
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new BadRequestException("Validation failed", errors);
        }

        // 2. Check duplicate student code
        var existingStudent = await _studentRepository.GetAllAsync(s => s.StudentCode.ToLower() == request.StudentCode.ToLower().Trim());
        if (existingStudent.Any())
        {
            throw new BadRequestException("StudentCode is already in use.", new Dictionary<string, string[]> {
                { "studentCode", new[] { $"Student code '{request.StudentCode}' is already registered." } }
            });
        }

        var student = new Student
        {
            StudentCode = request.StudentCode.Trim().ToUpper(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Utc)
        };

        await _studentRepository.AddAsync(student);
        await _studentRepository.SaveAsync();

        return MapToResponse(student, null);
    }

    public async Task UpdateAsync(int id, CreateStudentRequest request)
    {
        // 1. Run FluentValidation explicitly
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new BadRequestException("Validation failed", errors);
        }

        var student = await _studentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Student", id);

        // 2. Check duplicate student code (excluding currently updating student)
        var existingStudent = await _studentRepository.GetAllAsync(s => s.StudentCode.ToLower() == request.StudentCode.ToLower().Trim() && s.StudentId != id);
        if (existingStudent.Any())
        {
            throw new BadRequestException("StudentCode is already in use.", new Dictionary<string, string[]> {
                { "studentCode", new[] { $"Student code '{request.StudentCode}' is already registered." } }
            });
        }

        student.StudentCode = request.StudentCode.Trim().ToUpper();
        student.FullName = request.FullName.Trim();
        student.Email = request.Email.Trim();
        student.Phone = request.Phone.Trim();
        student.DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Utc);

        _studentRepository.Update(student);
        await _studentRepository.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var student = await _studentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Student", id);

        _studentRepository.Delete(student);
        await _studentRepository.SaveAsync();
    }

    private StudentBM MapToBusinessModel(Student student) => new()
    {
        StudentId = student.StudentId,
        StudentCode = student.StudentCode,
        FullName = student.FullName,
        Email = student.Email,
        Phone = student.Phone,
        DateOfBirth = student.DateOfBirth
    };

    private StudentResponse MapToResponse(Student student, string? expand)
    {
        var bm = MapToBusinessModel(student);

        var response = new StudentResponse
        {
            StudentId = bm.StudentId,
            StudentCode = bm.StudentCode,
            FullName = bm.FullName,
            Email = bm.Email,
            Phone = bm.Phone,
            DateOfBirth = bm.DateOfBirth
        };

        if (!string.IsNullOrWhiteSpace(expand))
        {
            var expands = expand.ToLower().Split(',');
            if (expands.Contains("enrollments") && student.Enrollments != null)
            {
                response.Enrollments = student.Enrollments.Select(e => new EnrollmentBriefResponse
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    CourseName = e.Course?.CourseName ?? string.Empty,
                    EnrollDate = e.EnrollDate,
                    Status = e.Status
                }).ToList();
            }
        }

        return response;
    }
}

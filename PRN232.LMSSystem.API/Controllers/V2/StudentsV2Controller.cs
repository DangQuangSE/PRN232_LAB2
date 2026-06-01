using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMSSystem.Services.Interfaces;
using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Response;

namespace PRN232.LMSSystem.API.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/students")]
[Authorize]
[Produces("application/json", "application/xml")]
public class StudentsV2Controller : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsV2Controller(IStudentService studentService)
    {
        _studentService = studentService;
    }

    /// <summary>
    /// [v2] Get all students — simplified response with core identity fields only.
    /// Breaking change from v1: no enrollments, no phone, no dateOfBirth.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentV2Response>>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters queryParams)
    {
        var (data, pagination) = await _studentService.GetAllAsync(queryParams);
        var v2Data = data.Select(s => new StudentV2Response
        {
            StudentId = s.StudentId,
            StudentCode = s.StudentCode,
            FullName = s.FullName,
            Email = s.Email
        });
        return Ok(ApiResponse<object>.SuccessResponse(v2Data, "Students retrieved successfully", pagination));
    }

    /// <summary>
    /// [v2] Get student by ID — simplified response with core identity fields only.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentV2Response>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var student = await _studentService.GetByIdAsync(id);
        var v2Data = new StudentV2Response
        {
            StudentId = student.StudentId,
            StudentCode = student.StudentCode,
            FullName = student.FullName,
            Email = student.Email
        };
        return Ok(ApiResponse<StudentV2Response>.SuccessResponse(v2Data, "Student retrieved successfully"));
    }
}

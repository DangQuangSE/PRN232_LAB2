using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PRN232.LMSSystem.API.Helpers;
using PRN232.LMSSystem.Services.Interfaces;
using PRN232.LMSSystem.Services.Models.Query;
using PRN232.LMSSystem.Services.Models.Request;
using PRN232.LMSSystem.Services.Models.Response;
using Asp.Versioning;

namespace PRN232.LMSSystem.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/students")]
[Authorize]
[Produces("application/json", "application/xml")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IDataShaper<StudentResponse> _dataShaper;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IDataShaper<EnrollmentOfStudentResponse> _enrollmentDataShaper;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(
        IStudentService studentService,
        IDataShaper<StudentResponse> dataShaper,
        IEnrollmentService enrollmentService,
        IDataShaper<EnrollmentOfStudentResponse> enrollmentDataShaper,
        ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _dataShaper = dataShaper;
        _enrollmentService = enrollmentService;
        _enrollmentDataShaper = enrollmentDataShaper;
        _logger = logger;
    }

    [HttpGet]
    [ExpandOptions("enrollments")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentResponse>>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAll([FromQuery] StudentQueryRequest request)
    {
        var (data, pagination) = await _studentService.GetAllAsync(request);
        var shapedData = _dataShaper.ShapeData(data, request.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Students retrieved successfully", pagination));
    }

    [HttpGet("{id:int}", Name = "GetStudentById")]
    [ExpandOptions("enrollments")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] QueryParameters queryParams)
    {
        var student = await _studentService.GetByIdAsync(id, queryParams.Expand);
        var shapedData = _dataShaper.ShapeData(student, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Student retrieved successfully"));
    }

    [HttpGet("{id:int}/enrollments")]
    [ExpandOptions("course")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EnrollmentOfStudentResponse>>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetEnrollments([FromRoute] int id, [FromQuery] QueryParameters queryParams)
    {
        await _studentService.GetByIdAsync(id);
        var (data, pagination) = await _enrollmentService.GetByStudentIdAsync(id, queryParams);
        var shapedData = _enrollmentDataShaper.ShapeData(data, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Student enrollments retrieved successfully", pagination));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStudentRequest request, 
        [FromHeader(Name = "X-Request-Id")] string? requestId)
    {
        if (requestId != null)
        {
            _logger.LogInformation("Processing create student request with X-Request-Id: {RequestId}", requestId);
        }

        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        var student = await _studentService.CreateAsync(request);
        return CreatedAtRoute("GetStudentById", new { id = student.StudentId, version = "1" },
            ApiResponse<StudentResponse>.SuccessResponse(student, "Student created successfully"));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CreateStudentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        await _studentService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Student updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await _studentService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Student deleted successfully"));
    }
}

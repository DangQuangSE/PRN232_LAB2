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
[Route("api/v{version:apiVersion}/courses")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IDataShaper<CourseResponse> _dataShaper;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IDataShaper<EnrollmentOfCourseResponse> _enrollmentDataShaper;
    private readonly IStudentService _studentService;
    private readonly IDataShaper<StudentResponse> _studentServiceDataShaper;

    public CoursesController(
        ICourseService courseService,
        IDataShaper<CourseResponse> dataShaper,
        IEnrollmentService enrollmentService,
        IDataShaper<EnrollmentOfCourseResponse> enrollmentDataShaper,
        IStudentService studentService,
        IDataShaper<StudentResponse> studentServiceDataShaper)
    {
        _courseService = courseService;
        _dataShaper = dataShaper;
        _enrollmentService = enrollmentService;
        _enrollmentDataShaper = enrollmentDataShaper;
        _studentService = studentService;
        _studentServiceDataShaper = studentServiceDataShaper;
    }

    [HttpGet]
    [ExpandOptions("semester", "enrollments")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CourseResponse>>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters queryParams)
    {
        var (data, pagination) = await _courseService.GetAllAsync(queryParams);
        var shapedData = _dataShaper.ShapeData(data, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Courses retrieved successfully", pagination));
    }

    [HttpGet("{id:int}")]
    [ExpandOptions("semester", "enrollments")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] QueryParameters queryParams)
    {
        var course = await _courseService.GetByIdAsync(id, queryParams.Expand);
        var shapedData = _dataShaper.ShapeData(course, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Course retrieved successfully"));
    }

    [HttpGet("{id:int}/enrollments")]
    [ExpandOptions("student")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EnrollmentOfCourseResponse>>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetEnrollments([FromRoute] int id, [FromQuery] QueryParameters queryParams)
    {
        await _courseService.GetByIdAsync(id);
        var (data, pagination) = await _enrollmentService.GetByCourseIdAsync(id, queryParams);
        var shapedData = _enrollmentDataShaper.ShapeData(data, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Course enrollments retrieved successfully", pagination));
    }

    [HttpGet("{courseId:int}/students")]
    [ExpandOptions("enrollments")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StudentResponse>>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetStudentsByCourse([FromRoute] int courseId, [FromQuery] QueryParameters queryParams)
    {
        await _courseService.GetByIdAsync(courseId);
        var (data, pagination) = await _studentService.GetStudentsByCourseIdAsync(courseId, queryParams);
        var shapedData = _studentServiceDataShaper.ShapeData(data, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Course students retrieved successfully", pagination));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Create([FromBody] CourseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        var course = await _courseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = course.CourseId },
            ApiResponse<CourseResponse>.SuccessResponse(course, "Course created successfully"));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CourseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        await _courseService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Course updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await _courseService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Course deleted successfully"));
    }
}

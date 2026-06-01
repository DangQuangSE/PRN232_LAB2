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
[Route("api/v{version:apiVersion}/semesters")]
[Authorize]
[Produces("application/json", "application/xml")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;
    private readonly IDataShaper<SemesterResponse> _dataShaper;
    private readonly ICourseService _courseService;
    private readonly IDataShaper<CourseBriefResponse> _courseDataShaper;

    public SemestersController(
        ISemesterService semesterService,
        IDataShaper<SemesterResponse> dataShaper,
        ICourseService courseService,
        IDataShaper<CourseBriefResponse> courseDataShaper)
    {
        _semesterService = semesterService;
        _dataShaper = dataShaper;
        _courseService = courseService;
        _courseDataShaper = courseDataShaper;
    }

    [HttpGet]
    [ExpandOptions("courses")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SemesterResponse>>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters queryParams)
    {
        var (data, pagination) = await _semesterService.GetAllAsync(queryParams);
        var shapedData = _dataShaper.ShapeData(data, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Semesters retrieved successfully", pagination));
    }

    [HttpGet("{id:int}")]
    [ExpandOptions("courses")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] QueryParameters queryParams)
    {
        var semester = await _semesterService.GetByIdAsync(id, queryParams.Expand);
        var shapedData = _dataShaper.ShapeData(semester, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Semester retrieved successfully"));
    }

    [HttpGet("{id:int}/courses")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CourseBriefResponse>>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetCourses([FromRoute] int id, [FromQuery] QueryParameters queryParams)
    {
        await _semesterService.GetByIdAsync(id);
        var (data, pagination) = await _courseService.GetBySemesterIdAsync(id, queryParams);
        var shapedData = _courseDataShaper.ShapeData(data, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Semester courses retrieved successfully", pagination));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Create([FromBody] SemesterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        var semester = await _semesterService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = semester.SemesterId },
            ApiResponse<SemesterResponse>.SuccessResponse(semester, "Semester created successfully"));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SemesterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        await _semesterService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Semester updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await _semesterService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Semester deleted successfully"));
    }
}

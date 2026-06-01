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
[Route("api/v{version:apiVersion}/subjects")]
[Authorize]
[Produces("application/json", "application/xml")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;
    private readonly IDataShaper<SubjectResponse> _dataShaper;

    public SubjectsController(ISubjectService subjectService, IDataShaper<SubjectResponse> dataShaper)
    {
        _subjectService = subjectService;
        _dataShaper = dataShaper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SubjectResponse>>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters queryParams)
    {
        var (data, pagination) = await _subjectService.GetAllAsync(queryParams);
        var shapedData = _dataShaper.ShapeData(data, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Subjects retrieved successfully", pagination));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById([FromRoute] int id, [FromQuery] QueryParameters queryParams)
    {
        var subject = await _subjectService.GetByIdAsync(id, queryParams.Expand);
        var shapedData = _dataShaper.ShapeData(subject, queryParams.Fields);
        return Ok(ApiResponse<object>.SuccessResponse(shapedData, "Subject retrieved successfully"));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Create([FromBody] SubjectRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        var subject = await _subjectService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = subject.SubjectId },
            ApiResponse<SubjectResponse>.SuccessResponse(subject, "Subject created successfully"));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SubjectRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", ModelState));

        await _subjectService.UpdateAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Subject updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await _subjectService.DeleteAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Subject deleted successfully"));
    }
}

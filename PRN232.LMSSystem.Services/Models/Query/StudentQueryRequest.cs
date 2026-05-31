namespace PRN232.LMSSystem.Services.Models.Query;

/// <summary>
/// Query binding model for students list.
/// </summary>
public class StudentQueryRequest : QueryParameters
{
    /// <summary>
    /// Optional major filter (e.g. SE, CE, IA).
    /// </summary>
    public string? Major { get; set; }
}

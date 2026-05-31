namespace PRN232.LMSSystem.Services.Models.Query;

/// <summary>
/// Parameters for querying, filtering, sorting, paging, field selection and expansion of related data.
/// </summary>
public class QueryParameters
{
    private const int MaxPageSize = 250;
    private int _pageSize = 10;

    /// <summary>Search keyword to filter results. Searches across relevant text fields (e.g. name, email, status).</summary>
    public string? Search { get; set; }

    /// <summary>
    /// Comma-separated field names to sort by. Prefix with '-' for descending order.
    /// Example: "fullName" for ascending, "-dateOfBirth" for descending, "fullName,-dateOfBirth" for multi-sort.
    /// </summary>
    public string? Sort { get; set; }

    /// <summary>Page number (1-based). Defaults to 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Number of items per page. Defaults to 10, max 250.</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>Alias for PageSize. Number of items per page. Defaults to 10, max 250.</summary>
    public int Size
    {
        get => PageSize;
        set => PageSize = value;
    }

    /// <summary>
    /// Comma-separated field names to include in response (field selection / projection).
    /// If omitted, all fields are returned.
    /// Example: "studentId,fullName,email"
    /// </summary>
    public string? Fields { get; set; }

    /// <summary>
    /// Comma-separated related entities to include in the response (expansion).
    /// Available expansions depend on the resource: e.g. "student,course" for enrollments, "enrollments" for students.
    /// </summary>
    public string? Expand { get; set; }
}

namespace PRN232.LMSSystem.Services.Models.Business;

public class SemesterBM
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

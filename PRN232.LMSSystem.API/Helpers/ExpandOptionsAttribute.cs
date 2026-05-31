namespace PRN232.LMSSystem.API.Helpers;

[AttributeUsage(AttributeTargets.Method)]
public class ExpandOptionsAttribute : Attribute
{
    public string[] Options { get; }
    public ExpandOptionsAttribute(params string[] options) => Options = options;
}

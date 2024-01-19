namespace RocketQL.Core.Extensions;

public static class CallerExtensions
{
    public static string CallerToSource(string filePath, string memberName, int lineNumber)
    {
        return $"{Path.GetFileName(filePath)}, {memberName}, {lineNumber}";
    }
}

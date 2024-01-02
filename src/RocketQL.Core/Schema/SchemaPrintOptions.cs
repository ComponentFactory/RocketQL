
namespace RocketQL.Core.Base;

public class SchemaPrintOptions
{
    public int Indent { get; set; } = 4;
    public bool PrintDescriptions { get; set; } = true;
    public bool PrintPredefined { get; set; } = false;
}
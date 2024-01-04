
namespace RocketQL.Core.Base;

public class SchemaPrintOptions
{
    public PrintIndentCharacter IndentCharacter { get; set; } = PrintIndentCharacter.Space;
    public int IndentCount { get; set; } = 4;
    public bool IncludeDescriptions { get; set; } = true;
    public bool IncludeBuiltIn { get; set; } = false;
    public bool IncludeUnrooted { get; set; } = true;
}
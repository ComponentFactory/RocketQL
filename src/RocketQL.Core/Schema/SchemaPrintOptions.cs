
namespace RocketQL.Core.Base;

public class SchemaPrintOptions
{
    public PrintIndentCharacter IndentCharacter { get; set; } = PrintIndentCharacter.Space;
    public int IndentCount { get; set; } = 4;
    public bool PrintDescriptions { get; set; } = true;
    public bool PrintBuiltIn { get; set; } = false;
}
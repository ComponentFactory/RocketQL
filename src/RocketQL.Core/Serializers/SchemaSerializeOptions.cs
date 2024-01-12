namespace RocketQL.Core.Serializers;

public class SchemaSerializeOptions
{
    public IndentCharacter IndentCharacter { get; set; } = IndentCharacter.Space;
    public int IndentCount { get; set; } = 4;
    public bool IncludeDescription { get; set; } = true;
    public bool IncludeBuiltIn { get; set; } = false;
    public bool IncludeUnrooted { get; set; } = true;
}
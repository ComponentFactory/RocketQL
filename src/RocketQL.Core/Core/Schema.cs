using RocketQL.Core.Serializers;

namespace RocketQL.Core;

public class Schema
{
    public Schema()
    {
    }

    public Dictionary<string, DirectiveNode> Directives { get; init; } = new();

    public void Merge(ReadOnlySpan<char> schema,
                      [CallerFilePath] string filePath = "",
                      [CallerMemberName] string memberName = "",
                      [CallerLineNumber] int lineNumber = 0)
    {
        Merge(Serialization.SchemaDeserialize(schema, $"{filePath}, {memberName}, {lineNumber}"));
    }

    public void Merge(ReadOnlySpan<char> schema, string source)
    {
        Merge(Serialization.SchemaDeserialize(schema, source));
    }

    public void Merge(SyntaxSchemaNode schema)
    {
        Merge(new SyntaxSchemaNode[] { schema });
    }

    public void Merge(IEnumerable<SyntaxSchemaNode> schemas)
    {
        foreach (var schema in schemas)
        {

        }
    }

    private void MergeDirective(SyntaxDirectiveDefinitionNodeList directives)
    {
        foreach(var directive in directives) 
        {
            // Each directive must have a unique name
            if (Directives.ContainsKey(directive.Name))
                throw ValidationException.DirectiveAlreadyDefined(directive.Location, directive.Name);

            // 
        }
    }
}

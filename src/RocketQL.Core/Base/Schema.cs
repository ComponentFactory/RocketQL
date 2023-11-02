using RocketQL.Core.Serializers;

namespace RocketQL.Core.Base;

public class Schema
{
    public Schema()
    {
    }

    public Dictionary<string, ScalarTypeDefinitionNode> Scalars { get; init; } = new();

    public void Merge(ReadOnlySpan<char> schema,
                      [CallerFilePath] string filePath = "",
                      [CallerMemberName] string memberName = "",
                      [CallerLineNumber] int lineNumber = 0)
    {
        Merge(Serialization.SchemaDeserialize(schema, CallerExtensions.CallerToSource(filePath, memberName, lineNumber)));
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
            AddScalarTypes(schema.ScalarTypes);
    }

    public void AddScalarTypes(IEnumerable<SyntaxScalarTypeDefinitionNode> nodes)
    {
        foreach (var node in nodes)
            AddScalarType(node);
    }

    public void AddScalarType(SyntaxScalarTypeDefinitionNode node)
    {
        if (Scalars.ContainsKey(node.Name))
            throw ValidationException.ScalarAlreadyDefined(node.Location, node.Name);

        Scalars.Add(node.Name, new(node.Description, node.Name, node.Location));
    }

    public void Validate()
    {
    }
}

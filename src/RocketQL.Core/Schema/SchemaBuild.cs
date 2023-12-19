using RocketQL.Core.Nodes;
using RocketQL.Core.Serializers;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private readonly SyntaxNodeList _syntaxNodes = [];

    public Schema()
    {
    }

    public SchemaDefinition Definition { get; protected set; } = new();
    public DirectiveDefinitions Directives { get; init; } = [];
    public TypeDefinitions Types { get; init; } = [];

    public bool IsValidated { get; protected set; } = false;

    public void Add(ReadOnlySpan<char> schema,
                    [CallerFilePath] string filePath = "",
                    [CallerMemberName] string memberName = "",
                    [CallerLineNumber] int lineNumber = 0)
    {
        Add(Serialization.SchemaDeserialize(schema, CallerExtensions.CallerToSource(filePath, memberName, lineNumber)));
    }

    public void Add(ReadOnlySpan<char> schema, string source)
    {
        Add(Serialization.SchemaDeserialize(schema, source));
    }

    public void Add(SyntaxSchemaNode schema)
    {
        Add(new SyntaxSchemaNode[] { schema });
    }

    public void Add(IEnumerable<SyntaxSchemaNode> schemas)
    {
        foreach (var schema in schemas)
        {
            _syntaxNodes.AddRange(schema.Schemas);
            _syntaxNodes.AddRange(schema.Directives);
            _syntaxNodes.AddRange(schema.ScalarTypes);
            _syntaxNodes.AddRange(schema.ObjectTypes);
            _syntaxNodes.AddRange(schema.InterfaceTypes);
            _syntaxNodes.AddRange(schema.UnionTypes);
            _syntaxNodes.AddRange(schema.EnumTypes);
            _syntaxNodes.AddRange(schema.InputObjectTypes);
            _syntaxNodes.AddRange(schema.ExtendSchemas);
            _syntaxNodes.AddRange(schema.ExtendScalarTypes);
            _syntaxNodes.AddRange(schema.ExtendObjectTypes);
            _syntaxNodes.AddRange(schema.ExtendInterfaceTypes);
            _syntaxNodes.AddRange(schema.ExtendUnionTypes);
            _syntaxNodes.AddRange(schema.ExtendEnumTypes);
            _syntaxNodes.AddRange(schema.ExtendInputObjectTypes);
        }
    }

    public void Add(SyntaxNode node) => _syntaxNodes.Add(node);
    public void Add(IEnumerable<SyntaxNode> nodes) => _syntaxNodes.AddRange(nodes);
}

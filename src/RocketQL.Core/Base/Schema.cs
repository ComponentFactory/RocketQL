using RocketQL.Core.Nodes;
using RocketQL.Core.Serializers;
using System.Xml.Linq;

namespace RocketQL.Core.Base;

public class Schema
{
    public Schema()
    {
    }

    public DirectiveDefinitions Directives { get; init; } = new();
    public ScalarTypeDefinitions Scalars { get; init; } = new();

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
        {
            AddDirectives(schema.Directives);
            AddScalarTypes(schema.ScalarTypes);
        }
    }

    public void AddDirectives(IEnumerable<SyntaxDirectiveDefinitionNode> nodes)
    {
        foreach (var node in nodes)
            AddDirective(node);
    }

    public void AddDirective(SyntaxDirectiveDefinitionNode node)
    {
        if (Directives.ContainsKey(node.Name))
            throw ValidationException.DirectiveAlreadyDefined(node.Location, node.Name);

        Directives.Add(node.Name, new()
        {
            Description = node.Description,
            Name = node.Name,
            Arguments = ToInputValueDefinitionNodes(node.Arguments),
            Repeatable = node.Repeatable,
            DirectiveLocations = node.DirectiveLocations,
            Location = node.Location
        });
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

        Scalars.Add(node.Name, new()
        {
            Description = node.Description,
            Name = node.Name,
            Directives = ToDirectiveNodes(node.Directives),
            Location = node.Location
        });
    }

    public void Validate()
    {
    }

    private static InputValueDefinitionNodes ToInputValueDefinitionNodes(SyntaxInputValueDefinitionNodeList inputValues)
    {
        var nodes = new InputValueDefinitionNodes();

        foreach (var inputValue in inputValues)
        {
            nodes.Add(inputValue.Name, new()
            {
                Description = inputValue.Description,
                Name = inputValue.Name,
                Type = ToTypeNode(inputValue.Type),
                DefaultValue = inputValue.DefaultValue,
                Directives = ToDirectiveNodes(inputValue.Directives),
                Location = inputValue.Location
            });
        }

        return nodes;
    }

    private static TypeNode ToTypeNode(SyntaxTypeNode node)
    {
        return node switch
        {
            SyntaxTypeNameNode nameNode => new TypeNameNode() 
            {
                 Name = nameNode.Name,
                 NonNull = nameNode.NonNull,
                 Location = nameNode.Location,
            },
            SyntaxTypeListNode listNode => new TypeListNode() 
            {
                Type = ToTypeNode(listNode.Type),
                Definition = null,
                NonNull = listNode.NonNull,
                Location = listNode.Location,
            },
            _ => throw ValidationException.UnrecognizedType(node.Location, node.GetType().Name)
        }; ;
    }

    private static DirectiveNodes ToDirectiveNodes(SyntaxDirectiveNodeList directives)
    {
        var nodes = new DirectiveNodes();

        foreach(var directive in directives)
        {
            nodes.Add(directive.Name, new()
            { 
                Name = directive.Name,
                Definition = null,
                Arguments = ToObjectFieldNodes(directive.Arguments),
                Location = directive.Location
            });
        }

        return nodes;
    }

    private static ObjectFieldNodes ToObjectFieldNodes(SyntaxObjectFieldNodeList fields)
    {
        var nodes = new ObjectFieldNodes();

        foreach (var field in fields)
            nodes.Add(field.Name, field);

        return nodes;
    }
}

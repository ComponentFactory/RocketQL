using RocketQL.Core.Nodes;
using RocketQL.Core.Serializers;
using System.Xml.Linq;

namespace RocketQL.Core.Base;

public partial class Schema
{
    public Schema()
    {
    }

    public DirectiveDefinitions Directives { get; init; } = new();
    public ScalarTypeDefinitions Scalars { get; init; } = new();
    public InterfaceTypeDefinitions Interfaces { get; init; } = new();
    public EnumTypeDefinitions Enums { get; init; } = new();

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
            AddInterfaceTypes(schema.InterfaceTypes);
            AddEnumTypes(schema.EnumTypes);
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
            Arguments = ToInputValueDefinitions(node.Arguments),
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
            Directives = ToDirectives(node.Directives),
            Location = node.Location
        });
    }


    public void AddInterfaceTypes(IEnumerable<SyntaxInterfaceTypeDefinitionNode> nodes)
    {
        foreach (var node in nodes)
            AddInterfaceType(node);
    }

    public void AddInterfaceType(SyntaxInterfaceTypeDefinitionNode node)
    {
        if (Interfaces.ContainsKey(node.Name))
            throw ValidationException.InterfaceAlreadyDefined(node.Location, node.Name);

        Interfaces.Add(node.Name, new()
        {
            Description = node.Description,
            Name = node.Name,
            ImplementsInterfaces = ToInterfaces(node.ImplementsInterfaces),
            Directives = ToDirectives(node.Directives),
            Fields = ToFieldDefinitions(node.Fields),
            Location = node.Location
        });
    }

    public void AddEnumTypes(IEnumerable<SyntaxEnumTypeDefinitionNode> nodes)
    {
        foreach (var node in nodes)
            AddEnum(node);
    }

    public void AddEnum(SyntaxEnumTypeDefinitionNode node)
    {
        if (Enums.ContainsKey(node.Name))
            throw ValidationException.EnumAlreadyDefined(node.Location, node.Name);

        Enums.Add(node.Name, new()
        {
            Description = node.Description,
            Name = node.Name,
            Directives = ToDirectives(node.Directives),
            EnumValues = ToEnumValues(node.EnumValues),
            Location = node.Location
        });
    }
}

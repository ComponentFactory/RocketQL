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
    public ScalarTypeDefinitions ScalarTypes { get; init; } = new();
    public ObjectTypeDefinitions ObjectTypes { get; init; } = new();
    public InterfaceTypeDefinitions InterfaceTypes { get; init; } = new();
    public UnionTypeDefinitions UnionTypes { get; init; } = new();
    public EnumTypeDefinitions EnumTypes { get; init; } = new();
    public InputObjectTypeDefinitions InputObjectTypes { get; init; } = new();

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
            AddObjectTypes(schema.ObjectTypes);            
            AddInterfaceTypes(schema.InterfaceTypes);
            AddUnionTypes(schema.UnionTypes);
            AddEnumTypes(schema.EnumTypes);
            AddInputObjectTypes(schema.InputObjectTypes);
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
        if (ScalarTypes.ContainsKey(node.Name))
            throw ValidationException.ScalarAlreadyDefined(node.Location, node.Name);

        ScalarTypes.Add(node.Name, new()
        {
            Description = node.Description,
            Name = node.Name,
            Directives = ToDirectives(node.Directives),
            Location = node.Location
        });
    }

    public void AddObjectTypes(IEnumerable<SyntaxObjectTypeDefinitionNode> nodes)
    {
        foreach (var node in nodes)
            AddObjectType(node);
    }

    public void AddObjectType(SyntaxObjectTypeDefinitionNode node)
    {
        if (ObjectTypes.ContainsKey(node.Name))
            throw ValidationException.ObjectAlreadyDefined(node.Location, node.Name);

        ObjectTypes.Add(node.Name, new()
        {
            Description = node.Description,
            Name = node.Name,
            ImplementsInterfaces = ToInterfaces(node.ImplementsInterfaces),
            Directives = ToDirectives(node.Directives),
            Fields = ToFieldDefinitions(node.Fields),
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
        if (InterfaceTypes.ContainsKey(node.Name))
            throw ValidationException.InterfaceAlreadyDefined(node.Location, node.Name);

        InterfaceTypes.Add(node.Name, new()
        {
            Description = node.Description,
            Name = node.Name,
            ImplementsInterfaces = ToInterfaces(node.ImplementsInterfaces),
            Directives = ToDirectives(node.Directives),
            Fields = ToFieldDefinitions(node.Fields),
            Location = node.Location
        });
    }

    public void AddUnionTypes(IEnumerable<SyntaxUnionTypeDefinitionNode> nodes)
    {
        foreach (var node in nodes)
            AddUnionType(node);
    }

    public void AddUnionType(SyntaxUnionTypeDefinitionNode node)
    {
        if (UnionTypes.ContainsKey(node.Name))
            throw ValidationException.UnionAlreadyDefined(node.Location, node.Name);

        UnionTypes.Add(node.Name, new()
        {
            Description = node.Description,
            Name = node.Name,
            Directives = ToDirectives(node.Directives),
            MemberTypes = ToMemberTypes(node.MemberTypes),
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
        if (EnumTypes.ContainsKey(node.Name))
            throw ValidationException.EnumAlreadyDefined(node.Location, node.Name);

        EnumTypes.Add(node.Name, new()
        {
            Description = node.Description,
            Name = node.Name,
            Directives = ToDirectives(node.Directives),
            EnumValues = ToEnumValues(node.EnumValues),
            Location = node.Location
        });
    }

    public void AddInputObjectTypes(IEnumerable<SyntaxInputObjectTypeDefinitionNode> nodes)
    {
        foreach (var node in nodes)
            AddInputObjectType(node);
    }

    public void AddInputObjectType(SyntaxInputObjectTypeDefinitionNode node)
    {
        if (InputObjectTypes.ContainsKey(node.Name))
            throw ValidationException.InputObjectAlreadyDefined(node.Location, node.Name);

        InputObjectTypes.Add(node.Name, new()
        {
            Description = node.Description,
            Name = node.Name,
            Directives = ToDirectives(node.Directives),
            InputFields = ToInputValueDefinitions(node.InputFields),
            Location = node.Location
        });
    }
}

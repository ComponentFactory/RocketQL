using RocketQL.Core.Nodes;
using RocketQL.Core.Serializers;
using System.Xml.Linq;

namespace RocketQL.Core.Base;

public partial class Schema
{
    public Schema()
    {
    }

    public SchemaDefinition Definition { get; protected set; } = new();
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
            foreach (var node in schema.Schemas)
                AddSchema(node);

            AddDirectives(schema.Directives);
            AddScalarTypes(schema.ScalarTypes);
            AddObjectTypes(schema.ObjectTypes);            
            AddInterfaceTypes(schema.InterfaceTypes);
            AddUnionTypes(schema.UnionTypes);
            AddEnumTypes(schema.EnumTypes);
            AddInputObjectTypes(schema.InputObjectTypes);
        }
    }

    public void AddSchema(SyntaxSchemaDefinitionNode node)
    {
        if (!Definition.IsDefault)
            throw ValidationException.SchemaDefinitionAlreadyDefined(node.Location);

        Definition = new()
        { 
            Description = node.Description,
            Directives = ToDirectives(node.Directives),
            OperationTypeDefinitions = ToOperationTypeDefinitions(node.OperationTypes),
            Location = node.Location
        };
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

    private static OperationTypeDefinitions ToOperationTypeDefinitions(SyntaxOperationTypeDefinitionNodeList operationTypes)
    {
        var nodes = new OperationTypeDefinitions();

        foreach (var operationType in operationTypes)
        {
            if (nodes.ContainsKey(operationType.Operation))
                throw ValidationException.OperationTypeAlreadyDefined(operationType.Location, operationType.Operation);

            nodes.Add(operationType.Operation, new()
            {
                Operation = operationType.Operation,
                NamedType = operationType.NamedType,
                Definition = null,
                Location = operationType.Location
            });
        }

        return nodes;
    }

    private static Directives ToDirectives(SyntaxDirectiveNodeList directives)
    {
        var nodes = new Directives();

        foreach (var directive in directives)
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

    private static Interfaces ToInterfaces(SyntaxNameList names)
    {
        var nodes = new Interfaces();

        foreach (var name in names)
        {
            nodes.Add(name, new()
            {
                Name = name,
                Definition = null,
            });
        }

        return nodes;
    }

    private static MemberTypes ToMemberTypes(SyntaxNameList names)
    {
        var nodes = new MemberTypes();

        foreach (var name in names)
        {
            nodes.Add(name, new()
            {
                Name = name,
                Definition = null,
            });
        }

        return nodes;
    }

    private static EnumValueDefinitions ToEnumValues(SyntaxEnumValueDefinitionList enumValues)
    {
        var nodes = new EnumValueDefinitions();

        foreach (var enumValue in enumValues)
        {
            nodes.Add(enumValue.Name, new()
            {
                Description = enumValue.Description,
                Name = enumValue.Name,
                Directives = ToDirectives(enumValue.Directives),
                Location = enumValue.Location
            });
        }

        return nodes;
    }

    private static ObjectFields ToObjectFieldNodes(SyntaxObjectFieldNodeList fields)
    {
        var nodes = new ObjectFields();

        foreach (var field in fields)
            nodes.Add(field.Name, field);

        return nodes;
    }

    private static FieldDefinitions ToFieldDefinitions(SyntaxFieldDefinitionNodeList fields)
    {
        var nodes = new FieldDefinitions();

        foreach (var field in fields)
        {
            nodes.Add(field.Name, new()
            {
                Description = field.Description,
                Name = field.Name,
                Arguments = ToInputValueDefinitions(field.Arguments),
                Type = ToTypeNode(field.Type),
                Definition = null
            });
        }

        return nodes;
    }

    private static InputValueDefinitions ToInputValueDefinitions(SyntaxInputValueDefinitionNodeList inputValues)
    {
        var nodes = new InputValueDefinitions();

        foreach (var inputValue in inputValues)
        {
            nodes.Add(inputValue.Name, new()
            {
                Description = inputValue.Description,
                Name = inputValue.Name,
                Type = ToTypeNode(inputValue.Type),
                DefaultValue = inputValue.DefaultValue,
                Directives = ToDirectives(inputValue.Directives),
                Location = inputValue.Location
            });
        }

        return nodes;
    }

    private static TypeLocation ToTypeNode(SyntaxTypeNode node)
    {
        return node switch
        {
            SyntaxTypeNameNode nameNode => new TypeName()
            {
                Name = nameNode.Name,
                NonNull = nameNode.NonNull,
                Location = nameNode.Location,
            },
            SyntaxTypeListNode listNode => new TypeList()
            {
                Type = ToTypeNode(listNode.Type),
                Definition = null,
                NonNull = listNode.NonNull,
                Location = listNode.Location,
            },
            _ => throw ValidationException.UnrecognizedType(node.Location, node.GetType().Name)
        }; ;
    }
}

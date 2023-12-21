﻿namespace RocketQL.Core.Base;

public class Schema
{
    private readonly SyntaxNodeList _syntaxNodes = [];

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

    public void Add(SyntaxNode node)
    {
        _syntaxNodes.Add(node); 
        IsValidated = false;
    }

    public void Add(IEnumerable<SyntaxNode> nodes)
    {
        _syntaxNodes.AddRange(nodes);
        IsValidated = false;
    }

    public void Validate()
    {
        Clean();

        try
        {
            ConvertToSchemaNodes();
            InterlinkSchemaNodes();
            //ValidateDirectives();
            //ValidateTypes();
            //ValidateSchema();
            IsValidated = true;
        }
        catch
        {
            Clean();
            throw;
        }
    }

    private void ConvertToSchemaNodes()
    {
        // Convert each syntax node into a schema node, checking only for duplicate name errors
        foreach (var node in _syntaxNodes)
        {
            switch (node)
            {
                case SyntaxSchemaDefinitionNode schemaDefinition:
                    ConvertSchemaDefinition(schemaDefinition);
                    break;
                case SyntaxDirectiveDefinitionNode directiveDefinition:
                    ConvertDirectiveDefinition(directiveDefinition);
                    break;
                case SyntaxScalarTypeDefinitionNode scalarType:
                    ConvertScalarType(scalarType);
                    break;
                case SyntaxObjectTypeDefinitionNode objectType:
                    ConvertObjectType(objectType);
                    break;
                case SyntaxInterfaceTypeDefinitionNode interfaceType:
                    ConvertInterfaceType(interfaceType);
                    break;
                case SyntaxUnionTypeDefinitionNode unionType:
                    ConvertUnionType(unionType);
                    break;
                case SyntaxEnumTypeDefinitionNode enumType:
                    ConvertEnumType(enumType);
                    break;
                case SyntaxInputObjectTypeDefinitionNode inputObjectType:
                    ConvertInputObjectType(inputObjectType);
                    break;
                default:
                    throw ValidationException.UnrecognizedType(node);
            }
        }
    }

    private void ConvertSchemaDefinition(SyntaxSchemaDefinitionNode schemaDefinition)
    {
        if (!Definition.IsDefault)
            throw ValidationException.SchemaDefinitionAlreadyDefined(schemaDefinition.Location);

        var operation = ToOperationTypeDefinitions(schemaDefinition.OperationTypes).Values;

        Definition = new()
        {
            Description = schemaDefinition.Description,
            Directives = ToDirectives(schemaDefinition.Directives),
            Query = operation.FirstOrDefault(o => o.Operation == OperationType.QUERY),
            Mutation = operation.FirstOrDefault(o => o.Operation == OperationType.MUTATION),
            Subscription = operation.FirstOrDefault(o => o.Operation == OperationType.SUBSCRIPTION),
            Location = schemaDefinition.Location
        };
    }

    private void ConvertDirectiveDefinition(SyntaxDirectiveDefinitionNode directiveDefinition)
    {
        if (Directives.ContainsKey(directiveDefinition.Name))
            throw ValidationException.DirectiveNameAlreadyDefined(directiveDefinition.Location, directiveDefinition.Name);

        Directives.Add(directiveDefinition.Name, new()
        {
            Description = directiveDefinition.Description,
            Name = directiveDefinition.Name,
            Arguments = ToInputValueDefinitions(directiveDefinition.Arguments),
            Repeatable = directiveDefinition.Repeatable,
            DirectiveLocations = directiveDefinition.DirectiveLocations,
            Location = directiveDefinition.Location
        });
    }

    private void ConvertScalarType(SyntaxScalarTypeDefinitionNode scalarType)
    {
        if (Types.ContainsKey(scalarType.Name))
            throw ValidationException.ScalarNameAlreadyDefined(scalarType.Location, scalarType.Name);

        Types.Add(scalarType.Name, new ScalarTypeDefinition()
        {
            Description = scalarType.Description,
            Name = scalarType.Name,
            Directives = ToDirectives(scalarType.Directives),
            Location = scalarType.Location
        });
    }

    private void ConvertObjectType(SyntaxObjectTypeDefinitionNode objectType)
    {
        if (Types.ContainsKey(objectType.Name))
            throw ValidationException.ObjectNameAlreadyDefined(objectType.Location, objectType.Name);

        Types.Add(objectType.Name, new ObjectTypeDefinition()
        {
            Description = objectType.Description,
            Name = objectType.Name,
            ImplementsInterfaces = ToInterfaces(objectType.ImplementsInterfaces),
            Directives = ToDirectives(objectType.Directives),
            Fields = ToFieldDefinitions(objectType.Fields),
            Location = objectType.Location
        });
    }


    private void ConvertInterfaceType(SyntaxInterfaceTypeDefinitionNode interfaceType)
    {
        if (Types.ContainsKey(interfaceType.Name))
            throw ValidationException.InterfaceNameAlreadyDefined(interfaceType.Location, interfaceType.Name);

        Types.Add(interfaceType.Name, new InterfaceTypeDefinition()
        {
            Description = interfaceType.Description,
            Name = interfaceType.Name,
            ImplementsInterfaces = ToInterfaces(interfaceType.ImplementsInterfaces),
            Directives = ToDirectives(interfaceType.Directives),
            Fields = ToFieldDefinitions(interfaceType.Fields),
            Location = interfaceType.Location
        });
    }

    private void ConvertUnionType(SyntaxUnionTypeDefinitionNode unionType)
    {
        if (Types.ContainsKey(unionType.Name))
            throw ValidationException.UnionNameAlreadyDefined(unionType.Location, unionType.Name);

        Types.Add(unionType.Name, new UnionTypeDefinition()
        {
            Description = unionType.Description,
            Name = unionType.Name,
            Directives = ToDirectives(unionType.Directives),
            MemberTypes = ToMemberTypes(unionType.MemberTypes),
            Location = unionType.Location
        });
    }

    private void ConvertEnumType(SyntaxEnumTypeDefinitionNode enumType)
    {
        if (Types.ContainsKey(enumType.Name))
            throw ValidationException.EnumNameAlreadyDefined(enumType.Location, enumType.Name);

        Types.Add(enumType.Name, new EnumTypeDefinition()
        {
            Description = enumType.Description,
            Name = enumType.Name,
            Directives = ToDirectives(enumType.Directives),
            EnumValues = ToEnumValues(enumType.EnumValues),
            Location = enumType.Location
        });
    }


    private void ConvertInputObjectType(SyntaxInputObjectTypeDefinitionNode inputObjectType)
    {
        if (Types.ContainsKey(inputObjectType.Name))
            throw ValidationException.InputObjectNameAlreadyDefined(inputObjectType.Location, inputObjectType.Name);

        Types.Add(inputObjectType.Name, new InputObjectTypeDefinition()
        {
            Description = inputObjectType.Description,
            Name = inputObjectType.Name,
            Directives = ToDirectives(inputObjectType.Directives),
            InputFields = ToInputValueDefinitions(inputObjectType.InputFields),
            Location = inputObjectType.Location
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
                Definition = null,
                NonNull = nameNode.NonNull,
                Location = nameNode.Location,
            },
            SyntaxTypeListNode listNode => new TypeList()
            {
                Type = ToTypeNode(listNode.Type),
                NonNull = listNode.NonNull,
                Location = listNode.Location,
            },
            _ => throw ValidationException.UnrecognizedType(node.Location, node.GetType().Name)
        }; ;
    }

    private static ObjectFields ToObjectFieldNodes(SyntaxObjectFieldNodeList fields)
    {
        var nodes = new ObjectFields();

        foreach (var field in fields)
            nodes.Add(field.Name, field);

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

    private void InterlinkSchemaNodes()
    {
        // directives
        // types
    }

    //private void ValidateSchema()
    //{
    //    // Schema definition can be omitted
    //    if (Definition.IsDefault)
    //    {
    //        // Look for types with names that match the operation, but the type cannot be referenced by any other types
    //        if (Types.TryGetValue("Query", out var queryTypeDefinition) && (queryTypeDefinition.UsedByTypes.Count == 0))
    //        {
    //            Definition.Query = new OperationTypeDefinition()
    //            {
    //                Operation = OperationType.QUERY,
    //                Definition = queryTypeDefinition,
    //                Location = queryTypeDefinition.Location,
    //            };
    //        }

    //        if (Types.TryGetValue("Mutation", out var mutationTypeDefinition) && (mutationTypeDefinition.UsedByTypes.Count == 0))
    //        {
    //            Definition.Mutation = new OperationTypeDefinition()
    //            {
    //                Operation = OperationType.MUTATION,
    //                Definition = mutationTypeDefinition,
    //                Location = mutationTypeDefinition.Location,
    //            };
    //        }

    //        if (Types.TryGetValue("Subscription", out var subscriptionTypeDefinition) && (subscriptionTypeDefinition.UsedByTypes.Count == 0))
    //        {
    //            Definition.Subscription = new OperationTypeDefinition()
    //            {
    //                Operation = OperationType.SUBSCRIPTION,
    //                Definition = subscriptionTypeDefinition,
    //                Location = subscriptionTypeDefinition.Location,
    //            };
    //        }

    //        // Schema must always define the Query root operation
    //        if (Definition.Query is null)
    //            throw ValidationException.SchemaDefinitionMissingQuery(Definition.Location);

    //        // Each operation must have a different type
    //        if (Definition.Mutation is not null)
    //        {
    //            if (Definition.Query.Definition == Definition.Mutation.Definition)
    //                throw ValidationException.SchemaOperationsNotUnique(Definition.Location, "Query", "Mutation", Definition.Query.Definition.Name);
    //        }

    //        if (Definition.Subscription is not null)
    //        {
    //            if (Definition.Query.Definition == Definition.Subscription.Definition)
    //                throw ValidationException.SchemaOperationsNotUnique(Definition.Location, "Query", "Subscription", Definition.Query.Definition.Name);

    //            if (Definition.Mutation is not null)
    //            {
    //                if (Definition.Mutation.Definition == Definition.Subscription.Definition)
    //                    throw ValidationException.SchemaOperationsNotUnique(Definition.Location, "Mutation", "Subscription", Definition.Mutation.Definition.Name);
    //            }
    //        }
    //    }
    //}


    //private int ValidateNodes(SyntaxNodeList nodes, bool errors = false)
    //{
    //    int processed = 0;

    //    for(int i=0; i<nodes.Count; i++)
    //    {
    //        if (nodes[i] switch
    //        {
    //            SyntaxSchemaDefinitionNode schemaNode => Validate(schemaNode, errors),
    //            _ => throw ValidationException.UnrecognizedType(nodes[i].Location, nodes[i].GetType().Name)
    //        })
    //        {
    //            nodes.RemoveAt(i--);
    //            processed++;
    //        }
    //    }

    //    return processed;
    //}

    //private bool Validate(SyntaxSchemaDefinitionNode schemaNode, bool errors = false) 
    //{ 
    //    // Only allowed a single schema definition
    //    if (!Definition.IsDefault)
    //        throw ValidationException.SchemaDefinitionAlreadyDefined(schemaNode.Location);

    //    // Cannot have an empty schema definition
    //    if (schemaNode.OperationTypes.Count == 0)
    //        throw ValidationException.SchemaDefinitionEmpty(schemaNode.Location);

    //    // Resolve the operations
    //    Dictionary<OperationType, OperationTypeDefinition> operations = [];
    //    foreach (var operation in schemaNode.OperationTypes)
    //    {
    //        // Cannot define the same operation twice
    //        if (operations.ContainsKey(operation.Operation))
    //            throw ValidationException.SchemaOperationAlreadyDefined(operation.Location, operation.Operation);

    //        // The object type might not have been defined yet
    //        if (!ObjectTypes.TryGetValue(operation.NamedType, out var typeDefinition))
    //        {
    //            if (errors)
    //                _validationErrors.Add(ValidationException.TypeNotDefinedForSchemaOperation(operation.Location, operation.Operation, operation.NamedType));

    //            return false;
    //        }
    //    }

    //    // Must define the query operation
    //    if (!operations.TryGetValue(OperationType.QUERY, out var queryType))
    //        throw ValidationException.SchemaDefinitionMissingQuery(schemaNode.Location);

    //    // Other operations are optional
    //    operations.TryGetValue(OperationType.MUTATION, out var mutatationType);
    //    operations.TryGetValue(OperationType.SUBSCRIPTION, out var subscriptionType);

    //    // Resolve all the directives
    //    // TODO

    //    Definition = new()
    //    {
    //        Description = schemaNode.Description,
    //        Directives = [], // TODO
    //        Query = queryType,
    //        Mutation = mutatationType,
    //        Subscription = subscriptionType,
    //        Location = schemaNode.Location,
    //    };

    //    return true;
    //}

    private void Clean()
    {
        Definition = new();
        Types.Clear();
        IsValidated = false;
    }
}
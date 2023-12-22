using RocketQL.Core.Nodes;

namespace RocketQL.Core.Base;

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
            ConvertToNodes();
            InterlinkDirectives();
            InterlinkTypes();
            InterlinkSchema();
            ValidateDirectives();
            ValidateTypes();
            ValidateSchema();
            IsValidated = true;
        }
        catch
        {
            Clean();
            throw;
        }
    }

    private void ConvertToNodes()
    {
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

        var operation = ConvertOperationType(schemaDefinition.OperationTypes).Values;

        Definition = new()
        {
            Description = schemaDefinition.Description,
            Directives = ConvertDirectives(schemaDefinition.Directives),
            Query = operation.FirstOrDefault(o => o.Operation == OperationType.QUERY),
            Mutation = operation.FirstOrDefault(o => o.Operation == OperationType.MUTATION),
            Subscription = operation.FirstOrDefault(o => o.Operation == OperationType.SUBSCRIPTION),
            Location = schemaDefinition.Location
        };
    }

    private void ConvertDirectiveDefinition(SyntaxDirectiveDefinitionNode directiveDefinition)
    {
        if (Directives.ContainsKey(directiveDefinition.Name))
            throw ValidationException.NameAlreadyDefined(directiveDefinition.Location, "Directive", directiveDefinition.Name);

        Directives.Add(directiveDefinition.Name, new()
        {
            Description = directiveDefinition.Description,
            Name = directiveDefinition.Name,
            Arguments = ConvertInputValues(directiveDefinition.Arguments, "Directive", directiveDefinition.Name, "Argument"),
            Repeatable = directiveDefinition.Repeatable,
            DirectiveLocations = directiveDefinition.DirectiveLocations,
            Location = directiveDefinition.Location
        });
    }

    private void ConvertScalarType(SyntaxScalarTypeDefinitionNode scalarType)
    {
        if (Types.ContainsKey(scalarType.Name))
            throw ValidationException.NameAlreadyDefined(scalarType.Location, "Scalar", scalarType.Name);

        Types.Add(scalarType.Name, new ScalarTypeDefinition()
        {
            Description = scalarType.Description,
            Name = scalarType.Name,
            Directives = ConvertDirectives(scalarType.Directives),
            Location = scalarType.Location
        });
    }

    private void ConvertObjectType(SyntaxObjectTypeDefinitionNode objectType)
    {
        if (Types.ContainsKey(objectType.Name))
            throw ValidationException.NameAlreadyDefined(objectType.Location, "Object", objectType.Name);

        Types.Add(objectType.Name, new ObjectTypeDefinition()
        {
            Description = objectType.Description,
            Name = objectType.Name,
            ImplementsInterfaces = ConvertInterfaces(objectType.ImplementsInterfaces),
            Directives = ConvertDirectives(objectType.Directives),
            Fields = ConvertFieldDefinitions(objectType.Fields),
            Location = objectType.Location
        });
    }

    private void ConvertInterfaceType(SyntaxInterfaceTypeDefinitionNode interfaceType)
    {
        if (Types.ContainsKey(interfaceType.Name))
            throw ValidationException.NameAlreadyDefined(interfaceType.Location, "Interface", interfaceType.Name);

        Types.Add(interfaceType.Name, new InterfaceTypeDefinition()
        {
            Description = interfaceType.Description,
            Name = interfaceType.Name,
            ImplementsInterfaces = ConvertInterfaces(interfaceType.ImplementsInterfaces),
            Directives = ConvertDirectives(interfaceType.Directives),
            Fields = ConvertFieldDefinitions(interfaceType.Fields),
            Location = interfaceType.Location
        });
    }

    private void ConvertUnionType(SyntaxUnionTypeDefinitionNode unionType)
    {
        if (Types.ContainsKey(unionType.Name))
            throw ValidationException.NameAlreadyDefined(unionType.Location, "Union", unionType.Name);

        Types.Add(unionType.Name, new UnionTypeDefinition()
        {
            Description = unionType.Description,
            Name = unionType.Name,
            Directives = ConvertDirectives(unionType.Directives),
            MemberTypes = ConvertMemberTypes(unionType.MemberTypes),
            Location = unionType.Location
        });
    }

    private void ConvertEnumType(SyntaxEnumTypeDefinitionNode enumType)
    {
        if (Types.ContainsKey(enumType.Name))
            throw ValidationException.NameAlreadyDefined(enumType.Location, "Enum", enumType.Name);

        Types.Add(enumType.Name, new EnumTypeDefinition()
        {
            Description = enumType.Description,
            Name = enumType.Name,
            Directives = ConvertDirectives(enumType.Directives),
            EnumValues = ConvertEnumValues(enumType.EnumValues),
            Location = enumType.Location
        });
    }

    private void ConvertInputObjectType(SyntaxInputObjectTypeDefinitionNode inputObjectType)
    {
        if (Types.ContainsKey(inputObjectType.Name))
            throw ValidationException.NameAlreadyDefined(inputObjectType.Location, "Input object", inputObjectType.Name);

        Types.Add(inputObjectType.Name, new InputObjectTypeDefinition()
        {
            Description = inputObjectType.Description,
            Name = inputObjectType.Name,
            Directives = ConvertDirectives(inputObjectType.Directives),
            InputFields = ConvertInputValues(inputObjectType.InputFields, "Field", inputObjectType.Name, "Argument"),
            Location = inputObjectType.Location
        });
    }

    private static OperationTypeDefinitions ConvertOperationType(SyntaxOperationTypeDefinitionNodeList operationTypes)
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

    private static Directives ConvertDirectives(SyntaxDirectiveNodeList directives)
    {
        var nodes = new Directives();

        foreach (var directive in directives)
        {
            nodes.Add(directive.Name, new()
            {
                Name = directive.Name,
                Definition = null,
                Arguments = ConvertObjectFields(directive.Arguments),
                Location = directive.Location
            });
        }

        return nodes;
    }

    private static InputValueDefinitions ConvertInputValues(SyntaxInputValueDefinitionNodeList inputValues, string parentNode, string parentName, string listType)
    {
        var nodes = new InputValueDefinitions();

        foreach (var inputValue in inputValues)
        {
            if (inputValue.Name.StartsWith("__"))
                throw ValidationException.ListEntryDoubleUnderscore(inputValue.Location, parentNode, parentName, listType.ToLower(), inputValue.Name);

            if (nodes.ContainsKey(inputValue.Name))
                throw ValidationException.ListEntryDuplicateName(inputValue.Location, parentNode, parentName, listType.ToLower(), inputValue.Name);

            nodes.Add(inputValue.Name, new()
            {
                Description = inputValue.Description,
                Name = inputValue.Name,
                Type = ConvertTypeNode(inputValue.Type),
                DefaultValue = inputValue.DefaultValue,
                Directives = ConvertDirectives(inputValue.Directives),
                Location = inputValue.Location
            });
        }

        return nodes;
    }

    private static TypeNode ConvertTypeNode(SyntaxTypeNode node)
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
                Type = ConvertTypeNode(listNode.Type),
                NonNull = listNode.NonNull,
                Location = listNode.Location,
            },
            _ => throw ValidationException.UnrecognizedType(node.Location, node.GetType().Name)
        }; ;
    }

    private static ObjectFields ConvertObjectFields(SyntaxObjectFieldNodeList fields)
    {
        var nodes = new ObjectFields();

        foreach (var field in fields)
            nodes.Add(field.Name, field);

        return nodes;
    }

    private static Interfaces ConvertInterfaces(SyntaxNameList names)
    {
        var nodes = new Interfaces();

        foreach (var name in names)
            nodes.Add(name, new()
            {
                Name = name,
                Definition = null,
            });

        return nodes;
    }

    private static MemberTypes ConvertMemberTypes(SyntaxNameList names)
    {
        var nodes = new MemberTypes();

        foreach (var name in names)
            nodes.Add(name, new()
            {
                Name = name,
                Definition = null,
            });

        return nodes;
    }

    private static EnumValueDefinitions ConvertEnumValues(SyntaxEnumValueDefinitionList enumValues)
    {
        var nodes = new EnumValueDefinitions();

        foreach (var enumValue in enumValues)
            nodes.Add(enumValue.Name, new()
            {
                Description = enumValue.Description,
                Name = enumValue.Name,
                Directives = ConvertDirectives(enumValue.Directives),
                Location = enumValue.Location
            });

        return nodes;
    }

    private static FieldDefinitions ConvertFieldDefinitions(SyntaxFieldDefinitionNodeList fields)
    {
        var nodes = new FieldDefinitions();

        foreach (var field in fields)
            nodes.Add(field.Name, new()
            {
                Description = field.Description,
                Name = field.Name,
                Arguments = ConvertInputValues(field.Arguments, "Argument", "Field", field.Name),
                Type = ConvertTypeNode(field.Type),
                Definition = null
            });

        return nodes;
    }

    private void InterlinkDirectives()
    {
        foreach(var directiveDefinition in Directives.Values)
        {
            foreach (var argumentDefinition in directiveDefinition.Arguments.Values)
            {
                InterlinkDirectives(argumentDefinition.Directives, argumentDefinition, directiveDefinition);
                InterlinkTypeNode(argumentDefinition.Type, argumentDefinition, directiveDefinition);
            }
        }
    }

    private void InterlinkTypes()
    {
        foreach (var type in Types.Values)
        {
            switch (type)
            {
                case ScalarTypeDefinition scalarType:
                    InterlinkDirectives(scalarType.Directives, scalarType);
                    break;
                case ObjectTypeDefinition objectType:
                    // TODO
                    break;
                case InterfaceTypeDefinition interfaceType:
                    // TODO
                    break;
                case UnionTypeDefinition unionType:
                    // TODO
                    break;
                case EnumTypeDefinition enumType:
                    InterlinkDirectives(enumType.Directives, enumType);
                    break;
                case InputObjectTypeDefinition inputObjectType:
                    // TODO
                    break;
                default:
                    throw ValidationException.UnrecognizedType(type);
            }
        }
    }

    private void InterlinkDirectives(Directives directives, SchemaNode parentNode, SchemaNode? grandParentNode = null)
    {
        foreach (var directive in directives.Values)
        {
            if (!Directives.TryGetValue(directive.Name, out DirectiveDefinition? directiveDefinition))
            {
                if (grandParentNode is null)
                    throw ValidationException.UndefinedDirective(directive, parentNode);
                else
                    throw ValidationException.UndefinedDirective(directive, parentNode, grandParentNode);
            }

            directive.Definition = directiveDefinition;
        }
    }

    private void InterlinkSchema()
    {
    }

    private void InterlinkTypeNode(TypeNode typeLocation, SchemaNode listNode, SchemaNode parentNode)
    {
        if (typeLocation is TypeList typeList)
            InterlinkTypeNode(typeList.Type, listNode, parentNode);
        else if (typeLocation is TypeName typeName)
        {
            if (Types.TryGetValue(typeName.Name, out var type))
                typeName.Definition = type;
            else
                throw ValidationException.UndefinedTypeForListEntry(typeName.Location, typeName.Name, listNode, parentNode);
        }
    }

    private void ValidateDirectives()
    {
        foreach (var directiveDefinition in Directives.Values)
        {
            if (directiveDefinition.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(directiveDefinition);
        }
    }

    private void ValidateTypes()
    {
        foreach (var type in Types.Values)
        {
            switch (type)
            {
                case ScalarTypeDefinition scalarType:
                    ValidateScalarType(scalarType);
                    break;
                case ObjectTypeDefinition objectType:
                    // TODO
                    break;
                case InterfaceTypeDefinition interfaceType:
                    // TODO
                    break;
                case UnionTypeDefinition unionType:
                    // TODO
                    break;
                case EnumTypeDefinition enumType:
                    ValidateEnumType(enumType);
                    break;
                case InputObjectTypeDefinition inputObjectType:
                    // TODO
                    break;
                default:
                    throw ValidationException.UnrecognizedType(type);
            }
        }
    }

    private void ValidateEnumType(EnumTypeDefinition enumType)
    {
        if (enumType.Name.StartsWith("__"))
            throw ValidationException.NameDoubleUnderscore(enumType);
    }

    private void ValidateScalarType(ScalarTypeDefinition scalarType)
    {
        if (scalarType.Name.StartsWith("__"))
            throw ValidationException.NameDoubleUnderscore(scalarType);
    }

    private void ValidateSchema()
    {
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
    }


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

using RocketQL.Core.Nodes;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private readonly SyntaxNodeList _syntaxNodes = [];

    public SchemaDefinitions Schemas { get; init; } = [];
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
            AddPredefinedTypes();
            new SchemaConvert(this).Visit();
            new SchemaLink(this).Visit();
            new SchemaValidate(this).Visit();
            IsValidated = true;
        }
        catch
        {
            Clean();
            throw;
        }
    }

    public void AddPredefinedTypes()
    {
        foreach(string scalar in new string[] { "Int", "Float", "String", "Boolean", "ID" })
            Types.Add(scalar, new ScalarTypeDefinition()
            {
                Description = string.Empty,
                Name = scalar,
                Directives = [],
                Location = new()
            });
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
        Schemas.Clear();
        Directives.Clear();
        Types.Clear();
        IsValidated = false;
    }
}

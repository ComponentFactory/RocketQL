namespace RocketQL.Core.Base;

public partial class Schema
{
    private readonly SyntaxNodeList _nodes = [];

    public SchemaRoot? Root { get; set; }
    private SchemaDefinitions Schemas { get; init; } = [];
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

    public void Add(SyntaxNode node)
    {
        _nodes.Add(node);
        IsValidated = false;
    }

    public void Add(IEnumerable<SyntaxNode> nodes)
    {
        _nodes.AddRange(nodes);
        IsValidated = false;
    }

    public void Add(SyntaxNodeList nodes)
    {
        _nodes.AddRange(nodes);
        IsValidated = false;
    }

    public void Add(IEnumerable<SyntaxNodeList> schemas)
    {
        foreach (var nodes in schemas)
            _nodes.AddRange(nodes);

        IsValidated = false;
    }

    public void Validate()
    {
        Clean();

        try
        {
            AddPredefinedDirectives();
            AddPredefinedScalars();
            Converter.Visit();
            Linker.Visit();
            Validater.Visit();
            ValidateSchema();
            IsValidated = true;
        }
        catch
        {
            Clean();
            throw;
        }
    }

    private void ValidateSchema()
    {
        if (Schemas.Count == 1)
        {
            Root = new SchemaRoot()
            {
                Description = Schemas[0].Description,
                Directives = Schemas[0].Directives,
                Query = Schemas[0].Operations.Where(o => o.Key == OperationType.QUERY).Select(o => o.Value).FirstOrDefault(),
                Mutation = Schemas[0].Operations.Where(o => o.Key == OperationType.MUTATION).Select(o => o.Value).FirstOrDefault(),
                Subscription = Schemas[0].Operations.Where(o => o.Key == OperationType.SUBSCRIPTION).Select(o => o.Value).FirstOrDefault(),
                Location = Schemas[0].Location,
            };
        }
        else
        {
            Types.TryGetValue("Query", out var queryTypeDefinition);
            Types.TryGetValue("Mutation", out var mutationTypeDefinition);
            Types.TryGetValue("Subscription", out var subscriptionTypeDefinition);

            if (queryTypeDefinition is null)
                throw ValidationException.AutoSchemaQueryMissing();

            if (queryTypeDefinition is not ObjectTypeDefinition)
                throw ValidationException.AutoSchemaOperationNotObject(queryTypeDefinition, "Query");

            if (!AllReferencesWithinType(queryTypeDefinition))
                throw ValidationException.AutoSchemaOperationReferenced(queryTypeDefinition, "Query");

            if (mutationTypeDefinition is not null)
            {
                if (mutationTypeDefinition is not ObjectTypeDefinition)
                    throw ValidationException.AutoSchemaOperationNotObject(mutationTypeDefinition, "Mutation");

                if (!AllReferencesWithinType(mutationTypeDefinition))
                    throw ValidationException.AutoSchemaOperationReferenced(mutationTypeDefinition, "Mutation");
            }

            if (subscriptionTypeDefinition is not null)
            {
                if (subscriptionTypeDefinition is not ObjectTypeDefinition)
                    throw ValidationException.AutoSchemaOperationNotObject(subscriptionTypeDefinition, "Subscription");

                if (!AllReferencesWithinType(subscriptionTypeDefinition))
                    throw ValidationException.AutoSchemaOperationReferenced(subscriptionTypeDefinition, "Subscription");
            }

            Root = new SchemaRoot()
            {
                Description = string.Empty,
                Directives = [],
                Query = OperationTypeFromObjectType(queryTypeDefinition, OperationType.QUERY),
                Mutation = OperationTypeFromObjectType(mutationTypeDefinition, OperationType.MUTATION),
                Subscription = OperationTypeFromObjectType(subscriptionTypeDefinition, OperationType.SUBSCRIPTION),
                Location = queryTypeDefinition.Location,
            };
        }
    }

    public void Reset()
    {
        _nodes.Clear();
        Clean();
    }

    private void Clean()
    {
        Root = null;
        Schemas.Clear();
        Directives.Clear();
        Types.Clear();
        IsValidated = false;
    }

    private void AddPredefinedDirectives()
    {
        Directives.Add("deprecated", new DirectiveDefinition()
        {
            Description = string.Empty,
            Name = "deprecated",
            Repeatable = false,
            Arguments = new()
            {
                { "reason", new InputValueDefinition()
                            {
                                Description = string.Empty,
                                Name = "reason",
                                Type = new TypeName()
                                {
                                    Name = "String",
                                    Definition = null,
                                    Location = new()
                                },
                                DefaultValue = new StringValueNode("No longer supported"),
                                Directives = [],
                                Location = new(),
                                ElementUsage = "Argument",
                            }
                }
            },
            DirectiveLocations = DirectiveLocations.FIELD_DEFINITION |
                                 DirectiveLocations.ARGUMENT_DEFINITION |
                                 DirectiveLocations.INPUT_FIELD_DEFINITION |
                                 DirectiveLocations.ENUM_VALUE,
            Location = new(),
            IsPredefined = true
        });

        Directives.Add("specifiedBy", new DirectiveDefinition()
        {
            Description = string.Empty,
            Name = "specifiedBy",
            Repeatable = false,
            Arguments = new()
            {
                { "url", new InputValueDefinition()
                         {
                             Description = string.Empty,
                             Name = "url",
                             Type = new TypeNonNull()
                             {
                                 Type =    new TypeName()                          
                                 {
                                     Name = "String",
                                     Definition = null,
                                     Location = new()
                                 },
                                 Location = new()
                             },
                             DefaultValue = null,
                             Directives = [],
                             Location = new(),
                             ElementUsage = "Argument",
                         }
                }
            },
            DirectiveLocations = DirectiveLocations.SCALAR,
            Location = new(),
            IsPredefined = true
        });
    }

    private void AddPredefinedScalars()
    {
        foreach (string scalar in new string[] { "Int", "Float", "String", "Boolean", "ID" })
            Types.Add(scalar, new ScalarTypeDefinition()
            {
                Description = string.Empty,
                Name = scalar,
                Directives = [],
                Location = new(),
                IsPredefined = true
            });
    }

    private static bool AllReferencesWithinType(TypeDefinition root)
    {
        foreach (var reference in root.References)
        {
            var checkReference = reference;
            while (checkReference.Parent != null)
                checkReference = checkReference.Parent;

            if (checkReference != root)
                return false;
        }

        return true;
    }

    private static OperationTypeDefinition? OperationTypeFromObjectType(TypeDefinition? typeDefinition, OperationType operationType)
    {
        if (typeDefinition is null)
            return null;

        return new OperationTypeDefinition()
        {
            Definition = typeDefinition,
            Operation = operationType,
            NamedType = typeDefinition.Name,
            Location = typeDefinition.Location,
        };
    }
}

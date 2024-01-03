using RocketQL.Core.Nodes;
using System.Xml;

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
            AddBuiltInDirectives();
            AddBuiltInScalars();
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

    private void AddBuiltInDirectives()
    {
        Directives.Add("include", new DirectiveDefinition()
        {
            Description = "Directs the executor to include this field or fragment only when the `if` argument is true",
            Name = "include",
            Repeatable = false,
            Arguments = new()
            {
                { "if", new InputValueDefinition()
                        {
                            Description = "Included when true.",
                            Name = "if",
                            Type = new TypeNonNull()
                            {
                                Type = new TypeName()
                                {
                                    Name = "Boolean",
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
            DirectiveLocations = DirectiveLocations.FIELD |
                                 DirectiveLocations.FRAGMENT_SPREAD |
                                 DirectiveLocations.INLINE_FRAGMENT,
            Location = new(),
            IsBuiltIn = true
        });

        Directives.Add("skip", new DirectiveDefinition()
        {
            Description = "Directs the executor to skip this field or fragment when the `if` argument is true.",
            Name = "skip",
            Repeatable = false,
            Arguments = new()
            {
                { "if", new InputValueDefinition()
                        {
                            Description = "Skipped when true.",
                            Name = "if",
                            Type = new TypeNonNull()
                            {
                                Type = new TypeName()
                                {
                                    Name = "Boolean",
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
            DirectiveLocations = DirectiveLocations.FIELD |
                                 DirectiveLocations.FRAGMENT_SPREAD |
                                 DirectiveLocations.INLINE_FRAGMENT,
            Location = new(),
            IsBuiltIn = true
        });

        Directives.Add("deprecated", new DirectiveDefinition()
        {
            Description = "Marks the field, argument, input field or enum value as deprecated.",
            Name = "deprecated",
            Repeatable = false,
            Arguments = new()
            {
                { "reason", new InputValueDefinition()
                            {
                                Description = "The reason for the deprecation",
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
            IsBuiltIn = true
        });

        Directives.Add("specifiedBy", new DirectiveDefinition()
        {
            Description = "Exposes a URL that specifies the behaviour of this scalar.",
            Name = "specifiedBy",
            Repeatable = false,
            Arguments = new()
            {
                { "url", new InputValueDefinition()
                         {
                             Description =   "The URL that specifies the behaviour of this scalar.",
                             Name = "url",
                             Type = new TypeNonNull()
                             {
                                 Type = new TypeName()                          
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
            IsBuiltIn = true
        });
    }

    private void AddBuiltInScalars()
    {
        foreach (var scalarPair in new[] { 
            ("Int",     """
                        The `Int` scalar type represents non-fractional signed whole numeric values. 
                        Int can represent values between -(2^31) and 2^31 - 1.
                        """), 
            ("Float",   """
                        The `Float` scalar type represents signed double-precision fractional values as specified 
                        by [IEEE 754](http://en.wikipedia.org/wiki/IEEE_floating_point).
                        """), 
            ("String",  """
                        The `String` scalar type represents textual data, represented as UTF-8 character sequences. 
                        The String type is most often used by GraphQL to represent free-form human-readable text.
                        """), 
            ("Boolean", """
                        The `Boolean` scalar type represents `true` or `false`.
                        """), 
            ("ID",      """
                        The `ID` scalar type represents a unique identifier, often used to refetch an object or as 
                        key for a cache. The ID type appears in a JSON response as a String; however, it is not 
                        intended to be human-readable. When expected as an input type, any string (such as `"4"`) 
                        or integer (such as `4`) input value will be accepted as an ID.
                        """) 
        })
        {
            Types.Add(scalarPair.Item1, new ScalarTypeDefinition()
            {
                Description = scalarPair.Item2,
                Name = scalarPair.Item1,
                Directives = [],
                Location = new(),
                IsBuiltIn = true
            });
        }
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

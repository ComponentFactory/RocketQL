using RocketQL.Core.Nodes;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private SyntaxNodeList Nodes { get; init; } = [];
    private List<ValidationException> Exceptions { get; init; } = [];
    private SchemaDefinitions Schemas { get; init; } = [];

    public SchemaRoot? Root { get; set; }
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
        Nodes.Add(node);
        IsValidated = false;
    }

    public void Add(IEnumerable<SyntaxNode> nodes)
    {
        Nodes.AddRange(nodes);
        IsValidated = false;
    }

    public void Add(SyntaxNodeList nodes)
    {
        Nodes.AddRange(nodes);
        IsValidated = false;
    }

    public void Add(IEnumerable<SyntaxNodeList> schemas)
    {
        foreach (var nodes in schemas)
            Nodes.AddRange(nodes);

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
            Rooted.Visit();
            CheckExceptions();
            IsValidated = true;
        }
        catch
        {
            Clean();
            throw;
        }
    }

    public void Reset()
    {
        Nodes.Clear();
        Clean();
    }

    private void Clean()
    {
        Exceptions.Clear();
        Root = null;
        Schemas.Clear();
        Directives.Clear();
        Types.Clear();
        IsValidated = false;
    }

    public bool IsInputTypeCompatibleWithValue(TypeNode typeNode, ValueNode valueNode)
    {
        return typeNode switch
        {
            TypeName typeNameNode => IsInputTypeCompatibleWithTypeName(typeNameNode, valueNode),
            TypeList typeListNode => IsInputTypeCompatibleWithTypeList(typeListNode, valueNode),
            TypeNonNull typeNonNullNode => IsInputValueCompatibleWithTypeNonNull(typeNonNullNode, valueNode),
            _ => false
        };
    }

    private bool IsInputTypeCompatibleWithTypeName(TypeName typeNameNode, ValueNode valueNode)
    {
        if (Types.TryGetValue(typeNameNode.Name, out var typeDefinition))
        {
            return typeDefinition switch
            {
                ScalarTypeDefinition scalarTypeDefinition => IsInputTypeCompatibleWithScalarType(typeNameNode, valueNode, scalarTypeDefinition),
                EnumTypeDefinition enumTypeDefinition => IsInputTypeCompatibleWithEnumType(typeNameNode, valueNode, enumTypeDefinition),
                InputObjectTypeDefinition inputObjectTypeDefinition => IsInputTypeCompatibleWithInputObjectType(typeNameNode, valueNode, inputObjectTypeDefinition),
                _ => false
            };
        }

        return false;
    }

    private static bool IsInputTypeCompatibleWithScalarType(TypeName typeNameNode, ValueNode valueNode, ScalarTypeDefinition scalarTypeDefinition)
    {
        // You can assign 'null' to a scalar
        if (valueNode is NullValueNode)
            return true;

        return scalarTypeDefinition.Name switch
        {
            "Int" => valueNode is IntValueNode,
            "Float" => valueNode is FloatValueNode,
            "Boolean" => valueNode is BooleanValueNode,
            _ => valueNode is StringValueNode,
        };
    }

    private static bool IsInputTypeCompatibleWithEnumType(TypeName typeNameNode, ValueNode valueNode, EnumTypeDefinition enumTypeDefinition)
    {
        // You can assign 'null' to an enum
        if (valueNode is NullValueNode)
            return true;

        return valueNode switch 
        { 
            StringValueNode stringValue => enumTypeDefinition.EnumValues.ContainsKey(stringValue.Value),
            EnumValueNode enumValue => enumTypeDefinition.EnumValues.ContainsKey(enumValue.Value),
            _ => false
        };
    }

    private bool IsInputTypeCompatibleWithInputObjectType(TypeName typeNameNode, ValueNode valueNode, InputObjectTypeDefinition inputObjectTypeDefinition)
    {
        // You can assign 'null' to an input object
        if (valueNode is NullValueNode)
            return true;

        // You must assign an object to an input type
        if (valueNode is not ObjectValueNode objectValueNode)
            return false;

        var objectNodes = objectValueNode.ObjectFields.ToDictionary(o => o.Name, o => o.Value);
        foreach (var inputFieldDefinition in inputObjectTypeDefinition.InputFields.Values)
        {
            if (!objectNodes.TryGetValue(inputFieldDefinition.Name, out var objectNode))
            {
                if (inputFieldDefinition.Type is TypeNonNull)
                    return false;
            }
            else if (!IsInputTypeCompatibleWithValue(inputFieldDefinition.Type, objectNode))
                return false;

            objectNodes.Remove(inputFieldDefinition.Name);
        }

        return (objectNodes.Count == 0);
    }

    private bool IsInputTypeCompatibleWithTypeList(TypeList typeListNode, ValueNode valueNode)
    {
        if (valueNode is NullValueNode)
            return true;

        if (valueNode is not ListValueNode listValueNode)
            return false;

        foreach (var entryValueNode in listValueNode.Values)
            if (!IsInputTypeCompatibleWithValue(typeListNode.Type, entryValueNode))
                return false;

        return true;
    }

    private bool IsInputValueCompatibleWithTypeNonNull(TypeNonNull typeNonNullNode, ValueNode valueNode)
    {
        if (valueNode is NullValueNode)
            return false;

        return IsInputTypeCompatibleWithValue(typeNonNullNode.Type, valueNode);
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

    private static void FatalException(ValidationException validationException)
    {
        throw validationException;
    }

    private void NonFatalException(ValidationException validationException)
    {
        Exceptions.Add(validationException);
    }

    private void CheckExceptions()
    {
        if (Exceptions.Count == 1)
            throw Exceptions[0];
        else if (Exceptions.Count > 1)
            throw new ValidationExceptions(Exceptions);
    }
}

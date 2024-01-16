using System.Collections.Generic;

namespace RocketQL.Core.Base;

public partial class Schema : ISchema
{
    private SchemaRoot _root = SchemaRoot.Empty;
    private readonly SchemaDefinitions _schemas = [];
    private readonly DirectiveDefinitions _directives = [];
    private readonly TypeDefinitions _types = [];
    private readonly List<ValidationException> _exceptions = [];
    private readonly SyntaxNodeList _nodes = [];

    public SchemaRoot Root { get; protected set; } = SchemaRoot.Empty;
    public IReadOnlyDictionary<string, DirectiveDefinition> Directives { get; protected set; } = DirectiveDefinitions.Empty;
    public IReadOnlyDictionary<string, TypeDefinition> Types { get; protected set; } = TypeDefinitions.Empty;
    public bool IsValidated { get; protected set; } = false;

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

    public void Add(ReadOnlySpan<char> schema, string source)
    {
        Add(Serialization.SchemaDeserialize(schema, source));
    }

    public void Add(
        ReadOnlySpan<char> schema,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        Add(Serialization.SchemaDeserialize(schema, CallerExtensions.CallerToSource(filePath, memberName, lineNumber)));
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

            Root = _root;
            Directives = _directives;
            Types = _types;

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
        _nodes.Clear();
        Clean();
    }

    private void Clean()
    {
        _root = SchemaRoot.Empty;
        _schemas.Clear();
        _directives.Clear();
        _types.Clear();
        _exceptions.Clear();

        Root = SchemaRoot.Empty;
        Directives = DirectiveDefinitions.Empty;
        Types = TypeDefinitions.Empty;

        IsValidated = false;
    }

    private void AddBuiltInDirectives()
    {
        List<DirectiveDefinition> directives = [];

        directives.Add(new DirectiveDefinition(
            "Directs the executor to include this field or fragment only when the `if` argument is true",
            "@include",
            new()
            {
                { "if", new InputValueDefinition(
                    "Included when true.",
                    "if",
                    new TypeNonNull(new TypeName("Boolean", Location.Empty), Location.Empty),
                    null,
                    [],
                    Location.Empty,
                    "Argument")
                }
            },
            false,
            DirectiveLocations.FIELD | DirectiveLocations.FRAGMENT_SPREAD | DirectiveLocations.INLINE_FRAGMENT,
            Location.Empty));

        directives.Add(new DirectiveDefinition(
            "Directs the executor to skip this field or fragment when the `if` argument is true.",
            "@skip",
            new()
            {
                { "if", new InputValueDefinition(
                    "Skipped when true.",
                    "if",
                    new TypeNonNull(new TypeName("Boolean", Location.Empty), Location.Empty),
                    null,
                    [],
                    Location.Empty,
                    "Argument")
                }
            },
            false,
            DirectiveLocations.FIELD | DirectiveLocations.FRAGMENT_SPREAD | DirectiveLocations.INLINE_FRAGMENT,
            Location.Empty));

        directives.Add(new DirectiveDefinition(
            "Marks the field, argument, input field or enum value as deprecated.",
            "@deprecated",
            new()
            {
                { "reason", new InputValueDefinition(
                    "The reason for the deprecation",
                    "reason",
                    new TypeName("String", Location.Empty),
                    new StringValueNode("No longer supported"),
                    [],
                    Location.Empty,
                    "Argument")
                }
            },
            false,
            DirectiveLocations.FIELD_DEFINITION | DirectiveLocations.ARGUMENT_DEFINITION | DirectiveLocations.INPUT_FIELD_DEFINITION | 
            DirectiveLocations.ENUM_VALUE,
            Location.Empty));

        directives.Add(new DirectiveDefinition(
            "Exposes a URL that specifies the behaviour of this scalar.",
            "@specifiedBy",
            new()
            {
                { "url", new InputValueDefinition(
                    "The URL that specifies the behaviour of this scalar.",
                    "url",
                    new TypeNonNull(new TypeName("String", Location.Empty), Location.Empty),
                    null,
                    [],
                    Location.Empty,
                    "Argument")
                }
            },
            false,
            DirectiveLocations.SCALAR,
            Location.Empty));

        foreach(var directive in directives)
        {
            directive.IsBuiltIn = true;
            _directives.Add(directive.Name, directive);
        }
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
            var scalar = new ScalarTypeDefinition(scalarPair.Item2, scalarPair.Item1, [], Location.Empty);
            scalar.IsBuiltIn = true;
            _types.Add(scalarPair.Item1, scalar);
        }
    }

    private static void FatalException(ValidationException validationException)
    {
        throw validationException;
    }

    private void NonFatalException(ValidationException validationException)
    {
        _exceptions.Add(validationException);
    }

    private void CheckExceptions()
    {
        if (_exceptions.Count == 1)
            throw _exceptions[0];
        else if (_exceptions.Count > 1)
            throw new RocketExceptions(_exceptions);
    }
}

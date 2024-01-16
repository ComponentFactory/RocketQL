namespace RocketQL.Core.Base;

public partial class Request : IRequest
{
    private ISchema _schema = new Schema();
    private ValueNode _variables = NullValueNode.Null;
    private readonly OperationDefinitions _operations = [];
    private readonly FragmentDefinitions _fragments = [];
    private readonly List<ValidationException> _exceptions = [];
    private readonly SyntaxNodeList _nodes = [];

    public IReadOnlyDictionary<string, OperationDefinition> Operations { get; protected set; } = OperationDefinitions.Empty;
    public IReadOnlyDictionary<string, FragmentDefinition> Fragments { get; protected set; } = FragmentDefinitions.Empty;
    public bool IsValidatedSchema { get; protected set; } = false;
    public bool IsValidatedVariables { get; protected set; } = false;

    public void Add(SyntaxNode node)
    {
        _nodes.Add(node);
        IsValidatedSchema = false;
    }

    public void Add(IEnumerable<SyntaxNode> nodes)
    {
        _nodes.AddRange(nodes);
        IsValidatedSchema = false;
    }

    public void Add(SyntaxNodeList nodes)
    {
        _nodes.AddRange(nodes);
        IsValidatedSchema = false;
    }

    public void Add(IEnumerable<SyntaxNodeList> schemas)
    {
        foreach (var nodes in schemas)
            _nodes.AddRange(nodes);

        IsValidatedSchema = false;
    }

    public void Add(ReadOnlySpan<char> schema, string source)
    {
        Add(Serialization.RequestDeserialize(schema, source));
    }

    public void Add(
        ReadOnlySpan<char> schema,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        Add(Serialization.RequestDeserialize(schema, CallerExtensions.CallerToSource(filePath, memberName, lineNumber)));
    }


    public void ValidateSchema(ISchema schema)
    {
        CleanSchema();

        if (!schema.IsValidated)
            FatalException(ValidationException.SchemaNotValidated());

        try
        {
            _schema = schema;
            Converter.Visit();
            Linker.Visit();
            CheckExceptions();
            IsValidatedSchema = true;
        }
        catch
        {
            CleanSchema();
            throw;
        }
    }

    public void ValidateVariables(ValueNode variables)
    {
        CleanVariables();

        if (!IsValidatedSchema)
            FatalException(ValidationException.SchemaNotValidated());

        try
        {
            _variables = variables;
            CheckExceptions();
            IsValidatedVariables = true;
        }
        catch
        {
            CleanVariables();
            throw;
        }
    }

    public void Reset()
    {
        _nodes.Clear();
        CleanSchema();
    }

    private void CleanSchema()
    {
        _operations.Clear();
        _fragments.Clear();

        _schema = new Schema();
        Operations = OperationDefinitions.Empty;
        Fragments = FragmentDefinitions.Empty;

        IsValidatedSchema = false;
        CleanVariables();
    }

    private void CleanVariables()
    {
        _exceptions.Clear();
        _variables = null;
        IsValidatedVariables = false;
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

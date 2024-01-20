namespace RocketQL.Core.Base;

public partial class RequestBuilder
{
    private readonly OperationDefinitions _operations = [];
    private readonly FragmentDefinitions _fragments = [];
    private readonly List<ValidationException> _exceptions = [];
    private readonly SyntaxNodeList _nodes = [];

    public RequestBuilder AddSyntaxNode(SyntaxNode node)
    {
        _nodes.Add(node);
        return this;
    }

    public RequestBuilder AddSyntaxNodes(IEnumerable<SyntaxNode> nodes)
    {
        _nodes.AddRange(nodes);
        return this;
    }

    public RequestBuilder AddSyntaxNodes(SyntaxNodeList nodes)
    {
        _nodes.AddRange(nodes);
        return this;
    }

    public RequestBuilder AddSyntaxNodes(IEnumerable<SyntaxNodeList> schemas)
    {
        foreach (var nodes in schemas)
            _nodes.AddRange(nodes);

        return this;
    }

    public RequestBuilder AddFromString(ReadOnlySpan<char> schema, string source)
    {
        AddSyntaxNodes(Serialization.RequestDeserialize(schema, source));
        return this;
    }

    public RequestBuilder AddFromString(ReadOnlySpan<char> schema,
                                        [CallerFilePath] string filePath = "",
                                        [CallerMemberName] string memberName = "",
                                        [CallerLineNumber] int lineNumber = 0)
    {
        AddSyntaxNodes(Serialization.RequestDeserialize(schema, CallerExtensions.CallerToSource(filePath, memberName, lineNumber)));
        return this;
    }

    public Request Build(ISchema schema)
    {
        Clean();

        try
        {
            Converter.Visit(schema);
            Linker.Visit(schema);
            Validator.Visit(schema);
            CheckExceptions();

            return new Request(_operations, _fragments);
        }
        catch
        {
            Clean();
            throw;
        }
    }


    private void Clean()
    {
        _operations.Clear();
        _fragments.Clear();
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

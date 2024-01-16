namespace RocketQL.Core.Base;

public interface IRequest
{
    public IReadOnlyDictionary<string, OperationDefinition> Operations { get; }
    public IReadOnlyDictionary<string, FragmentDefinition> Fragments { get; }

    bool IsValidatedSchema { get; }
    bool IsValidatedVariables { get; }

    void Add(SyntaxNode node);
    void Add(IEnumerable<SyntaxNode> nodes);
    void Add(SyntaxNodeList nodes);
    void Add(IEnumerable<SyntaxNodeList> schemas);
    void Add(ReadOnlySpan<char> schema, string source);
    void Add(ReadOnlySpan<char> schema, 
             [CallerFilePath] string filePath = "", 
             [CallerMemberName] string memberName = "", 
             [CallerLineNumber] int lineNumber = 0);

    void ValidateSchema(ISchema schema);
    void ValidateVariables(ValueNode variables);

    void Reset();
}

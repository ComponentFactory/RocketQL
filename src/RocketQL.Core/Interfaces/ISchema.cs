namespace RocketQL.Core.Base;

public interface ISchema
{
    SchemaRoot Root { get; }
    IReadOnlyDictionary<string, DirectiveDefinition> Directives { get; }
    IReadOnlyDictionary<string, TypeDefinition> Types { get;  }
    bool IsValidated { get; }

    void Add(SyntaxNode node);
    void Add(IEnumerable<SyntaxNode> nodes);
    void Add(SyntaxNodeList nodes);
    void Add(IEnumerable<SyntaxNodeList> schemas);
    void Add(ReadOnlySpan<char> schema, string source);
    void Add(ReadOnlySpan<char> schema, 
             [CallerFilePath] string filePath = "", 
             [CallerMemberName] string memberName = "", 
             [CallerLineNumber] int lineNumber = 0);

    void Validate();

    bool IsInputTypeCompatibleWithValue(TypeNode typeNode, ValueNode valueNode);

    void Reset();
}

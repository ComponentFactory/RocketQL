namespace RocketQL.Core.Visitors;

public abstract class NodePathTracker
{
    private readonly Stack<object> _path = [];

    public void PushPath(object segment) => _path.Push(segment);
    public void PopPath() => _path.Pop();

    public string[] CurrentPath
    {
        get
        {
            List<string> path = [];

            foreach (var segment in _path.Reverse())
                path.Add(segment switch
                {
                    ObjectFieldNode fieldNode => $"argument {fieldNode.Name}",
                    SyntaxNode syntaxNode => $"{syntaxNode.OutputElement().ToLower()} {syntaxNode.OutputName()}",
                    OperationTypeDefinition operationType => $"{operationType.Operation.ToString().ToLower()} {operationType.NamedType}",
                    OperationDefinition operation => OperationDefinitionToString(operation),
                    DocumentNode documentNode => $"{documentNode.OutputElement().ToLower()} {documentNode.OutputName()}",
                    _ => segment.ToString(),
                } ?? ""); ;
            ;

            return [.. path];
        }
    }

    private string OperationDefinitionToString(OperationDefinition operation)
    {
        var operationName = string.IsNullOrEmpty(operation.Name) ? "(anon)" : operation.Name;
        return $"{operation.Operation.ToString().ToLower()} {operationName}";
    }
}

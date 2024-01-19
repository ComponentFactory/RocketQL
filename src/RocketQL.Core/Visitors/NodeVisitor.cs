using RocketQL.Core.Nodes;

namespace RocketQL.Core.Visitors;

public abstract class NodeVisitor
{
    private readonly Stack<object> _path = [];

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
                    DocumentNode documentNode => $"{documentNode.OutputElement.ToLower()} {documentNode.OutputName}",
                    _ => segment.ToString(),
                } ?? "");

            return [.. path];
        }
    }

    public void PushPath(object segment) => _path.Push(segment);
    public void PopPath() => _path.Pop();
}

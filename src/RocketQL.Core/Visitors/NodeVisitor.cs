namespace RocketQL.Core.Visitors;

public abstract class NodeVisitor
{
    private readonly Stack<string> _path = [];

    public string[] CurrentPath => _path.ToArray();
    public void PushPath(string segment) => _path.Push(segment);
    public void PopPath() => _path.Pop();
}
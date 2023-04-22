namespace RocketQL.Core.Requests;

public class RequestExecution : IExecuteRequest
{
    private readonly IRootSchemaResolver _resolver;

    public RequestExecution(IRootSchemaResolver resolver)
    {
        _resolver = resolver;
    }

    public ValueNode ExecuteRequest(string executable, string variables, string? operationName)
    {
        return Execute(Document.RequestDeserialize(executable), Json.Deserialize(variables), operationName);
    }

    public ValueNode Execute(RequestNode executable, ValueNode variables, string? operationName)
    {
        return NullValueNode.Null;
    }
}


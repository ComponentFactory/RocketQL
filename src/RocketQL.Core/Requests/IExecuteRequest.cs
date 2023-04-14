namespace RocketQL.Core;

public interface IExecuteRequest
{
    ValueNode ExecuteRequest(string executable, string variables, string? operationName);
    ValueNode Execute(ExecutableDocumentNode executable, ValueNode variables, string? operationName);
}


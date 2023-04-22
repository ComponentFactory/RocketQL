namespace RocketQL.Core.Requests;

public interface IExecuteRequest
{
    ValueNode ExecuteRequest(string executable, string variables, string? operationName);
    ValueNode Execute(RequestNode executable, ValueNode variables, string? operationName);
}


namespace RocketQL.Core
{
    public class RequestExecution : IExecuteRequest
    {
        private readonly IRootSchemaResolver _resolver;

        public RequestExecution(IRootSchemaResolver resolver)
        {
            _resolver = resolver;
        }

        public ValueNode ExecuteRequest(string executable, string variables, string? operationName)
        {
            return Execute(new ExecutableParser(executable).Parse(), new JsonParser(variables).Parse(), operationName);
        }

        public ValueNode Execute(ExecutableDocumentNode executable, ValueNode variables, string? operationName)
        {
            return NullValueNode.Null;
        }
    }
}

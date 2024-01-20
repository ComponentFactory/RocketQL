namespace RocketQL.Core.Base;

public partial class Request(IReadOnlyDictionary<string, OperationDefinition> operations,
                             IReadOnlyDictionary<string, FragmentDefinition> fragments) : IRequest
{
    public static readonly Request Empty = new(new OperationDefinitions(), new FragmentDefinitions());

    public IReadOnlyDictionary<string, OperationDefinition> Operations => operations;
    public IReadOnlyDictionary<string, FragmentDefinition> Fragments => fragments;
}

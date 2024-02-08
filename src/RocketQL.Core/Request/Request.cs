namespace RocketQL.Core.Base;

public partial class Request(ISchema schema,
                             IReadOnlyDictionary<string, OperationDefinition> operations,
                             IReadOnlyDictionary<string, FragmentDefinition> fragments) : IRequest
{
    public static readonly Request Empty = new(Base.Schema.Empty, new OperationDefinitions(), new FragmentDefinitions());

    public ISchema Schema => schema;
    public IReadOnlyDictionary<string, OperationDefinition> Operations => operations;
    public IReadOnlyDictionary<string, FragmentDefinition> Fragments => fragments;
}

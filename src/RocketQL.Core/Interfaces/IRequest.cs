namespace RocketQL.Core.Base;

public interface IRequest
{
    ISchema Schema { get; }
    IReadOnlyDictionary<string, OperationDefinition> Operations { get; }
    IReadOnlyDictionary<string, FragmentDefinition> Fragments { get; }
}

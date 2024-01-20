namespace RocketQL.Core.Base;

public interface IRequest
{
    IReadOnlyDictionary<string, OperationDefinition> Operations { get; }
    IReadOnlyDictionary<string, FragmentDefinition> Fragments { get; }
}

namespace RocketQL.Core.Base;

public interface ISchema
{
    SchemaRoot Root { get; }
    IReadOnlyDictionary<string, DirectiveDefinition> Directives { get; }
    IReadOnlyDictionary<string, TypeDefinition> Types { get; }
}

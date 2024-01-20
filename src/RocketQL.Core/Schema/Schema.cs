namespace RocketQL.Core.Base;

public class Schema(SchemaRoot root,
                    IReadOnlyDictionary<string, DirectiveDefinition> directives,
                    IReadOnlyDictionary<string, TypeDefinition> types) : ISchema
{
    public static readonly Schema Empty = new(SchemaRoot.Empty, new DirectiveDefinitions(), new TypeDefinitions());

    public SchemaRoot Root => root;
    public IReadOnlyDictionary<string, DirectiveDefinition> Directives => directives;
    public IReadOnlyDictionary<string, TypeDefinition> Types => types;
}

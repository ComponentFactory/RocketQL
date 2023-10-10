namespace RocketQL.Core.Nodes;

public record class ValidatedSchemaNode(Dictionary<string, DirectiveDefinitionNode> Directives);


namespace RocketQL.Core.Nodes;

public record class ScalarTypeDefinitionNode(string Description, string Name, Location Location) : LocationNode(Location);

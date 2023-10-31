namespace RocketQL.Core.Nodes;

public record class DirectiveDefinitionNode(string Description, string Name, InputValueDefinitionNodeList Arguments, bool Repeatable, DirectiveLocations DirectiveLocations, Location Location);

public class DirectiveNodeList : List<DirectiveNode> { };
public class ObjectFieldNodeList : List<ObjectFieldNode> { };
public class InputValueDefinitionNodeList : List<InputValueDefinitionNode> { };

public record class DirectiveNode(DirectiveDefinitionNode Directive, ObjectFieldNodeList Arguments);
public record class InputValueDefinitionNode(string Description, string Name, TypeNode Type, ValueNode? DefaultValue, DirectiveNodeList Directives);

public abstract record class TypeNode(bool NonNull);
public record class TypeNameNode(TypeDefinitionNode Type, bool NonNull) : TypeNode(NonNull);
public record class TypeListNode(TypeNode Type, bool NonNull) : TypeNode(NonNull);

public abstract record class TypeDefinitionNode();


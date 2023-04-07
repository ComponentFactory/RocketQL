namespace RocketQL.Core;

public record class DocumentNode(DirectiveDefinitionNodeList? DirectiveDefinitions, ScalarTypeDefinitionNodeList? ScalarTypeDefinitions);

public record class DirectiveDefinitionNode(string Description, string Name, InputValueDefinitionNodeList? Arguments, bool Repeatable, DirectiveLocations DirectiveLocations);
public record class InputValueDefinitionNode(string Description, string Name, TypeNode Type, ValueNode? DefaultValue, DirectiveNodeList? Directives);
public record class ScalarTypeDefinitionNode(string Description, string Name, DirectiveNodeList? Directives);

public record class DirectiveNode(string Name, ObjectFieldNodeList? Arguments);

public abstract record class TypeNode(bool NonNull);
public record class TypeNameNode(string Name, bool NonNull) : TypeNode(NonNull);
public record class TypeListNode(TypeNode Type, bool NonNull) : TypeNode(NonNull);

public abstract record class ValueNode();
public record class NullValueNode() : ValueNode();
public record class BooleanValueNode(bool Value) : ValueNode();
public record class IntValueNode(string Value) : ValueNode();
public record class FloatValueNode(string Value) : ValueNode();
public record class StringValueNode(string Value) : ValueNode();
public record class EnumValueNode(string Value) : ValueNode();
public record class ListValueNode(ValueNodeList? Values) : ValueNode();
public record class ObjectValueNode(ObjectFieldNodeList? ObjectFields) : ValueNode();
public record class ObjectFieldNode(string Name, ValueNode Value) : ValueNode();

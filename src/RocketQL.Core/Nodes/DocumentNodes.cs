namespace RocketQL.Core.Nodes;

public record class RequestNode(OperationDefinitionNodeList Operations, FragmentDefinitionNodeList Fragments);

public record class SchemaNode(DirectiveDefinitionNodeList Directives,
                               SchemaDefinitionNodeList Schemas, ExtendSchemaDefinitionNodeList ExtendSchemas,
                               ScalarTypeDefinitionNodeList ScalarTypes, ExtendScalarTypeDefinitionNodeList ExtendScalarTypes,
                               ObjectTypeDefinitionNodeList ObjectTypes, ExtendObjectTypeDefinitionNodeList ExtendObjectTypes,
                               InterfaceTypeDefinitionNodeList InterfaceTypes, ExtendInterfaceTypeDefinitionNodeList ExtendInterfaceTypes,
                               UnionTypeDefinitionNodeList UnionTypes, ExtendUnionTypeDefinitionNodeList ExtendUnionTypes,
                               EnumTypeDefinitionNodeList EnumTypes, ExtendEnumTypeDefinitionNodeList ExtendEnumTypes,
                               InputObjectTypeDefinitionNodeList InputObjectTypes, ExtendInputObjectTypeDefinitionNodeList ExtendInputObjectTypes);

public class OperationDefinitionNodeList : List<OperationDefinitionNode> { };
public class FragmentDefinitionNodeList : List<FragmentDefinitionNode> { };
public class DirectiveDefinitionNodeList : List<DirectiveDefinitionNode> { };
public class SchemaDefinitionNodeList : List<SchemaDefinitionNode> { };
public class ExtendSchemaDefinitionNodeList : List<ExtendSchemaDefinitionNode> { };
public class ScalarTypeDefinitionNodeList : List<ScalarTypeDefinitionNode> { };
public class ExtendScalarTypeDefinitionNodeList : List<ExtendScalarTypeDefinitionNode> { };
public class ObjectTypeDefinitionNodeList : List<ObjectTypeDefinitionNode> { };
public class ExtendObjectTypeDefinitionNodeList : List<ExtendObjectTypeDefinitionNode> { };
public class InterfaceTypeDefinitionNodeList : List<InterfaceTypeDefinitionNode> { };
public class ExtendInterfaceTypeDefinitionNodeList : List<ExtendInterfaceTypeDefinitionNode> { };
public class UnionTypeDefinitionNodeList : List<UnionTypeDefinitionNode> { };
public class ExtendUnionTypeDefinitionNodeList : List<ExtendUnionTypeDefinitionNode> { };
public class EnumTypeDefinitionNodeList : List<EnumTypeDefinitionNode> { };
public class ExtendEnumTypeDefinitionNodeList : List<ExtendEnumTypeDefinitionNode> { };
public class InputObjectTypeDefinitionNodeList : List<InputObjectTypeDefinitionNode> { };
public class ExtendInputObjectTypeDefinitionNodeList : List<ExtendInputObjectTypeDefinitionNode> { };
public class DirectiveNodeList : List<DirectiveNode> { };
public class EnumValueDefinitionList : List<EnumValueDefinition> { };
public class FieldDefinitionNodeList : List<FieldDefinitionNode> { };
public class InputValueDefinitionNodeList : List<InputValueDefinitionNode> { };
public class ObjectFieldNodeList : List<ObjectFieldNode> { };
public class OperationTypeDefinitionNodeList : List<OperationTypeDefinitionNode> { };
public class SelectionDefinitionNodeList : List<SelectionNode> { };
public class NameList : List<string> { };
public class VariableDefinitionNodeList : List<VariableDefinitionNode> { };

public record class OperationDefinitionNode(OperationType Operation, string Name, VariableDefinitionNodeList VariableDefinitions, DirectiveNodeList Directives, SelectionDefinitionNodeList SelectionSet);
public record class FragmentDefinitionNode(string Name, string TypeCondition, DirectiveNodeList Directives, SelectionDefinitionNodeList SelectionSet);
public record class ExtendSchemaDefinitionNode(DirectiveNodeList Directives, OperationTypeDefinitionNodeList OperationTypes);
public record class SchemaDefinitionNode(string Description, DirectiveNodeList Directives, OperationTypeDefinitionNodeList OperationTypes) : ExtendSchemaDefinitionNode(Directives, OperationTypes);
public record class ExtendScalarTypeDefinitionNode(string Name, DirectiveNodeList Directives);
public record class ScalarTypeDefinitionNode(string Description, string Name, DirectiveNodeList Directives) : ExtendScalarTypeDefinitionNode(Name, Directives);
public record class ExtendObjectTypeDefinitionNode(string Name, NameList ImplementsInterfaces, DirectiveNodeList Directives, FieldDefinitionNodeList Fields);
public record class ObjectTypeDefinitionNode(string Description, string Name, NameList ImplementsInterfaces, DirectiveNodeList Directives, FieldDefinitionNodeList Fields) : ExtendObjectTypeDefinitionNode(Name, ImplementsInterfaces, Directives, Fields);
public record class ExtendInterfaceTypeDefinitionNode(string Name, NameList ImplementsInterfaces, DirectiveNodeList Directives, FieldDefinitionNodeList Fields);
public record class InterfaceTypeDefinitionNode(string Description, string Name, NameList ImplementsInterfaces, DirectiveNodeList Directives, FieldDefinitionNodeList Fields) : ExtendInterfaceTypeDefinitionNode(Name, ImplementsInterfaces, Directives, Fields);
public record class ExtendUnionTypeDefinitionNode(string Name, DirectiveNodeList Directives, NameList MemberTypes);
public record class UnionTypeDefinitionNode(string Description, string Name, DirectiveNodeList Directives, NameList MemberTypes) : ExtendUnionTypeDefinitionNode(Name, Directives, MemberTypes);
public record class ExtendEnumTypeDefinitionNode(string Name, DirectiveNodeList Directives, EnumValueDefinitionList EnumValues);
public record class EnumTypeDefinitionNode(string Description, string Name, DirectiveNodeList Directives, EnumValueDefinitionList EnumValues) : ExtendEnumTypeDefinitionNode(Name, Directives, EnumValues);
public record class ExtendInputObjectTypeDefinitionNode(string Name, DirectiveNodeList Directives, InputValueDefinitionNodeList InputFields);
public record class InputObjectTypeDefinitionNode(string Description, string Name, DirectiveNodeList Directives, InputValueDefinitionNodeList InputFields) : ExtendInputObjectTypeDefinitionNode(Name, Directives, InputFields);
public record class DirectiveDefinitionNode(string Description, string Name, InputValueDefinitionNodeList Arguments, bool Repeatable, DirectiveLocations DirectiveLocations, Location Location);

public record class DirectiveNode(string Name, ObjectFieldNodeList Arguments);
public record class OperationTypeDefinitionNode(OperationType Operation, string NamedType);
public record class VariableDefinitionNode(string Name, TypeNode Type, ValueNode? DefaultValue, DirectiveNodeList Directives);
public record class InputValueDefinitionNode(string Description, string Name, TypeNode Type, ValueNode? DefaultValue, DirectiveNodeList Directives);
public record class FieldDefinitionNode(string Description, string Name, InputValueDefinitionNodeList Arguments, TypeNode Type, DirectiveNodeList Directives);
public record class EnumValueDefinition(string Description, string Name, DirectiveNodeList Directives);

public abstract record class SelectionNode();
public record class FieldSelectionNode(string Alias, string Name, ObjectFieldNodeList Arguments, DirectiveNodeList Directives, SelectionDefinitionNodeList SelectionSet) : SelectionNode();
public record class FragmentSpreadSelectionNode(string Name, DirectiveNodeList Directives) : SelectionNode();
public record class InlineFragmentSelectionNode(string TypeCondition, DirectiveNodeList Directives, SelectionDefinitionNodeList SelectionSet) : SelectionNode();

public abstract record class TypeNode(bool NonNull);
public record class TypeNameNode(string Name, bool NonNull) : TypeNode(NonNull);
public record class TypeListNode(TypeNode Type, bool NonNull) : TypeNode(NonNull);



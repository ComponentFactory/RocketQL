namespace RocketQL.Core.Nodes;

public record class SyntaxRequestNode(SyntaxOperationDefinitionNodeList Operations, SyntaxFragmentDefinitionNodeList Fragments);

public record class SyntaxSchemaNode(SyntaxDirectiveDefinitionNodeList Directives,
                                     SyntaxSchemaDefinitionNodeList Schemas, SyntaxExtendSchemaDefinitionNodeList ExtendSchemas,
                                     SyntaxScalarTypeDefinitionNodeList ScalarTypes, SyntaxExtendScalarTypeDefinitionNodeList ExtendScalarTypes,
                                     SyntaxObjectTypeDefinitionNodeList ObjectTypes, SyntaxExtendObjectTypeDefinitionNodeList ExtendObjectTypes,
                                     SyntaxInterfaceTypeDefinitionNodeList InterfaceTypes, SyntaxExtendInterfaceTypeDefinitionNodeList ExtendInterfaceTypes,
                                     SyntaxUnionTypeDefinitionNodeList UnionTypes, SyntaxExtendUnionTypeDefinitionNodeList ExtendUnionTypes,
                                     SyntaxEnumTypeDefinitionNodeList EnumTypes, SyntaxExtendEnumTypeDefinitionNodeList ExtendEnumTypes,
                                     SyntaxInputObjectTypeDefinitionNodeList InputObjectTypes, SyntaxExtendInputObjectTypeDefinitionNodeList ExtendInputObjectTypes);

public class SyntaxOperationDefinitionNodeList : List<SyntaxOperationDefinitionNode> { };
public class SyntaxFragmentDefinitionNodeList : List<SyntaxFragmentDefinitionNode> { };
public class SyntaxDirectiveDefinitionNodeList : List<SyntaxDirectiveDefinitionNode> { };
public class SyntaxSchemaDefinitionNodeList : List<SyntaxSchemaDefinitionNode> { };
public class SyntaxExtendSchemaDefinitionNodeList : List<SyntaxExtendSchemaDefinitionNode> { };
public class SyntaxScalarTypeDefinitionNodeList : List<SyntaxScalarTypeDefinitionNode> { };
public class SyntaxExtendScalarTypeDefinitionNodeList : List<SyntaxExtendScalarTypeDefinitionNode> { };
public class SyntaxObjectTypeDefinitionNodeList : List<SyntaxObjectTypeDefinitionNode> { };
public class SyntaxExtendObjectTypeDefinitionNodeList : List<SyntaxExtendObjectTypeDefinitionNode> { };
public class SyntaxInterfaceTypeDefinitionNodeList : List<SyntaxInterfaceTypeDefinitionNode> { };
public class SyntaxExtendInterfaceTypeDefinitionNodeList : List<SyntaxExtendInterfaceTypeDefinitionNode> { };
public class SyntaxUnionTypeDefinitionNodeList : List<SyntaxUnionTypeDefinitionNode> { };
public class SyntaxExtendUnionTypeDefinitionNodeList : List<SyntaxExtendUnionTypeDefinitionNode> { };
public class SyntaxEnumTypeDefinitionNodeList : List<SyntaxEnumTypeDefinitionNode> { };
public class SyntaxExtendEnumTypeDefinitionNodeList : List<SyntaxExtendEnumTypeDefinitionNode> { };
public class SyntaxInputObjectTypeDefinitionNodeList : List<SyntaxInputObjectTypeDefinitionNode> { };
public class SyntaxExtendInputObjectTypeDefinitionNodeList : List<SyntaxExtendInputObjectTypeDefinitionNode> { };
public class SyntaxDirectiveNodeList : List<SyntaxDirectiveNode> { };
public class SyntaxEnumValueDefinitionList : List<SyntaxEnumValueDefinition> { };
public class SyntaxFieldDefinitionNodeList : List<SyntaxFieldDefinitionNode> { };
public class SyntaxInputValueDefinitionNodeList : List<SyntaxInputValueDefinitionNode> { };
public class SyntaxObjectFieldNodeList : List<ObjectFieldNode> { };
public class SyntaxOperationTypeDefinitionNodeList : List<SyntaxOperationTypeDefinitionNode> { };
public class SyntaxSelectionDefinitionNodeList : List<SyntaxSelectionNode> { };
public class SyntaxNameList : List<string> { };
public class SyntaxVariableDefinitionNodeList : List<SyntaxVariableDefinitionNode> { };

public record class SyntaxOperationDefinitionNode(OperationType Operation, string Name, SyntaxVariableDefinitionNodeList VariableDefinitions, SyntaxDirectiveNodeList Directives, SyntaxSelectionDefinitionNodeList SelectionSet);
public record class SyntaxFragmentDefinitionNode(string Name, string TypeCondition, SyntaxDirectiveNodeList Directives, SyntaxSelectionDefinitionNodeList SelectionSet);
public record class SyntaxExtendSchemaDefinitionNode(SyntaxDirectiveNodeList Directives, SyntaxOperationTypeDefinitionNodeList OperationTypes);
public record class SyntaxSchemaDefinitionNode(string Description, SyntaxDirectiveNodeList Directives, SyntaxOperationTypeDefinitionNodeList OperationTypes) : SyntaxExtendSchemaDefinitionNode(Directives, OperationTypes);
public record class SyntaxExtendScalarTypeDefinitionNode(string Name, SyntaxDirectiveNodeList Directives);
public record class SyntaxScalarTypeDefinitionNode(string Description, string Name, SyntaxDirectiveNodeList Directives) : SyntaxExtendScalarTypeDefinitionNode(Name, Directives);
public record class SyntaxExtendObjectTypeDefinitionNode(string Name, SyntaxNameList ImplementsInterfaces, SyntaxDirectiveNodeList Directives, SyntaxFieldDefinitionNodeList Fields);
public record class SyntaxObjectTypeDefinitionNode(string Description, string Name, SyntaxNameList ImplementsInterfaces, SyntaxDirectiveNodeList Directives, SyntaxFieldDefinitionNodeList Fields) : SyntaxExtendObjectTypeDefinitionNode(Name, ImplementsInterfaces, Directives, Fields);
public record class SyntaxExtendInterfaceTypeDefinitionNode(string Name, SyntaxNameList ImplementsInterfaces, SyntaxDirectiveNodeList Directives, SyntaxFieldDefinitionNodeList Fields);
public record class SyntaxInterfaceTypeDefinitionNode(string Description, string Name, SyntaxNameList ImplementsInterfaces, SyntaxDirectiveNodeList Directives, SyntaxFieldDefinitionNodeList Fields) : SyntaxExtendInterfaceTypeDefinitionNode(Name, ImplementsInterfaces, Directives, Fields);
public record class SyntaxExtendUnionTypeDefinitionNode(string Name, SyntaxDirectiveNodeList Directives, SyntaxNameList MemberTypes);
public record class SyntaxUnionTypeDefinitionNode(string Description, string Name, SyntaxDirectiveNodeList Directives, SyntaxNameList MemberTypes) : SyntaxExtendUnionTypeDefinitionNode(Name, Directives, MemberTypes);
public record class SyntaxExtendEnumTypeDefinitionNode(string Name, SyntaxDirectiveNodeList Directives, SyntaxEnumValueDefinitionList EnumValues);
public record class SyntaxEnumTypeDefinitionNode(string Description, string Name, SyntaxDirectiveNodeList Directives, SyntaxEnumValueDefinitionList EnumValues) : SyntaxExtendEnumTypeDefinitionNode(Name, Directives, EnumValues);
public record class SyntaxExtendInputObjectTypeDefinitionNode(string Name, SyntaxDirectiveNodeList Directives, SyntaxInputValueDefinitionNodeList InputFields);
public record class SyntaxInputObjectTypeDefinitionNode(string Description, string Name, SyntaxDirectiveNodeList Directives, SyntaxInputValueDefinitionNodeList InputFields) : SyntaxExtendInputObjectTypeDefinitionNode(Name, Directives, InputFields);
public record class SyntaxDirectiveDefinitionNode(string Description, string Name, SyntaxInputValueDefinitionNodeList Arguments, bool Repeatable, DirectiveLocations DirectiveLocations, Location Location);

public record class SyntaxDirectiveNode(string Name, SyntaxObjectFieldNodeList Arguments);
public record class SyntaxOperationTypeDefinitionNode(OperationType Operation, string NamedType);
public record class SyntaxVariableDefinitionNode(string Name, SyntaxTypeNode Type, ValueNode? DefaultValue, SyntaxDirectiveNodeList Directives);
public record class SyntaxInputValueDefinitionNode(string Description, string Name, SyntaxTypeNode Type, ValueNode? DefaultValue, SyntaxDirectiveNodeList Directives);
public record class SyntaxFieldDefinitionNode(string Description, string Name, SyntaxInputValueDefinitionNodeList Arguments, SyntaxTypeNode Type, SyntaxDirectiveNodeList Directives);
public record class SyntaxEnumValueDefinition(string Description, string Name, SyntaxDirectiveNodeList Directives);

public abstract record class SyntaxSelectionNode();
public record class SyntaxFieldSelectionNode(string Alias, string Name, SyntaxObjectFieldNodeList Arguments, SyntaxDirectiveNodeList Directives, SyntaxSelectionDefinitionNodeList SelectionSet) : SyntaxSelectionNode();
public record class SyntaxFragmentSpreadSelectionNode(string Name, SyntaxDirectiveNodeList Directives) : SyntaxSelectionNode();
public record class SyntaxInlineFragmentSelectionNode(string TypeCondition, SyntaxDirectiveNodeList Directives, SyntaxSelectionDefinitionNodeList SelectionSet) : SyntaxSelectionNode();

public abstract record class SyntaxTypeNode(bool NonNull);
public record class SyntaxTypeNameNode(string Name, bool NonNull) : SyntaxTypeNode(NonNull);
public record class SyntaxTypeListNode(SyntaxTypeNode Type, bool NonNull) : SyntaxTypeNode(NonNull);
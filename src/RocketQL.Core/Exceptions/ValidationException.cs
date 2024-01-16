namespace RocketQL.Core.Exceptions;

public class ValidationException(Location location, string message, string[] path) : RocketException(location, message)
{
    public ValidationException(Location location, string message)
        : this(location, message, [])
    {
    }

    public string[] Path { get; init; } = path;
    public string CommaPath => string.Join(", ", Path);

    public static ValidationException UnrecognizedType(Location location, string name, string[] path) => new(location, $"Unrecognized type '{name}' encountered.", path);
    public static ValidationException UnrecognizedType(SyntaxNode node, string[] path) => new(node.Location, $"Unrecognized type '{node.GetType()}' encountered.", path);
    public static ValidationException TypeNameAlreadyDefined(LocationNode node, string name, string type, string[] path) => new(node.Location, $"{type} '{name}' is already defined.", path);
    public static ValidationException DuplicateName(LocationNode node, string usage, string name, string[] path) => new(node.Location, $"Duplicate {usage} '{name}'.", path);
    public static ValidationException DefinitionNotAllowedInSchema(LocationNode node, string definition) => new(node.Location, $"{definition} definition not allowed in a schema.", []);
    public static ValidationException SchemaDefinitionAlreadyEncountered(LocationNode node, string[] path) => new(node.Location, $"Schema definition already encountered.", path);
    public static ValidationException SchemaDefinitionMultipleOperation(SyntaxOperationTypeDefinitionNode node, string[] path) => new(node.Location, $"Schema defines the {node.Operation.ToString().ToLower()} operation more than once.", path);
    public static ValidationException RequestAnonymousAlreadyDefined(SyntaxOperationDefinitionNode node, string[] path) => new(node.Location, "Anonymous operation is already defined.", path);
    public static ValidationException RequestOperationAlreadyDefined(SyntaxOperationDefinitionNode node, string[] path) => new(node.Location, $"Operation name '{node.Name}' is already defined.", path);
    public static ValidationException RequestAnonymousAndNamed(SyntaxOperationDefinitionNode node, string[] path) => new(node.Location, "Anonymous operation and named operation both defined.", path);
    public static ValidationException ExtendSchemaNotDefined(LocationNode node, string[] path) => new(node.Location, $"Extend schema cannot be applied because no schema has been defined.", path);
    public static ValidationException ExtendSchemaMandatory(LocationNode node, string[] path) => new(node.Location, $"Extend schema must specify at least one directive or operation.", path);
    public static ValidationException ExtendSchemaOperationAlreadyDefined(LocationNode node, OperationType operation, string[] path) => new(node.Location, $"Extend schema cannot add {operation.ToString().ToLower()} operation because it is already defined.", path);
    public static ValidationException ExtendIncorrectType(LocationNode node, string type, TypeDefinition typeDefinition, string[] path) => new(node.Location, $"Extend '{type.ToLower()}' cannot be used to extend {typeDefinition.OutputElement} '{typeDefinition.OutputName}'.", path);
    public static ValidationException ExtendScalarMandatory(LocationNode node, string name, string[] path) => new(node.Location, $"Extend scalar '{name}' must specify at least one directive.", path);
    public static ValidationException ExtendObjectMandatory(LocationNode node, string name, string[] path) => new(node.Location, $"Extend object '{name}' must specify at least one implements, directive or field.", path);
    public static ValidationException ExtendInterfaceMandatory(LocationNode node, string name, string[] path) => new(node.Location, $"Extend interface '{name}' must specify at least one implements, directive or field.", path);
    public static ValidationException ExtendObjectImplementAlreadyDefined(LocationNode node, string name, string interfaceType, string[] path) => new(node.Location, $"Extend object '{name}' specifies an interface '{interfaceType}' already defined.", path);
    public static ValidationException ExtendInterfaceImplementAlreadyDefined(LocationNode node, string name, string interfaceType, string[] path) => new(node.Location, $"Extend interface '{name}' specifies an interface '{interfaceType}' already defined.", path);
    public static ValidationException ExtendUnionMandatory(LocationNode node, string name, string[] path) => new(node.Location, $"Extend union '{name}' must specify at least one directive or type.", path);
    public static ValidationException ExtendUnionAlreadyDefined(LocationNode node, string name, string memberType, string[] path) => new(node.Location, $"Extend union '{name}' specifies a member type '{memberType}' already defined.", path);
    public static ValidationException ExtendEnumMandatory(LocationNode node, string name, string[] path) => new(node.Location, $"Extend enum '{name}' must specify at least one directive or field.", path);
    public static ValidationException ExtendEnumValueAlreadyDefined(LocationNode node, string name, string enumValue, string[] path) => new(node.Location, $"Extend enum '{name}' has duplicate definition of enum value '{enumValue}'.", path);
    public static ValidationException ExtendExistingEnumValueUnchanged(LocationNode node, string name, string enumValue, string[] path) => new(node.Location, $"Extend enum '{name}' for existing enum value '{enumValue}' does not make any change.", path);
    public static ValidationException ExtendInputObjectMandatory(LocationNode node, string name, string[] path) => new(node.Location, $"Extend input object '{name}' must specify at least one directive or input field.", path);
    public static ValidationException ExtendTypeAlreadyDefined(LocationNode node, string name, string type, string[] path) => new(node.Location, $"{type} '{name}' cannot be extended because it is not defined.", path);
    public static ValidationException ExtendExistingFieldUnchanged(SyntaxFieldDefinitionNode node, string[] path) => new(node.Location, $"Field '{node.Name}' has not been changed in extend definition.", path);
    public static ValidationException ExtendExistingInputFieldUnchanged(SyntaxInputValueDefinitionNode node, string[] path) => new(node.Location, $"Input field '{node.Name}' has not been changed in extend definition.", path);


    //-----------------------------------
    // Generated during linker stage
    //-----------------------------------
    public static ValidationException SchemaOperationTypeNotDefined(OperationTypeDefinition node, string[] path) => new(node.Location, $"Schema{node.Operation.ToString().ToLower()} operation type '{node.NamedType}' not defined.", path);
    public static ValidationException SchemaOperationTypeNotObject(OperationTypeDefinition node, DocumentNode type, string[] path) => new(node.Location, $"Schema {node.Operation.ToString().ToLower()} operation '{node.NamedType}' has type {type.OutputElement.ToLower()} instead of object type.", path);
    public static ValidationException UndefinedInterface(Interface node, TypeDefinition parentNode, string[] path) => new(node.Location, $"Undefined interface '{node.OutputName}' defined on {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}'.", path);
    public static ValidationException UndefinedMemberType(MemberType node, string[] path) => new(node.Location, $"Undefined member type '{node.OutputName}'.", path);
    public static ValidationException UndefinedDirective(Directive node, DocumentNode parentNode, string[] path) => new(node.Location, $"Undefined directive '{node.OutputName}' defined on {parentNode.OutputElement.ToLower()}.", path);
    public static ValidationException UndefinedTypeForListEntry(LocationNode node, string type, DocumentNode parentNode, string[] path) => new(node.Location, $"Undefined type '{type}' on {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}'.", path);
    public static ValidationException TypeIsNotAnInterface(Interface node, TypeDefinition parentNode, DocumentNode actualNode, string[] path) => new(node.Location, $"Cannot implement interface '{node.OutputName}' defined on {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}' because it is a '{actualNode.OutputElement.ToLower()}'.", path);
    public static ValidationException TypeIsNotAnObject(MemberType node, DocumentNode actualNode, string article, string[] path) => new(node.Location, $"Cannot reference member type '{node.OutputName}' because it is {article} {actualNode.OutputElement.ToLower()}.", path);
    public static ValidationException UnrecognizedType(DocumentNode node, string[] path) => new(node.Location, $"Unrecognized type '{node.GetType()}' encountered.", path);
    public static ValidationException SchemaDefinitionEmpty(DocumentNode node, string[] path) => new(node.Location, "Schema definition must have at least one operation type.", path);
    public static ValidationException SchemaDefinitionMissingQuery(DocumentNode node, string[] path) => new(node.Location, "Schema definition missing mandatory query operation.", path);
    public static ValidationException SchemaOperationsNotUnique(OperationTypeDefinition left, OperationTypeDefinition right, string[] path) => new(right.Location, $"Schema operations {left.Operation.ToString().ToLower()} and {right.Operation.ToString().ToLower()} cannot have the same '{left.NamedType}' type.", path);
    public static ValidationException AutoSchemaQueryMissing() => new(Location.Empty, "Cannot auto generate schema because 'Query' type missing.", []);
    public static ValidationException AutoSchemaOperationNotObject(TypeDefinition node, string opreration) => new(node.Location, $"Cannot auto generate schema because '{opreration}' is type {node.OutputElement.ToLower()} instead of object type.", []);
    public static ValidationException AutoSchemaOperationReferenced(TypeDefinition node, string opreration) => new(node.Location, $"Cannot auto generate schema because '{opreration}' type is referenced from other types instead of being a top level type.", []);
    public static ValidationException NameDoubleUnderscore(DocumentNode node, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' not allowed to start with two underscores.", path);
    public static ValidationException TypeIsNotAnInputType(DocumentNode node, string[] path) => new(node.Location, $"{node.OutputElement.ToLower()} '{node.OutputName}' is not an input type.", path);
    public static ValidationException TypeIsNotAnOutputType(DocumentNode node, string[] path) => new(node.Location, $"{node.OutputElement.ToLower()} '{node.OutputName}' is not an output type.", path);
    public static ValidationException DefaultValueNotCompatibleInArgument(InputValueDefinition node, string[] path) => new(node.Location, $"Default value not compatible with type of argument '{node.Name}'.", path);
    public static ValidationException AtLeastOne(DocumentNode node, string target, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' must have at least one {target}.", path);
    public static ValidationException NonNullCannotBeDeprecated(DocumentNode node, string[] path) => new(node.Location, $"Cannot use @deprecated directive on non-null {node.OutputElement.ToLower()} '{node.OutputName}'.", path);
    public static ValidationException DirectiveCircularReference(DocumentNode node, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' has circular reference to itself.", path);
    public static ValidationException DirectiveNotAllowedLocation(DocumentNode node, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' is not specified for use at this location.", path);
    public static ValidationException DirectiveNotRepeatable(DocumentNode node, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' is not repeatable but has been applied multiple times.", path);
    public static ValidationException DirectiveMandatoryArgumentMissing(DocumentNode node, string argumentName, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' has mandatory argument '{argumentName}' missing.", path);
    public static ValidationException DirectiveArgumentNotDefined(DocumentNode node, string argumentName, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' does not define argument '{argumentName}'.", path);
    public static ValidationException InterfaceCannotImplmentOwnInterface(DocumentNode node, string[] path) => new(node.Location, $"Interface '{node.OutputName}' cannnot implement itself.", path);
    public static ValidationException TypeMissingImplements(DocumentNode node, string implementsName, string interfaceName, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' is missing implements '{implementsName}' because it is declared on interface '{interfaceName}'.", path);
    public static ValidationException TypeMissingFieldFromInterface(DocumentNode node, string fieldName, string interfaceName, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' is missing field '{fieldName}' declared on interface '{interfaceName}'.", path);
    public static ValidationException TypeMissingFieldArgumentFromInterface(DocumentNode node, string fieldName, string interfaceName, string argumentName, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' field '{fieldName}' is missing argument '{argumentName}' declared on interface '{interfaceName}'.", path);
    public static ValidationException TypeFieldArgumentNonNullFromInterface(DocumentNode node, string fieldName, string interfaceName, string argumentName, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' field '{fieldName}' argument '{argumentName}' cannot be non-null type because not declared on interface '{interfaceName}'.", path);
    public static ValidationException TypeFieldArgumentTypeFromInterface(DocumentNode node, string fieldName, string interfaceName, string argumentName, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' field '{fieldName}' argument '{argumentName}' has different type to the declared interface '{interfaceName}'.", path);
    public static ValidationException TypeFieldReturnNotCompatibleFromInterface(DocumentNode node, string fieldName, string interfaceName, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' field '{fieldName}' return type not a sub-type of matching field on interface '{interfaceName}'.", path);
    public static ValidationException InputObjectCircularReference(DocumentNode node, string[] path) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' has circular reference requiring a non-null value.", path);
    public static ValidationException UndefinedTypeForFragment(FragmentDefinition fragment, string[] path) => new(fragment.Location, $"Undefined type '{fragment.TypeCondition}' specified for fragment '{fragment.Name}'.", path);
    public static ValidationException FragmentTypeInvalid(FragmentDefinition fragment, TypeDefinition targetType, string[] path) => new(fragment.Location, $"Fragment '{fragment.Name}' cannot be applied to {targetType.OutputElement.ToLower()} '{targetType.OutputName}' only an object, interface or union.", path);
    public static ValidationException UndefinedTypeForInlineFragment(SelectionInlineFragment inline, DocumentNode rootNode, string[] path) => new(inline.Location, $"Undefined type '{inline.TypeCondition}' specified for inline fragment within {rootNode.OutputElement.ToLower()} '{rootNode.OutputName}'.", path);
    public static ValidationException UndefinedTypeForFragmentSpread(SelectionFragmentSpread spread, DocumentNode rootNode, string[] path) => new(spread.Location, $"Undefined type '{spread.Name}' specified for fragment spread within {rootNode.OutputElement.ToLower()} '{rootNode.OutputName}'.", path);
    public static ValidationException SchemaNotValidated() => new(Location.Empty, "Provided schema has not been validated.", []);
    public static ValidationException CannotSerializeInvalidSchema() => new(Location.Empty, "Cannot serialize a schema that is not validated.", []);

}



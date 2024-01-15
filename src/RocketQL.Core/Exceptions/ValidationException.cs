namespace RocketQL.Core.Exceptions;

public class ValidationException(Location location, string message) : RocketException(location, message)
{
    public static ValidationException UnrecognizedType(Location location, string name) => new(location, $"Unrecognized type '{name}' encountered.");
    public static ValidationException UnrecognizedType(SyntaxNode node) => new(node.Location, $"Unrecognized type '{node.GetType()}' encountered.");
    public static ValidationException UnrecognizedType(DocumentNode node) => new(node.Location, $"Unrecognized type '{node.GetType()}' encountered.");
    public static ValidationException SchemaDefinitionAlreadyDefined(Location location) => new(location, $"Schema is already defined.");
    public static ValidationException SchemaDefinitionEmpty(DocumentNode node) => new(node.Location, "Schema definition must have at least one operation type.");
    public static ValidationException SchemaDefinitionMissingQuery(DocumentNode node) => new(node.Location, $"Schema definition missing mandatory query operation.");
    public static ValidationException SchemaDefinitionMultipleOperation(Location location, OperationType operation) => new(location, $"Schema defines the {operation.ToString().ToLower()} operation more than once.");
    public static ValidationException SchemaOperationsNotUnique(OperationTypeDefinition left, OperationTypeDefinition right) => new(right.Location, $"Schema operations {left.Operation.ToString().ToLower()} and {right.Operation.ToString().ToLower()} cannot have the same '{left.NamedType}' type.");
    public static ValidationException SchemaOperationTypeNotObject(OperationTypeDefinition node, DocumentNode type) => new(node.Location, $"Schema operation {node.Operation.ToString().ToLower()} '{node.NamedType}' has type {type.OutputElement.ToLower()} instead of object type.");
    public static ValidationException AutoSchemaQueryMissing() => new(new Location(), "Cannot auto generate schema because 'Query' type missing.");
    public static ValidationException AutoSchemaOperationNotObject(TypeDefinition node, string opreration) => new(node.Location, $"Cannot auto generate schema because '{opreration}' is type {node.OutputElement.ToLower()} instead of object type.");
    public static ValidationException AutoSchemaOperationReferenced(TypeDefinition node, string opreration) => new(node.Location, $"Cannot auto generate schema because '{opreration}' type is referenced from other types instead of being a top level type.");
    public static ValidationException ExtendSchemaNotDefined(Location location) => new(location, $"Cannot extend schema because non is currently defined.");
    public static ValidationException TypeNotDefinedForSchemaOperation(OperationTypeDefinition node) => new(node.Location, $"Type '{node.NamedType}' not defined for the schema operation {node.Operation.ToString().ToLower()}.");
    public static ValidationException NameDoubleUnderscore(DocumentNode node) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' not allowed to start with two underscores.");
    public static ValidationException NameAlreadyDefined(Location location, string target, string name) => new(location, $"{target} '{name}' is already defined.");
    public static ValidationException SchemaNotDefinedForExtend(Location location) => new(location, $"Extend schema cannot be applied because no schema has been defined.");
    public static ValidationException TypeNotDefinedForExtend(Location location, string typeName, string name) => new(location, $"{typeName} '{name}' cannot be extended because it is not defined.");
    public static ValidationException IncorrectTypeForExtend(DocumentNode node, string typeName) => new(node.Location, $"Extend {typeName.ToLower()} cannot be used to extend {node.OutputElement} '{node.OutputName}'.");    
    public static ValidationException DuplicateMemberType(Location location, string memberType, string unionTypeName) => new(location, $"Union '{unionTypeName}' specifies duplicate member type '{memberType}'.");
    public static ValidationException EnumValueAlreadyDefined(Location location, string enumValue, string enumTypeName) => new(location, $"Enum '{enumTypeName}' has duplicate definition of value '{enumValue}'.");
    public static ValidationException ListEntryDuplicateName(Location location, string list, string entryName, string entryType, string name) => new(location, $"{list} '{entryName}' has duplicate {entryType} '{name}'.");
    public static ValidationException ListEntryDuplicateName(Location location, string type, string typeName, string list, string entryName, string entryType, string name) => new(location, $"{type} '{typeName}' has {list.ToLower()} '{entryName}' with duplicate {entryType} '{name}'.");
    public static ValidationException ListEntryDoubleUnderscore(Location location, string list, string entryName, string entryType, string name) => new(location, $"{list} '{entryName}' has {entryType.ToLower()} '{name}' not allowed to start with two underscores.");
    public static ValidationException ListEntryDoubleUnderscore(Location location, string type,  string typeName, string list, string entryName, string entryType, string name) => new(location, $"{type} '{typeName}' has {list.ToLower()} '{entryName}' with {entryType.ToLower()} '{name}' not allowed to start with two underscores.");
    public static ValidationException UndefinedTypeForListEntry(Location location, string type, string listNodeElement, string listNodeName, DocumentNode parentNode) => new(location, $"Undefined type '{type}' for {listNodeElement.ToLower()} '{listNodeName}' of {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}'.");
    public static ValidationException UndefinedDirective(DocumentNode node, DocumentNode parentNode) => new(node.Location, $"Undefined directive '{node.OutputName}' defined on {parentNode.OutputElement.ToLower()}{OptionalQuotedName(parentNode)}.");
    public static ValidationException UndefinedDirective(DocumentNode node, string parentNodeElement, string parentNodeName, DocumentNode grandParentNode) => new(node.Location, $"Undefined directive '{node.OutputName}' defined on {parentNodeElement.ToLower()} '{parentNodeName}' of {grandParentNode.OutputElement.ToLower()} '{grandParentNode.OutputName}'.");
    public static ValidationException UndefinedInterface(DocumentNode node, DocumentNode parentNode) => new(node.Location, $"Undefined interface '{node.OutputName}' defined on {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}'.");
    public static ValidationException InterfaceCannotImplmentOwnInterface(DocumentNode node) => new(node.Location, $"Interface '{node.OutputName}' cannnot implement itself.");
    public static ValidationException UndefinedMemberType(DocumentNode node, DocumentNode parentNode) => new(node.Location, $"Undefined member type '{node.OutputName}' defined on {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}'.");
    public static ValidationException AtLeastOne(DocumentNode node, string target) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' must have at least one {target}.");
    public static ValidationException TypeIsNotAnInterface(DocumentNode node, DocumentNode parentNode, DocumentNode actualNode) => new(node.Location, $"Cannot implement interface '{node.OutputName}' defined on {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}' because it is a '{actualNode.OutputElement.ToLower()}'.");
    public static ValidationException TypeIsNotAnObject(DocumentNode node, DocumentNode parentNode, DocumentNode actualNode, string article) => new(node.Location, $"Cannot reference member type '{node.OutputName}' defined on {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}' because it is {article} {actualNode.OutputElement.ToLower()}.");
    public static ValidationException TypeIsNotAnOutputType(DocumentNode node, DocumentNode parentNode, string errorType) => new(node.Location, $"{parentNode.OutputElement} '{parentNode.OutputName}' has {node.OutputElement.ToLower()} '{node.OutputName}' with type '{errorType}' that is not an output type.");
    public static ValidationException TypeIsNotAnInputType(DocumentNode node, DocumentNode parentNode, DocumentNode argumentNode, string errorType) => new(node.Location, $"{parentNode.OutputElement} '{parentNode.OutputName}' has {node.OutputElement.ToLower()} '{node.OutputName}' with argument '{argumentNode.OutputName}' of type '{errorType}' that is not an input type.");
    public static ValidationException TypeIsNotAnInputType(DocumentNode node, DocumentNode parentNode, string errorType) => new(node.Location, $"{parentNode.OutputElement} '{parentNode.OutputName}' has {node.OutputElement.ToLower()} '{node.OutputName}' of type '{errorType}' that is not an input type.");
    public static ValidationException ValueNotCompatibleWithType(DocumentNode node, DocumentNode parentNode, DocumentNode argumentNode) => new(node.Location, $"{argumentNode.OutputElement} '{argumentNode.OutputName}' of {node.OutputElement.ToLower()} '{node.OutputName}' of {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}' has a default value incompatible with the type.");
    public static ValidationException ValueNotCompatibleWithType(DocumentNode node, DocumentNode argumentNode) => new(node.Location, $"{argumentNode.OutputElement} '{argumentNode.OutputName}' of {node.OutputElement.ToLower()} '{node.OutputName}' has a default value incompatible with the type.");
    public static ValidationException NonNullArgumentCannotBeDeprecated(DocumentNode node, DocumentNode parentNode, DocumentNode argumentNode) => new(node.Location, $"Cannot use @deprecated directive on non-null {argumentNode.OutputElement.ToLower()} '{argumentNode.OutputName}' of {node.OutputElement.ToLower()} '{node.OutputName}' of {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}'.");
    public static ValidationException NonNullFieldCannotBeDeprecated(DocumentNode node, DocumentNode parentNode) => new(node.Location, $"Cannot use @deprecated directive on non-null {node.OutputElement.ToLower()} '{node.OutputName}' of {parentNode.OutputElement.ToLower()} '{parentNode.OutputName}'.");
    public static ValidationException TypeMissingImplements(DocumentNode node, string implementsName, string interfaceName) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' is missing implements '{implementsName}' because it is declared on interface '{interfaceName}'.");
    public static ValidationException TypeMissingFieldFromInterface(DocumentNode node, string fieldName, string interfaceName) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' is missing field '{fieldName}' declared on interface '{interfaceName}'.");
    public static ValidationException TypeMissingFieldArgumentFromInterface(DocumentNode node, string fieldName, string interfaceName, string argumentName) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' field '{fieldName}' is missing argument '{argumentName}' declared on interface '{interfaceName}'.");
    public static ValidationException TypeFieldArgumentTypeFromInterface(DocumentNode node, string fieldName, string interfaceName, string argumentName) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' field '{fieldName}' argument '{argumentName}' has different type to the declared interface '{interfaceName}'.");
    public static ValidationException TypeFieldArgumentNonNullFromInterface(DocumentNode node, string fieldName, string interfaceName, string argumentName) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' field '{fieldName}' argument '{argumentName}' cannot be non-null type because not declared on interface '{interfaceName}'.");
    public static ValidationException TypeFieldReturnNotCompatibleFromInterface(DocumentNode node, string fieldName, string interfaceName) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' field '{fieldName}' return type not a sub-type of matching field on interface '{interfaceName}'.");
    public static ValidationException InputObjectCircularReference(DocumentNode node) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' has circular reference requiring a non-null value.");
    public static ValidationException DirectiveCircularReference(DocumentNode node) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' has circular reference to itself.");
    public static ValidationException DirectiveNotAllowedLocation(DocumentNode node, DocumentNode parentNode, params DocumentNode?[] extraNodes) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' is not specified for use on {ExpandNodesOf(extraNodes)}{parentNode.OutputElement.ToLower()}{OptionalQuotedName(parentNode)} location.");
    public static ValidationException DirectiveNotRepeatable(DocumentNode node, DocumentNode parentNode, params DocumentNode?[] extraNodes) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' is not repeatable but has been applied multiple times on {ExpandNodesOf(extraNodes)}{parentNode.OutputElement.ToLower()}{OptionalQuotedName(parentNode)}.");
    public static ValidationException DirectiveArgumentNotDefined(DocumentNode node, string argumentName, DocumentNode parentNode, params DocumentNode?[] extraNodes) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' does not define argument '{argumentName}' provided on {ExpandNodesOf(extraNodes)}{parentNode.OutputElement.ToLower()} '{parentNode.OutputName}'.");
    public static ValidationException DirectiveMandatoryArgumentMissing(DocumentNode node, string argumentName, DocumentNode parentNode, params DocumentNode?[] extraNodes) => new(node.Location, $"{node.OutputElement} '{node.OutputName}' has mandatory argument '{argumentName}' missing on {ExpandNodesOf(extraNodes)}{parentNode.OutputElement.ToLower()} '{parentNode.OutputName}'.");
    public static ValidationException ExtendSchemaMandatory(Location location) => new(location, $"Extend schema 'must specify at least one directive or operation.");
    public static ValidationException ExtendScalarMandatory(Location location, string name) => new(location, $"Extend scalar '{name}' must specify at least one directive.");
    public static ValidationException ExtendObjectInterfaceMandatory(Location location, string typeName, string name) => new(location, $"Extend {typeName.ToLower()} '{name}' must specify at least one implements, directive or field.");
    public static ValidationException ExtendInputObjectMandatory(Location location, string typeName, string name) => new(location, $"Extend {typeName.ToLower()} '{name}' must specify at least one directive or input field.");
    public static ValidationException ExtendSchemaOperationAlreadyDefined(Location location, OperationType operation) => new(location, $"Extend schema cannot add operation {operation} because it is already defined.");
    public static ValidationException ExtendImplementAlreadyDefined(Location location, string objectType, string objectName, string interfaceType) => new(location, $"{objectType} '{objectName}' specifies an interface '{interfaceType}' already defined.");
    public static ValidationException ExtendFieldAlreadyDefined(Location location, string fieldName, string objectType, string objectTypeName) => new(location, $"{objectType} '{objectTypeName}' has duplicate definition of field '{fieldName}'.");
    public static ValidationException ExtendInputFieldAlreadyDefined(Location location, string fieldName, string objectType, string objectTypeName) => new(location, $"{objectType} '{objectTypeName}' has duplicate definition of input field '{fieldName}'.");
    public static ValidationException ExtendFieldArgumentAlreadyDefined(Location location, string fieldName, string argumentName, string objectType, string objectTypeName) => new(location, $"{objectType}  '{objectTypeName}' has duplicate definition of argument '{argumentName}' for field '{fieldName}'.");
    public static ValidationException ExtendUnionMandatory(Location location, string name) => new(location, $"Extend union '{name}' must specify at least one directive or type.");
    public static ValidationException ExtendEnumMandatory(Location location, string name) => new(location, $"Extend enum '{name}' must specify at least one directive or field.");
    public static ValidationException ExtendEnumValueAlreadyDefined(Location location, string enumValue, string enumTypeName) => new(location, $"Extend enum '{enumTypeName}' has duplicate definition of enum value '{enumValue}'.");
    public static ValidationException ExtendUnionAlreadyDefined(Location location, string memberType, string unionTypeName) => new(location, $"Extend union '{unionTypeName}' specifies a type '{memberType}' already defined.");
    public static ValidationException ExtendExistingFieldUnchanged(Location location, string fieldName, string objectType, string objectTypeName) => new(location, $"{objectType} '{objectTypeName}' for existing field '{fieldName}' does not make any change.");
    public static ValidationException ExtendExistingInputFieldUnchanged(Location location, string fieldName, string objectType, string objectTypeName) => new(location, $"{objectType} '{objectTypeName}' for existing input field '{fieldName}' does not make any change.");
    public static ValidationException ExtendExistingEnumValueUnchanged(Location location, string enumValue, string enumTypeName) => new(location, $"Extend enum '{enumTypeName}' for existing enum value '{enumValue}' does not make any change.");
    public static ValidationException CannotSerializeInvalidSchema() => new(new(), "Cannot serialize a schema that is not validated.");
    public static ValidationException SchemaNotValidated() => new(new(), "Provided schema has not been validated.");
    public static ValidationException SchemaDefinitionIgnored(Location location, string definition) => new(location, $"{definition} definition not allowed in a schema.");
    public static ValidationException RequestDefinitionIgnored(Location location, string definition) => new(location, $"{definition} definition not allowed in a request.");
    public static ValidationException RequestAnonymousAlreadyDefined(Location location) => new(location, $"Anonymous operation is already defined.");
    public static ValidationException RequestAnonymousAndNamed(Location location) => new(location, $"Anonymous operation and named operation both defined.");
    public static ValidationException RequestOperationAlreadyDefined(Location location, string name) => new(location, $"Operation name '{name}' is already defined.");
    public static ValidationException DuplicateOperationVariable(Location location, string operationName, string variable) => new(location, $"{operationName} has duplicate variable '{variable}'.");
    public static ValidationException UndefinedTypeForFragment(FragmentDefinition fragment) => new(fragment.Location, $"Undefined type '{fragment.TypeCondition}' specified for fragment '{fragment.Name}'.");
    public static ValidationException FragmentTypeInvalid(FragmentDefinition fragment, TypeDefinition targetType) => new(fragment.Location, $"Fragment '{fragment.Name}' cannot be applied to {targetType.OutputElement.ToLower()} '{targetType.OutputName}' only an object, interface or union.");

    

    private static string OptionalQuotedName(DocumentNode node)
    {
        if (string.IsNullOrWhiteSpace(node.OutputName))
            return "";
        else
            return $" '{node.OutputName}'";
    }

    private static string ExpandNodesOf(DocumentNode?[] extraNodes)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var extraNode in extraNodes.Reverse())
            if (extraNode is not null)
                sb.Append($"{extraNode.OutputElement.ToLower()} '{extraNode.OutputName}' of ");

        return sb.ToString();
    }
}



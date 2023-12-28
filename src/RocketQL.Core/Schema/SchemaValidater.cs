using System.Linq;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private class SchemaValidater(Schema schema) : ISchemaNodeVisitors
    {
        private readonly Schema _schema = schema;

        public void Visit()
        {
            ISchemaNodeVisitors visitor = this;
            visitor.Visit(_schema.Directives.Values);
            visitor.Visit(_schema.Types.Values);
            visitor.Visit(_schema.Schemas);
        }

        public void VisitDirectiveDefinition(DirectiveDefinition directiveDefinition)
        {
            if (directiveDefinition.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(directiveDefinition);

            foreach (var argumentDefinition in directiveDefinition.Arguments.Values)
            {
                if (argumentDefinition.Name.StartsWith("__"))
                    throw ValidationException.ListEntryDoubleUnderscore(argumentDefinition.Location, 
                                                                        directiveDefinition.OutputElement, directiveDefinition.OutputName, 
                                                                        argumentDefinition.OutputElement, argumentDefinition.Name);
            }
        }

        public void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType)
        {
            if (scalarType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(scalarType);
        }

        public void VisitObjectTypeDefinition(ObjectTypeDefinition objectType)
        {
            if (objectType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(objectType);

            VisitFieldDefinintions(objectType.Fields.Values, objectType, true);
            IsValidImplementations(objectType.Fields, CheckTypeImplementsInterfaces(objectType.ImplementsInterfaces, objectType, true), objectType);
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            if (interfaceType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(interfaceType);

            VisitFieldDefinintions(interfaceType.Fields.Values, interfaceType, false);
            IsValidImplementations(interfaceType.Fields, CheckTypeImplementsInterfaces(interfaceType.ImplementsInterfaces, interfaceType, false), interfaceType);
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
            if (unionType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(unionType);
        }

        public void VisitEnumTypeDefinition(EnumTypeDefinition enumType)
        {
            if (enumType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(enumType);
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
            if (inputObjectType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(inputObjectType);

            foreach (var fieldDefinition in inputObjectType.InputFields.Values)
                if (fieldDefinition.Name.StartsWith("__"))
                    throw ValidationException.ListEntryDoubleUnderscore(fieldDefinition.Location, 
                                                                        inputObjectType.OutputElement, inputObjectType.OutputName, 
                                                                        "Field", fieldDefinition.Name);
        }

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
        }

        private static void VisitFieldDefinintions(IEnumerable<FieldDefinition> fieldDefinitions, SchemaNode parentNode, bool isObject)
        {
            foreach (var fieldDefinition in fieldDefinitions)
            {
                if (fieldDefinition.Name.StartsWith("__"))
                    throw ValidationException.ListEntryDoubleUnderscore(fieldDefinition.Location, 
                                                                        parentNode.OutputElement, parentNode.OutputName, 
                                                                        fieldDefinition.OutputElement, fieldDefinition.Name);

                if (fieldDefinition.Type.Definition is null)
                    throw ValidationException.UnrecognizedType(fieldDefinition.Location, fieldDefinition.Name);

                if (!fieldDefinition.Type.Definition.IsOutputType)
                    throw ValidationException.TypeIsNotAnOutputType(fieldDefinition, parentNode, fieldDefinition.Type.Definition.OutputName);

                foreach (var argumentDefinition in fieldDefinition.Arguments.Values)
                {
                    if (argumentDefinition.Name.StartsWith("__"))
                        throw ValidationException.ListEntryDoubleUnderscore(argumentDefinition.Location, 
                                                                            parentNode.OutputElement, parentNode.OutputName, 
                                                                            fieldDefinition.OutputElement, fieldDefinition.OutputName, 
                                                                            argumentDefinition.OutputElement, argumentDefinition.Name);

                    if (argumentDefinition.Type.Definition is null)
                        throw ValidationException.UnrecognizedType(argumentDefinition.Location, argumentDefinition.Name);

                    if (!argumentDefinition.Type.Definition.IsOutputType)
                        throw ValidationException.TypeIsNotAnInputType(fieldDefinition, parentNode, argumentDefinition, argumentDefinition.Type.Definition.OutputName);

                    if (isObject && argumentDefinition.Type.NonNull && (argumentDefinition.DefaultValue is null) && argumentDefinition.Directives.ContainsKey("deprecated"))
                        throw ValidationException.NonNullArgumentCannotBeDeprecated(fieldDefinition, parentNode, argumentDefinition);
                }
            }
        }

        private InterfaceTypeDefinitions CheckTypeImplementsInterfaces(Interfaces implementsInterfaces, SchemaNode parentNode, bool isObject)
        {
            InterfaceTypeDefinitions interfaceDefinitions = [];
            foreach (var interfaceEntry in implementsInterfaces.Values)
            {
                if (!isObject && (interfaceEntry.Name == parentNode.OutputName))
                    throw ValidationException.InterfaceCannotImplmentOwnInterface(interfaceEntry);

                if (!_schema.Types.TryGetValue(interfaceEntry.Name, out TypeDefinition? typeDefinition))
                    throw ValidationException.UndefinedInterface(interfaceEntry, parentNode);

                if (typeDefinition is not InterfaceTypeDefinition interfaceTypeDefinition)
                    throw ValidationException.TypeIsNotAnInterface(interfaceEntry, parentNode, typeDefinition);

                interfaceDefinitions.Add(interfaceTypeDefinition.Name, interfaceTypeDefinition);
            }

            HashSet<string> processed = [];
            foreach (var objectImplement in interfaceDefinitions)
                CheckInterfaceImplemented(interfaceDefinitions, processed, objectImplement.Value, parentNode, parentNode);

            return interfaceDefinitions;
        }

        private void CheckInterfaceImplemented(InterfaceTypeDefinitions objectImplements,
                                               HashSet<string> processed,
                                               InterfaceTypeDefinition checkInterface,
                                               SchemaNode parentNode,
                                               SchemaNode rootNode)
        {
            if (!processed.Contains(checkInterface.Name))
            {
                processed.Add(checkInterface.Name);

                if (!_schema.Types.TryGetValue(checkInterface.Name, out TypeDefinition? typeDefinition))
                    throw ValidationException.UndefinedInterface(checkInterface, parentNode);

                if (typeDefinition is not InterfaceTypeDefinition interfaceTypeDefinition)
                    throw ValidationException.TypeIsNotAnInterface(checkInterface, parentNode, typeDefinition);

                foreach (var implementsInterface in interfaceTypeDefinition.ImplementsInterfaces.Values)
                {
                    if (!objectImplements.ContainsKey(implementsInterface.Name))
                        throw ValidationException.TypeMissingImplements(rootNode, implementsInterface.Name, checkInterface.Name);

                    CheckInterfaceImplemented(objectImplements, processed, interfaceTypeDefinition, checkInterface, rootNode);
                }
            }
        }
        

        private static void IsValidImplementations(FieldDefinitions objectFields, InterfaceTypeDefinitions interfaceDefinitions, SchemaNode parentNode)
        {
            foreach(var interfaceDefinition in interfaceDefinitions.Values)
            {
                foreach(var interfaceField in interfaceDefinition.Fields.Values)
                {
                    if (!objectFields.TryGetValue(interfaceField.Name, out FieldDefinition? objectFieldDefinition))
                        throw ValidationException.TypeMissingFieldFromInterface(parentNode, interfaceField.Name, interfaceDefinition.Name);

                    var nonInterface = objectFieldDefinition.Arguments.ToDictionary();

                    foreach (var argument in interfaceField.Arguments.Values)
                    {
                        if (!objectFieldDefinition.Arguments.TryGetValue(argument.Name, out var objectFieldArgument))
                            throw ValidationException.TypeMissingFieldArgumentFromInterface(parentNode, interfaceField.Name, interfaceDefinition.Name, argument.Name);

                        if (!IsSameType(objectFieldArgument.Type, argument.Type))
                            throw ValidationException.TypeFieldArgumentTypeFromInterface(parentNode, interfaceField.Name, interfaceDefinition.Name, argument.Name);

                        nonInterface.Remove(argument.Name);
                    }

                    foreach(var nonInterfaceArgument in nonInterface.Values)
                        if (nonInterfaceArgument.Type.NonNull)
                            throw ValidationException.TypeFieldArgumentNonNullFromInterface(parentNode, interfaceField.Name, interfaceDefinition.Name, nonInterfaceArgument.Name);

                    if (!IsValidImplementationFieldType(objectFieldDefinition.Type, interfaceField.Type))
                        throw ValidationException.TypeFieldReturnNotCompatibleFromInterface(parentNode, interfaceField.Name, interfaceDefinition.Name);
                }
            }
        }

        private static bool IsValidImplementationFieldType(TypeNode fieldType, TypeNode implementedType)
        {
            if (fieldType.NonNull)
            {
                return IsValidImplementationFieldType(fieldType.Clone(nonNull: false), implementedType.Clone(nonNull: false));
            }
            else if ((fieldType is TypeList fieldTypeList) && (implementedType is TypeList implementedTypeList)) 
            {
                return IsValidImplementationFieldType(fieldTypeList.Type, implementedTypeList.Type);
            }

            return IsSubType(fieldType, implementedType);
        }

        private static bool IsSubType(TypeNode possibleSubType, TypeNode superType)
        {
            if (IsSameType(possibleSubType, superType))
                return true;

            if ((possibleSubType.Definition is ObjectTypeDefinition possibleObject) && (superType.Definition is UnionTypeDefinition superUnion))
            {
                foreach (var memberType in superUnion.MemberTypes)
                    if (memberType.Value.Definition == possibleObject)
                        return true;
            }

            if (superType.Definition is InterfaceTypeDefinition superInterfaceType)
            {
                Interfaces? possibleInterfaces = null;
                if (possibleSubType.Definition is ObjectTypeDefinition possibleObjectType)
                    possibleInterfaces = possibleObjectType.ImplementsInterfaces;
                else if (possibleSubType.Definition is InterfaceTypeDefinition possibleInterfaceType)
                    possibleInterfaces = possibleInterfaceType.ImplementsInterfaces;

                if ((possibleInterfaces is not null) && possibleInterfaces.ContainsKey(superInterfaceType.Name))
                    return true;
            }

            return false;
        }

        private static bool IsSameType(TypeNode left, TypeNode right)
        {
            if ((left is TypeName leftTypeName) && (right is TypeName rightTypeName))
            {
                return (leftTypeName.NonNull == rightTypeName.NonNull) &&
                        (leftTypeName.Definition == rightTypeName.Definition);
            }
            else if ((left is TypeList leftTypeList) && (right is TypeList rightTypeList))
            {
                return (leftTypeList.NonNull == rightTypeList.NonNull) &&
                        IsSameType(leftTypeList.Type, rightTypeList.Type);
            }

            return false;
        }
    }
}
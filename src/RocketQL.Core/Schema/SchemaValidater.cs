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

            Queue<TypeDefinition> referencedTypes = [];
            Queue<DirectiveDefinition> referencedDirectives = [];
            foreach (var argumentDefinition in directiveDefinition.Arguments.Values)
            {
                if (argumentDefinition.Name.StartsWith("__"))
                    throw ValidationException.ListEntryDoubleUnderscore(argumentDefinition.Location, 
                                                                        directiveDefinition.OutputElement, directiveDefinition.OutputName, 
                                                                        argumentDefinition.OutputElement, argumentDefinition.Name);

                if (argumentDefinition.Type.Definition is null)
                    throw ValidationException.UnrecognizedType(argumentDefinition.Location, argumentDefinition.Name);

                if (!argumentDefinition.Type.Definition.IsInputType)
                    throw ValidationException.TypeIsNotAnInputType(argumentDefinition, directiveDefinition, argumentDefinition.Type.Definition.OutputName);

                referencedTypes.Enqueue(argumentDefinition.Type.Definition);
                foreach (var directive in argumentDefinition.Directives)
                    referencedDirectives.Enqueue(directive.Definition!);

                CheckDirectiveUsage(argumentDefinition.Directives, directiveDefinition, DirectiveLocations.ARGUMENT_DEFINITION, argumentDefinition);
            }

            CheckDirectiveForCircularReference(directiveDefinition, referencedTypes, referencedDirectives);
        }

        public void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType)
        {
            scalarType.CheckDoubleUnderscore();
            CheckDirectiveUsage(scalarType.Directives, scalarType, DirectiveLocations.SCALAR);
        }

        public void VisitObjectTypeDefinition(ObjectTypeDefinition objectType)
        {
            objectType.CheckDoubleUnderscore();
            CheckDirectiveUsage(objectType.Directives, objectType, DirectiveLocations.OBJECT);

            if (objectType.Fields.Count == 0)
                throw ValidationException.AtLeastOne(objectType, "field");

            VisitFieldDefinintions(objectType.Fields.Values, objectType, isObject: true);
            IsValidImplementations(objectType.Fields, 
                                   CheckTypeImplementsInterfaces(objectType.ImplementsInterfaces, objectType, isObject: true), 
                                   objectType);
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            interfaceType.CheckDoubleUnderscore();
            CheckDirectiveUsage(interfaceType.Directives, interfaceType, DirectiveLocations.INTERFACE);

            if (interfaceType.Fields.Count == 0)
                throw ValidationException.AtLeastOne(interfaceType, "field");

            VisitFieldDefinintions(interfaceType.Fields.Values, interfaceType, isObject: false);
            IsValidImplementations(interfaceType.Fields, 
                                   CheckTypeImplementsInterfaces(interfaceType.ImplementsInterfaces, interfaceType, isObject: false), 
                                   interfaceType);
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
            unionType.CheckDoubleUnderscore();
            CheckDirectiveUsage(unionType.Directives, unionType, DirectiveLocations.UNION);
        }

        public void VisitEnumTypeDefinition(EnumTypeDefinition enumType)
        {
            enumType.CheckDoubleUnderscore();
            CheckDirectiveUsage(enumType.Directives, enumType, DirectiveLocations.ENUM);

            if (enumType.EnumValues.Count == 0)
                throw ValidationException.AtLeastOne(enumType, "enum value");

            foreach (var enumValueDefinition in enumType.EnumValues.Values)
                CheckDirectiveUsage(enumValueDefinition.Directives, enumType, DirectiveLocations.ENUM_VALUE, enumValueDefinition);
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
            inputObjectType.CheckDoubleUnderscore();
            CheckDirectiveUsage(inputObjectType.Directives, inputObjectType, DirectiveLocations.INPUT_OBJECT);

            if (inputObjectType.InputFields.Count == 0)
                throw ValidationException.AtLeastOne(inputObjectType, "input field");

            Queue<InputObjectTypeDefinition> referencedInputObjects = [];
            foreach (var fieldDefinition in inputObjectType.InputFields.Values)
            {
                if (fieldDefinition.Name.StartsWith("__"))
                    throw ValidationException.ListEntryDoubleUnderscore(fieldDefinition.Location,
                                                                        inputObjectType.OutputElement, inputObjectType.OutputName,
                                                                        "Field", fieldDefinition.Name);

                if (fieldDefinition.Type.Definition is null)
                    throw ValidationException.UnrecognizedType(fieldDefinition.Location, fieldDefinition.Name);

                if (!fieldDefinition.Type.Definition.IsInputType)
                    throw ValidationException.TypeIsNotAnInputType(fieldDefinition, inputObjectType, fieldDefinition.Type.Definition.OutputName);

                if (fieldDefinition.Type.NonNull && (fieldDefinition.DefaultValue is null) && fieldDefinition.Directives.Where(d => d.Name == "deprecated").Any())
                    throw ValidationException.NonNullFieldCannotBeDeprecated(fieldDefinition, inputObjectType);

                if ((fieldDefinition.Type.NonNull && (fieldDefinition.Type is TypeName)) && (fieldDefinition.Type.Definition is InputObjectTypeDefinition referenceInputObject))
                    referencedInputObjects.Enqueue(referenceInputObject);

                CheckDirectiveUsage(fieldDefinition.Directives, inputObjectType, DirectiveLocations.INPUT_FIELD_DEFINITION, fieldDefinition);
            }

            CheckInputObjectForCircularReference(inputObjectType, referencedInputObjects);
        }

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
            if (schemaDefinition.Operations.Count == 0)
                throw ValidationException.SchemaDefinitionEmpty(schemaDefinition);

            schemaDefinition.Operations.TryGetValue(OperationType.QUERY, out var query);
            schemaDefinition.Operations.TryGetValue(OperationType.MUTATION, out var mutation);
            schemaDefinition.Operations.TryGetValue(OperationType.SUBSCRIPTION, out var subscription);

            if (query is null)
                throw ValidationException.SchemaDefinitionMissingQuery(schemaDefinition);

            if (query.Definition is not ObjectTypeDefinition)
                throw ValidationException.SchemaOperationTypeNotObject(query, query.Definition!);

            if (mutation is not null)
            {
                if (mutation.Definition is not ObjectTypeDefinition)
                    throw ValidationException.SchemaOperationTypeNotObject(mutation, mutation.Definition!);
    
                if (mutation.Definition == query.Definition)
                    throw ValidationException.SchemaOperationsNotUnique(query, mutation);
            }

            if (subscription is not null)
            {
                if (subscription.Definition is not ObjectTypeDefinition)
                    throw ValidationException.SchemaOperationTypeNotObject(subscription, subscription.Definition!);

                if (subscription.Definition == query.Definition)
                    throw ValidationException.SchemaOperationsNotUnique(query, subscription);
            }

            if ((mutation is not null) && (subscription is not null) && (mutation.Definition == subscription.Definition))
                throw ValidationException.SchemaOperationsNotUnique(mutation, subscription);

            CheckDirectiveUsage(schemaDefinition.Directives, schemaDefinition, DirectiveLocations.SCHEMA);
        }

        private static void CheckDirectiveForCircularReference(DirectiveDefinition directiveDefinition,
                                                               Queue<TypeDefinition> referencedTypes,
                                                               Queue<DirectiveDefinition> referencedDirectives)
        {
            if ((referencedDirectives.Count > 0) || (referencedTypes.Count > 0))
            {
                HashSet<TypeDefinition> checkedTypes = [];
                HashSet<DirectiveDefinition> checkedDirectives = [];

                while ((referencedDirectives.Count > 0) || (referencedTypes.Count > 0))
                {
                    while (referencedDirectives.TryDequeue(out var referencedDirective))
                    {
                        if (referencedDirective == directiveDefinition)
                            throw ValidationException.DirectiveCircularReference(directiveDefinition);

                        foreach (var argumentDefinition in referencedDirective.Arguments.Values)
                        {
                            if (!checkedTypes.Contains(argumentDefinition.Type.Definition!))
                                referencedTypes.Enqueue(argumentDefinition.Type.Definition!);

                            FindDirectives(argumentDefinition.Directives, checkedDirectives, referencedDirectives);
                        }

                        checkedDirectives.Add(referencedDirective);
                    }

                    while (referencedTypes.TryDequeue(out var referencedType))
                    {
                        switch (referencedType)
                        {
                            case ScalarTypeDefinition scalarTypeDefinition:
                                FindDirectives(scalarTypeDefinition.Directives, checkedDirectives, referencedDirectives);
                                break;
                            case EnumTypeDefinition enumTypeDefinition:
                                FindDirectives(enumTypeDefinition.Directives, checkedDirectives, referencedDirectives);
                                foreach (var enumValue in enumTypeDefinition.EnumValues.Values)
                                    FindDirectives(enumValue.Directives, checkedDirectives, referencedDirectives);
                                break;
                            case InputObjectTypeDefinition inputObjectTypeDefinition:
                                FindDirectives(inputObjectTypeDefinition.Directives, checkedDirectives, referencedDirectives);
                                foreach (var fieldDefinition in inputObjectTypeDefinition.InputFields.Values)
                                {
                                    FindDirectives(fieldDefinition.Directives, checkedDirectives, referencedDirectives);
                                    if (!checkedTypes.Contains(fieldDefinition.Type.Definition!))
                                        referencedTypes.Enqueue(fieldDefinition.Type.Definition!);
                                }
                                break;
                        }

                        checkedTypes.Add(referencedType);
                    }
                }
            }
        }

        private static void FindDirectives(IEnumerable<Directive> directives,
                                           HashSet<DirectiveDefinition> checkedDirectives,
                                           Queue<DirectiveDefinition> referencedDirectives)
        {
            foreach (var directive in directives)
                if (!checkedDirectives.Contains(directive.Definition!))
                    referencedDirectives.Enqueue(directive.Definition!);
        }

        private void CheckDirectiveUsage(Directives directives, 
                                         SchemaNode parentNode, 
                                         DirectiveLocations directiveLocations, 
                                         SchemaNode? grandParent = null, 
                                         SchemaNode? greatGrandParent = null)
        {
            HashSet<DirectiveDefinition> checkedDirectives = [];
            foreach (var directive in directives)
            {
                if (!_schema.Directives.TryGetValue(directive.Name, out var directiveDefinition))
                    throw ValidationException.UnrecognizedType(directive);

                if ((directiveDefinition.DirectiveLocations & directiveLocations) != directiveLocations)
                    throw ValidationException.DirectiveNotAllowedLocation(directive, parentNode, grandParent, greatGrandParent);

                if (checkedDirectives.Contains(directiveDefinition) && !directiveDefinition.Repeatable)
                    throw ValidationException.DirectiveNotRepeatable(directive, parentNode, grandParent, greatGrandParent);

                var checkedArguments = directive.Arguments.ToDictionary();
                foreach (var argumentDefinition in directiveDefinition.Arguments.Values)
                {
                    if (argumentDefinition.Type.NonNull && argumentDefinition.DefaultValue is null)
                    {
                        if (!checkedArguments.TryGetValue(argumentDefinition.Name, out var checkedArgument))
                            throw ValidationException.DirectiveMandatoryArgumentMissing(directive, argumentDefinition.Name, parentNode, grandParent, greatGrandParent);

                        if (checkedArgument.Value is NullValueNode)
                            throw ValidationException.DirectiveMandatoryArgumentNull(directive, argumentDefinition.Name, parentNode, grandParent, greatGrandParent);
                    }

                    checkedArguments.Remove(argumentDefinition.Name);
                }

                if (checkedArguments.Count > 0)
                {
                    foreach (var checkArgument in checkedArguments)
                            throw ValidationException.DirectiveArgumentNotDefined(directive, checkArgument.Key, parentNode, grandParent, greatGrandParent);
                }

                checkedDirectives.Add(directiveDefinition);
            }
        }

        private void VisitFieldDefinintions(IEnumerable<FieldDefinition> fieldDefinitions, SchemaNode parentNode, bool isObject)
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

                CheckDirectiveUsage(fieldDefinition.Directives, parentNode, DirectiveLocations.FIELD_DEFINITION, fieldDefinition);

                foreach (var argumentDefinition in fieldDefinition.Arguments.Values)
                {
                    if (argumentDefinition.Name.StartsWith("__"))
                        throw ValidationException.ListEntryDoubleUnderscore(argumentDefinition.Location, 
                                                                            parentNode.OutputElement, parentNode.OutputName, 
                                                                            fieldDefinition.OutputElement, fieldDefinition.OutputName, 
                                                                            argumentDefinition.OutputElement, argumentDefinition.Name);

                    if (argumentDefinition.Type.Definition is null)
                        throw ValidationException.UnrecognizedType(argumentDefinition.Location, argumentDefinition.Name);

                    if (!argumentDefinition.Type.Definition.IsInputType)
                        throw ValidationException.TypeIsNotAnInputType(fieldDefinition, parentNode, argumentDefinition, argumentDefinition.Type.Definition.OutputName);

                    if (isObject && argumentDefinition.Type.NonNull && (argumentDefinition.DefaultValue is null) && argumentDefinition.Directives.Where(d => d.Name == "deprecated").Any())
                        throw ValidationException.NonNullArgumentCannotBeDeprecated(fieldDefinition, parentNode, argumentDefinition);

                    CheckDirectiveUsage(argumentDefinition.Directives, parentNode, DirectiveLocations.ARGUMENT_DEFINITION, fieldDefinition, argumentDefinition);
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
                CheckTypeImplementsInterface(interfaceDefinitions, processed, objectImplement.Value, parentNode, parentNode);

            return interfaceDefinitions;
        }

        private void CheckTypeImplementsInterface(InterfaceTypeDefinitions objectImplements,
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

                    CheckTypeImplementsInterface(objectImplements, processed, interfaceTypeDefinition, checkInterface, rootNode);
                }
            }
        }

        private static void IsValidImplementations(FieldDefinitions objectFields, InterfaceTypeDefinitions interfaceDefinitions, SchemaNode parentNode)
        {
            foreach (var interfaceDefinition in interfaceDefinitions.Values)
                IsValidImplementation(objectFields, interfaceDefinition, parentNode);
        }

        private static void IsValidImplementation(FieldDefinitions objectFields, InterfaceTypeDefinition interfaceDefinition, SchemaNode parentNode)
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

        private static void CheckInputObjectForCircularReference(InputObjectTypeDefinition inputObjectType, Queue<InputObjectTypeDefinition> referencedInputObjects)
        {
            if (referencedInputObjects.Count > 0)
            {
                HashSet<InputObjectTypeDefinition> checkedInputObjects = [];
                while (referencedInputObjects.TryDequeue(out var referencedInputObject))
                {
                    if (referencedInputObject == inputObjectType)
                        throw ValidationException.InputObjectCircularReference(inputObjectType);

                    foreach (var fieldDefinition in referencedInputObject.InputFields.Values)
                    {
                        if ((fieldDefinition.Type.NonNull && (fieldDefinition.Type is TypeName)) && (fieldDefinition.Type.Definition is InputObjectTypeDefinition referenceInputObject))
                        {
                            if (!checkedInputObjects.Contains(referenceInputObject))
                                referencedInputObjects.Enqueue(referenceInputObject);
                        }
                    }

                    checkedInputObjects.Add(referencedInputObject);
                }
            }
        }
    }
}
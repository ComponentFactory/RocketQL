using RocketQL.Core.Nodes;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private SchemaValidator? _schemaValidator = null;
    private SchemaValidator Validator => _schemaValidator ??= new SchemaValidator(this);

    private class SchemaValidator(Schema schema) : NodeVisitor, IDocumentNodeVisitors
    {
        private readonly Schema _schema = schema;

        public void Visit()
        {
            IDocumentNodeVisitors visitor = this;
            visitor.Visit(_schema._directives.Values);
            visitor.Visit(_schema._types.Values);
            visitor.Visit(_schema._schemas);
            ValidateSchema();
        }

        public void VisitOperationDefinition(OperationDefinition operation)
        {
        }

        public void VisitFragmentDefinition(FragmentDefinition fragment)
        {
        }

        public void VisitDirectiveDefinition(DirectiveDefinition directiveDefinition)
        {
            PushPath($"directive {directiveDefinition.Name}");

            if (directiveDefinition.Name.StartsWith("@__"))
                _schema.NonFatalException(ValidationException.NameDoubleUnderscore(directiveDefinition, CurrentPath));

            Queue<TypeDefinition> referencedTypes = [];
            Queue<DirectiveDefinition> referencedDirectives = [];
            foreach (var argumentDefinition in directiveDefinition.Arguments.Values)
            {
                PushPath($"argument {argumentDefinition.Name}");

                if (argumentDefinition.Name.StartsWith("__"))
                    _schema.NonFatalException(ValidationException.NameDoubleUnderscore(argumentDefinition, CurrentPath));

                if (argumentDefinition.Type.Definition is not null)
                {
                    if (!argumentDefinition.Type.Definition.IsInputType)
                        _schema.NonFatalException(ValidationException.TypeIsNotAnInputType(argumentDefinition, CurrentPath));
                    else
                    {
                        if ((argumentDefinition.DefaultValue is not null) && !_schema.IsInputTypeCompatibleWithValue(argumentDefinition.Type, argumentDefinition.DefaultValue))
                            _schema.NonFatalException(ValidationException.DefaultValueNotCompatibleInArgument(argumentDefinition, CurrentPath));

                        referencedTypes.Enqueue(argumentDefinition.Type.Definition);
                        foreach (var directive in argumentDefinition.Directives)
                            if (directive.Definition is not null)
                                referencedDirectives.Enqueue(directive.Definition);

                        CheckDirectiveUsage(argumentDefinition.Directives, directiveDefinition, DirectiveLocations.ARGUMENT_DEFINITION);
                    }
                }

                PopPath();
            }

            CheckDirectiveForCircularReference(directiveDefinition, referencedTypes, referencedDirectives);
            PopPath();
        }

        public void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType)
        {
            PushPath($"scalar {scalarType.Name}");
            CheckDoubleUnderscore(scalarType);
            CheckDirectiveUsage(scalarType.Directives, scalarType, DirectiveLocations.SCALAR);
            PopPath();
        }

        public void VisitObjectTypeDefinition(ObjectTypeDefinition objectType)
        {
            PushPath($"type {objectType.Name}");
            CheckDoubleUnderscore(objectType);
            CheckDirectiveUsage(objectType.Directives, objectType, DirectiveLocations.OBJECT);

            if (objectType.Fields.Count == 0)
                _schema.NonFatalException(ValidationException.AtLeastOne(objectType, "field", CurrentPath));
            else
                VisitFieldDefinintions(objectType.Fields.Values, objectType, isObject: true);

            IsValidImplementations(objectType.Fields, CheckTypeImplementsInterfaces(objectType.ImplementsInterfaces, objectType, isObject: true), objectType);
            PopPath();
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            PushPath($"interface {interfaceType.Name}");
            CheckDoubleUnderscore(interfaceType);
            CheckDirectiveUsage(interfaceType.Directives, interfaceType, DirectiveLocations.INTERFACE);

            if (interfaceType.Fields.Count == 0)
                _schema.NonFatalException(ValidationException.AtLeastOne(interfaceType, "field", CurrentPath));
            else
                VisitFieldDefinintions(interfaceType.Fields.Values, interfaceType, isObject: false);

            IsValidImplementations(interfaceType.Fields, CheckTypeImplementsInterfaces(interfaceType.ImplementsInterfaces, interfaceType, isObject: false), interfaceType);
            PopPath();
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
            PushPath($"union {unionType.Name}");
            CheckDoubleUnderscore(unionType);
            CheckDirectiveUsage(unionType.Directives, unionType, DirectiveLocations.UNION);
            PopPath();
        }

        public void VisitEnumTypeDefinition(EnumTypeDefinition enumType)
        {
            PushPath($"enum {enumType.Name}");
            CheckDoubleUnderscore(enumType);
            CheckDirectiveUsage(enumType.Directives, enumType, DirectiveLocations.ENUM);

            if (enumType.EnumValues.Count == 0)
                _schema.NonFatalException(ValidationException.AtLeastOne(enumType, "enum value", CurrentPath));
            else
            {
                foreach (var enumValueDefinition in enumType.EnumValues.Values)
                {
                    PushPath($"enum value {enumValueDefinition.Name}");
                    CheckDirectiveUsage(enumValueDefinition.Directives, enumType, DirectiveLocations.ENUM_VALUE);
                    PopPath();
                }
            }

            PopPath();
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
            PushPath($"input object {inputObjectType.Name}");
            CheckDoubleUnderscore(inputObjectType);
            CheckDirectiveUsage(inputObjectType.Directives, inputObjectType, DirectiveLocations.INPUT_OBJECT);

            if (inputObjectType.InputFields.Count == 0)
                _schema.NonFatalException(ValidationException.AtLeastOne(inputObjectType, "input field", CurrentPath));
            else
            {
                Queue<InputObjectTypeDefinition> referencedInputObjects = [];
                foreach (var fieldDefinition in inputObjectType.InputFields.Values)
                {
                    PushPath($"input field {fieldDefinition.Name}");

                    if (fieldDefinition.Name.StartsWith("__"))
                        _schema.NonFatalException(ValidationException.NameDoubleUnderscore(fieldDefinition, CurrentPath));

                    if (fieldDefinition.Type.Definition is not null)
                    {
                        if (!fieldDefinition.Type.Definition.IsInputType)
                            _schema.NonFatalException(ValidationException.TypeIsNotAnInputType(fieldDefinition, CurrentPath));
                        else
                        {
                            if ((fieldDefinition.Type is TypeNonNull) && (fieldDefinition.DefaultValue is null) && fieldDefinition.Directives.Where(d => d.Name == "@deprecated").Any())
                                _schema.NonFatalException(ValidationException.NonNullCannotBeDeprecated(fieldDefinition, CurrentPath));

                            if ((fieldDefinition.Type is TypeNonNull fieldNonNull) && ((fieldNonNull.Type is TypeName) && (fieldNonNull.Type.Definition is InputObjectTypeDefinition referenceInputObject)))
                                referencedInputObjects.Enqueue(referenceInputObject);

                            CheckDirectiveUsage(fieldDefinition.Directives, inputObjectType, DirectiveLocations.INPUT_FIELD_DEFINITION);
                        }
                    }

                    PopPath();
                }

                CheckInputObjectForCircularReference(inputObjectType, referencedInputObjects);
            }

            PopPath();
        }

        public void VisitSchemaDefinition(SchemaRoot schemaRoot)
        {
        }

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
            PushPath("schema");

            if (schemaDefinition.Operations.Count == 0)
                _schema.NonFatalException(ValidationException.SchemaDefinitionEmpty(schemaDefinition, CurrentPath));

            schemaDefinition.Operations.TryGetValue(OperationType.QUERY, out var query);
            schemaDefinition.Operations.TryGetValue(OperationType.MUTATION, out var mutation);
            schemaDefinition.Operations.TryGetValue(OperationType.SUBSCRIPTION, out var subscription);

            if (query is null)
                _schema.NonFatalException(ValidationException.SchemaDefinitionMissingQuery(schemaDefinition, CurrentPath));

            if ((mutation is not null) && (mutation.Definition is not null))
            {
                if (mutation.Definition is not ObjectTypeDefinition _)
                    _schema.NonFatalException(ValidationException.SchemaOperationTypeNotObject(mutation, mutation.Definition, CurrentPath));
                else if ((query is not null) && (query.Definition is not null) && (mutation.Definition == query.Definition))
                    _schema.NonFatalException(ValidationException.SchemaOperationsNotUnique(query, mutation, CurrentPath));
            }

            if ((subscription is not null) && (subscription.Definition is not null))
            {
                if (subscription.Definition is not ObjectTypeDefinition _)
                    _schema.NonFatalException(ValidationException.SchemaOperationTypeNotObject(subscription, subscription.Definition, CurrentPath));
                else if ((query is not null) && (query.Definition is not null) && (subscription.Definition == query.Definition))
                    _schema.NonFatalException(ValidationException.SchemaOperationsNotUnique(query, subscription, CurrentPath));
            }

            if ((mutation is not null) && (mutation.Definition is not null) &&
                (subscription is not null) && (subscription.Definition is not null) &&
                (mutation.Definition == subscription.Definition))
            {
                _schema.NonFatalException(ValidationException.SchemaOperationsNotUnique(mutation, subscription, CurrentPath));
            }

            CheckDirectiveUsage(schemaDefinition.Directives, schemaDefinition, DirectiveLocations.SCHEMA);
            PopPath();
        }

        private void ValidateSchema()
        {
            if (_schema._schemas.Count == 1)
            {
                _schema._root = new SchemaRoot(_schema._schemas[0].Description,
                                               _schema._schemas[0].Directives,
                                               _schema._schemas[0].Operations.Where(o => o.Key == OperationType.QUERY).Select(o => o.Value).FirstOrDefault(),
                                               _schema._schemas[0].Operations.Where(o => o.Key == OperationType.MUTATION).Select(o => o.Value).FirstOrDefault(),
                                               _schema._schemas[0].Operations.Where(o => o.Key == OperationType.SUBSCRIPTION).Select(o => o.Value).FirstOrDefault(),
                                               _schema._schemas[0].Location);
            }
            else
            {
                _schema._types.TryGetValue("Query", out var queryTypeDefinition);
                _schema._types.TryGetValue("Mutation", out var mutationTypeDefinition);
                _schema._types.TryGetValue("Subscription", out var subscriptionTypeDefinition);

                if (queryTypeDefinition is null)
                    _schema.NonFatalException(ValidationException.AutoSchemaQueryMissing());
                else
                {
                    if (queryTypeDefinition is not ObjectTypeDefinition)
                        _schema.NonFatalException(ValidationException.AutoSchemaOperationNotObject(queryTypeDefinition, "Query"));

                    if (!AllReferencesWithinType(queryTypeDefinition))
                        _schema.NonFatalException(ValidationException.AutoSchemaOperationReferenced(queryTypeDefinition, "Query"));
                }

                if (mutationTypeDefinition is not null)
                {
                    if (mutationTypeDefinition is not ObjectTypeDefinition)
                        _schema.NonFatalException(ValidationException.AutoSchemaOperationNotObject(mutationTypeDefinition, "Mutation"));

                    if (!AllReferencesWithinType(mutationTypeDefinition))
                        _schema.NonFatalException(ValidationException.AutoSchemaOperationReferenced(mutationTypeDefinition, "Mutation"));
                }

                if (subscriptionTypeDefinition is not null)
                {
                    if (subscriptionTypeDefinition is not ObjectTypeDefinition)
                        _schema.NonFatalException(ValidationException.AutoSchemaOperationNotObject(subscriptionTypeDefinition, "Subscription"));

                    if (!AllReferencesWithinType(subscriptionTypeDefinition))
                        _schema.NonFatalException(ValidationException.AutoSchemaOperationReferenced(subscriptionTypeDefinition, "Subscription"));
                }

                _schema._root = new SchemaRoot("",
                                               [],
                                               OperationTypeFromObjectType(queryTypeDefinition as ObjectTypeDefinition, OperationType.QUERY),
                                               OperationTypeFromObjectType(mutationTypeDefinition as ObjectTypeDefinition, OperationType.MUTATION),
                                               OperationTypeFromObjectType(subscriptionTypeDefinition as ObjectTypeDefinition, OperationType.SUBSCRIPTION),
                                               queryTypeDefinition?.Location ?? Location.Empty);

                OperationTypeDefinitions operations = [];

                if ((_schema._root.Query is not null) && (queryTypeDefinition is not null))
                {
                    operations.Add(_schema._root.Query.Operation, _schema._root.Query);
                    queryTypeDefinition.References.Add(_schema._root.Query);
                }

                if ((_schema._root.Mutation is not null) && (mutationTypeDefinition is not null))
                {
                    operations.Add(_schema._root.Mutation.Operation, _schema._root.Mutation);
                    mutationTypeDefinition.References.Add(_schema._root.Mutation);
                }

                if ((_schema._root.Subscription is not null) && (subscriptionTypeDefinition is not null))
                {
                    operations.Add(_schema._root.Subscription.Operation, _schema._root.Subscription);
                    subscriptionTypeDefinition.References.Add(_schema._root.Subscription);
                }

                _schema._schemas.Add(new SchemaDefinition(_schema._root.Description, _schema._root.Directives, operations, _schema._root.Location));
            }
        }

        private static bool AllReferencesWithinType(TypeDefinition root)
        {
            foreach (var reference in root.References)
            {
                var checkReference = reference;
                while (checkReference.Parent != null)
                    checkReference = checkReference.Parent;

                if (checkReference != root)
                    return false;
            }

            return true;
        }

        private static OperationTypeDefinition? OperationTypeFromObjectType(ObjectTypeDefinition? typeDefinition, OperationType operationType)
        {
            if (typeDefinition is null)
                return null;

            return new OperationTypeDefinition(operationType, typeDefinition.Name, typeDefinition.Location)
            {
                Definition = typeDefinition
            };
        }

        private void CheckDirectiveForCircularReference(DirectiveDefinition directiveDefinition, Queue<TypeDefinition> referencedTypes, Queue<DirectiveDefinition> referencedDirectives)
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
                        {
                            _schema.NonFatalException(ValidationException.DirectiveCircularReference(directiveDefinition, CurrentPath));
                            return;
                        }

                        foreach (var argumentDefinition in referencedDirective.Arguments.Values)
                        {
                            if ((argumentDefinition.Type.Definition is not null) && !checkedTypes.Contains(argumentDefinition.Type.Definition))
                                referencedTypes.Enqueue(argumentDefinition.Type.Definition);

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
                                    if ((fieldDefinition.Type.Definition is not null) && !checkedTypes.Contains(fieldDefinition.Type.Definition))
                                        referencedTypes.Enqueue(fieldDefinition.Type.Definition);
                                }
                                break;
                        }

                        checkedTypes.Add(referencedType);
                    }
                }
            }
        }

        private static void FindDirectives(IEnumerable<Directive> directives, HashSet<DirectiveDefinition> checkedDirectives, Queue<DirectiveDefinition> referencedDirectives)
        {
            foreach (var directive in directives)
                if ((directive.Definition is not null) && !checkedDirectives.Contains(directive.Definition))
                    referencedDirectives.Enqueue(directive.Definition);
        }

        private void CheckDirectiveUsage(Directives directives, DocumentNode parentNode, DirectiveLocations directiveLocations)
        {
            HashSet<DirectiveDefinition> checkedDirectives = [];
            foreach (var directive in directives)
            {
                PushPath($"directive {directive.Name}");

                if (directive.Definition is not null)
                {
                    if ((directive.Definition.DirectiveLocations & directiveLocations) != directiveLocations)
                        _schema.NonFatalException(ValidationException.DirectiveNotAllowedLocation(directive, CurrentPath));

                    if (checkedDirectives.Contains(directive.Definition) && !directive.Definition.Repeatable)
                        _schema.NonFatalException(ValidationException.DirectiveNotRepeatable(directive, CurrentPath));

                    var checkedArguments = directive.Arguments.ToDictionary();
                    foreach (var argumentDefinition in directive.Definition.Arguments.Values)
                    {
                        PushPath($"argument {argumentDefinition.Name}");

                        if (checkedArguments.TryGetValue(argumentDefinition.Name, out var checkedArgument))
                        {
                            if ((checkedArgument.Value is not null) && !_schema.IsInputTypeCompatibleWithValue(argumentDefinition.Type, checkedArgument.Value))
                                _schema.NonFatalException(ValidationException.DefaultValueNotCompatibleInArgument(argumentDefinition, CurrentPath));
                        }
                        else if ((argumentDefinition.DefaultValue is null) && (argumentDefinition.Type is TypeNonNull))
                            _schema.NonFatalException(ValidationException.DirectiveMandatoryArgumentMissing(directive, argumentDefinition.Name, CurrentPath));

                        checkedArguments.Remove(argumentDefinition.Name);

                        PopPath();
                    }

                    if (checkedArguments.Count > 0)
                        foreach (var checkArgument in checkedArguments)
                            _schema.NonFatalException(ValidationException.DirectiveArgumentNotDefined(directive, checkArgument.Key, CurrentPath));

                    checkedDirectives.Add(directive.Definition);
                }

                PopPath();
            }
        }

        private void VisitFieldDefinintions(IEnumerable<FieldDefinition> fieldDefinitions, DocumentNode parentNode, bool isObject)
        {
            foreach (var fieldDefinition in fieldDefinitions)
            {
                PushPath($"field {fieldDefinition.Name}");

                if (fieldDefinition.Name.StartsWith("__"))
                    _schema.NonFatalException(ValidationException.NameDoubleUnderscore(fieldDefinition, CurrentPath));

                if (fieldDefinition.Type.Definition is not null)
                {
                    if (!fieldDefinition.Type.Definition.IsOutputType)
                        _schema.NonFatalException(ValidationException.TypeIsNotAnOutputType(fieldDefinition, CurrentPath));

                    CheckDirectiveUsage(fieldDefinition.Directives, parentNode, DirectiveLocations.FIELD_DEFINITION);

                    foreach (var argumentDefinition in fieldDefinition.Arguments.Values)
                    {
                        PushPath($"argument {argumentDefinition.Name}");

                        if (argumentDefinition.Name.StartsWith("__"))
                            _schema.NonFatalException(ValidationException.NameDoubleUnderscore(argumentDefinition, CurrentPath));

                        if (argumentDefinition.Type.Definition is not null)
                        {
                            if (!argumentDefinition.Type.Definition.IsInputType)
                                _schema.NonFatalException(ValidationException.TypeIsNotAnInputType(argumentDefinition, CurrentPath));

                            if (isObject && argumentDefinition.Type is TypeNonNull && (argumentDefinition.DefaultValue is null) && argumentDefinition.Directives.Where(d => d.Name == "@deprecated").Any())
                                _schema.NonFatalException(ValidationException.NonNullCannotBeDeprecated(fieldDefinition, CurrentPath));

                            if ((argumentDefinition.DefaultValue is not null) && !_schema.IsInputTypeCompatibleWithValue(argumentDefinition.Type, argumentDefinition.DefaultValue))
                                _schema.NonFatalException(ValidationException.DefaultValueNotCompatibleInArgument(argumentDefinition, CurrentPath));

                            CheckDirectiveUsage(argumentDefinition.Directives, parentNode, DirectiveLocations.ARGUMENT_DEFINITION);
                        }

                        PopPath();
                    }
                }

                PopPath();
            }
        }

        private InterfaceTypeDefinitions CheckTypeImplementsInterfaces(Interfaces implementsInterfaces, TypeDefinition parentNode, bool isObject)
        {
            InterfaceTypeDefinitions interfaceDefinitions = [];
            foreach (var interfaceEntry in implementsInterfaces.Values)
            {
                PushPath($"interface {interfaceEntry.Name}");

                if (!isObject && (interfaceEntry.Name == parentNode.OutputName))
                    _schema.NonFatalException(ValidationException.InterfaceCannotImplmentOwnInterface(interfaceEntry, CurrentPath));
                else if (interfaceEntry.Definition is not null)
                {
                    if (interfaceEntry.Definition is not InterfaceTypeDefinition interfaceTypeDefinition)
                        _schema.NonFatalException(ValidationException.TypeIsNotAnInterface(interfaceEntry, parentNode, interfaceEntry.Definition, CurrentPath));
                    else
                        interfaceDefinitions.Add(interfaceTypeDefinition.Name, interfaceTypeDefinition);
                }

                PopPath();
            }

            HashSet<string> processed = [];
            foreach (var objectImplement in interfaceDefinitions)
                CheckTypeImplementsInterface(interfaceDefinitions, processed, objectImplement.Value, parentNode);

            return interfaceDefinitions;
        }

        private void CheckTypeImplementsInterface(InterfaceTypeDefinitions objectImplements, HashSet<string> processed, InterfaceTypeDefinition checkInterface, DocumentNode rootNode)
        {
            if (!processed.Contains(checkInterface.Name))
            {
                processed.Add(checkInterface.Name);

                foreach (var implementsInterface in checkInterface.ImplementsInterfaces.Values)
                {
                    if (!objectImplements.ContainsKey(implementsInterface.Name))
                        _schema.NonFatalException(ValidationException.TypeMissingImplements(rootNode, implementsInterface.Name, checkInterface.Name, CurrentPath));
                    else
                        CheckTypeImplementsInterface(objectImplements, processed, checkInterface, checkInterface);
                }
            }
        }

        private void IsValidImplementations(FieldDefinitions objectFields, InterfaceTypeDefinitions interfaceDefinitions, DocumentNode parentNode)
        {
            foreach (var interfaceDefinition in interfaceDefinitions.Values)
            {
                PushPath($"implements {interfaceDefinition.Name}");
                IsValidImplementation(objectFields, interfaceDefinition, parentNode);
                PopPath();
            }
        }

        private void IsValidImplementation(FieldDefinitions objectFields, InterfaceTypeDefinition interfaceDefinition, DocumentNode parentNode)
        {
            foreach (var interfaceField in interfaceDefinition.Fields.Values)
            {
                PushPath($"field {interfaceField.Name}");

                if (!objectFields.TryGetValue(interfaceField.Name, out var objectFieldDefinition))
                {
                    _schema.NonFatalException(ValidationException.TypeMissingFieldFromInterface(parentNode,
                                                                                                interfaceField.Name,
                                                                                                interfaceDefinition.Name,
                                                                                                CurrentPath));
                }
                else
                {
                    var nonInterface = objectFieldDefinition.Arguments.ToDictionary();

                    foreach (var argument in interfaceField.Arguments.Values)
                    {
                        PushPath($"argument {argument.Name}");

                        if (!objectFieldDefinition.Arguments.TryGetValue(argument.Name, out var objectFieldArgument))
                        {
                            _schema.NonFatalException(ValidationException.TypeMissingFieldArgumentFromInterface(parentNode,
                                                                                                                interfaceField.Name,
                                                                                                                interfaceDefinition.Name,
                                                                                                                argument.Name,
                                                                                                                CurrentPath));
                        }
                        else
                        {
                            if (!IsSameType(objectFieldArgument.Type, argument.Type))
                            {
                                _schema.NonFatalException(ValidationException.TypeFieldArgumentTypeFromInterface(parentNode,
                                                                                                                 interfaceField.Name,
                                                                                                                 interfaceDefinition.Name,
                                                                                                                 argument.Name,
                                                                                                                 CurrentPath));
                            }

                            nonInterface.Remove(argument.Name);
                        }

                        PopPath();
                    }

                    foreach (var nonInterfaceArgument in nonInterface.Values)
                    {
                        PushPath($"argument {nonInterfaceArgument.Name}");

                        if (nonInterfaceArgument.Type is TypeNonNull)
                        {
                            _schema.NonFatalException(ValidationException.TypeFieldArgumentNonNullFromInterface(parentNode,
                                                                                                                interfaceField.Name,
                                                                                                                interfaceDefinition.Name,
                                                                                                                nonInterfaceArgument.Name,
                                                                                                                CurrentPath));
                        }

                        PopPath();
                    }

                    if (!IsValidImplementationFieldType(objectFieldDefinition.Type, interfaceField.Type))
                    {
                        _schema.NonFatalException(ValidationException.TypeFieldReturnNotCompatibleFromInterface(parentNode,
                                                                                                                interfaceField.Name,
                                                                                                                interfaceDefinition.Name,
                                                                                                                CurrentPath));
                    }
                }

                PopPath();
            }
        }

        private static bool IsValidImplementationFieldType(TypeNode fieldType, TypeNode implementedType)
        {
            if (fieldType is TypeNonNull nonNullField)
                return IsValidImplementationFieldType(nonNullField.Type, (implementedType is TypeNonNull noNullImplement) ? noNullImplement.Type : implementedType);
            else if ((fieldType is TypeList fieldTypeList) && (implementedType is TypeList implementedTypeList))
                return IsValidImplementationFieldType(fieldTypeList.Type, implementedTypeList.Type);

            return IsSubType(fieldType, implementedType);
        }

        private static bool IsSubType(TypeNode possibleSubType, TypeNode superType)
        {
            if (IsSameType(possibleSubType, superType))
                return true;

            if (((possibleSubType.Definition is not null) && (possibleSubType.Definition is ObjectTypeDefinition possibleObject) && (superType.Definition is UnionTypeDefinition superUnion)))
            {
                foreach (var memberType in superUnion.MemberTypes)
                    if ((memberType.Value.Definition is not null) && (memberType.Value.Definition == possibleObject))
                        return true;
            }

            if ((superType.Definition is not null) && (superType.Definition is InterfaceTypeDefinition superInterfaceType))
            {
                Interfaces? possibleInterfaces = null;
                if (possibleSubType.Definition is not null)
                {
                    if (possibleSubType.Definition is ObjectTypeDefinition possibleObjectType)
                        possibleInterfaces = possibleObjectType.ImplementsInterfaces;
                    else if (possibleSubType.Definition is InterfaceTypeDefinition possibleInterfaceType)
                        possibleInterfaces = possibleInterfaceType.ImplementsInterfaces;
                }

                if ((possibleInterfaces is not null) && possibleInterfaces.ContainsKey(superInterfaceType.Name))
                    return true;
            }

            return false;
        }

        private static bool IsSameType(TypeNode left, TypeNode right)
        {
            if ((left is TypeName leftTypeName) && (right is TypeName rightTypeName))
            {
                return (leftTypeName.Definition == rightTypeName.Definition);
            }
            else if ((left is TypeNonNull leftTypeNonNull) && (right is TypeNonNull rightTypeNonNull))
            {
                return IsSameType(leftTypeNonNull.Type, rightTypeNonNull.Type);
            }
            else if ((left is TypeList leftTypeList) && (right is TypeList rightTypeList))
            {
                return IsSameType(leftTypeList.Type, rightTypeList.Type);
            }

            return false;
        }

        private void CheckInputObjectForCircularReference(InputObjectTypeDefinition inputObjectType, Queue<InputObjectTypeDefinition> referencedInputObjects)
        {
            if (referencedInputObjects.Count > 0)
            {
                HashSet<InputObjectTypeDefinition> checkedInputObjects = [];
                while (referencedInputObjects.TryDequeue(out var referencedInputObject))
                {
                    if (referencedInputObject == inputObjectType)
                    {
                        _schema.NonFatalException(ValidationException.InputObjectCircularReference(inputObjectType, CurrentPath));
                        return;
                    }
                    else
                    {
                        foreach (var fieldDefinition in referencedInputObject.InputFields.Values)
                        {
                            if ((fieldDefinition.Type is TypeNonNull nonNullField) &&
                                (nonNullField.Type is TypeName) &&
                                (nonNullField.Type.Definition is not null) &&
                                (nonNullField.Type.Definition is InputObjectTypeDefinition referenceInputObject) &&
                                (!checkedInputObjects.Contains(referenceInputObject)))
                            {
                                referencedInputObjects.Enqueue(referenceInputObject);
                            }
                        }

                        checkedInputObjects.Add(referencedInputObject);
                    }
                }
            }
        }

        private void CheckDoubleUnderscore(TypeDefinition node)
        {
            if (node.Name.StartsWith("__"))
                _schema.NonFatalException(ValidationException.NameDoubleUnderscore(node, CurrentPath));
        }
    }
}

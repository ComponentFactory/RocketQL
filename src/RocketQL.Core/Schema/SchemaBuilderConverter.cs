namespace RocketQL.Core.Base;

public partial class SchemaBuilder
{
    private SchemaBuilderConverter? _converter = null;
    private SchemaBuilderConverter Converter => _converter ??= new SchemaBuilderConverter(this);

    private class SchemaBuilderConverter(SchemaBuilder schema) : NodePathTracker, ISyntaxNodeVisitors
    {
        private readonly SchemaBuilder _schema = schema;
        private readonly HashSet<string> _uniqueNames1 = [];
        private readonly HashSet<string> _uniqueNames2 = [];

        public void Visit()
        {
            ((ISyntaxNodeVisitors)this).Visit(_schema._nodes);
        }

        public void VisitOperationDefinition(SyntaxOperationDefinitionNode operation)
        {
            _schema.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(operation, operation.OutputElement()));
        }

        public void VisitFragmentDefinition(SyntaxFragmentDefinitionNode fragment)
        {
            _schema.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(fragment, fragment.OutputElement()));
        }

        public void VisitSchemaDefinition(SyntaxSchemaDefinitionNode schema)
        {
            PushPath("schema");

            if (_schema._schemas.Count > 0)
                _schema.NonFatalException(ValidationException.SchemaDefinitionAlreadyEncountered(schema, CurrentPath));
            else
                _schema._schemas.Add(new(schema.Description, ConvertDirectives(schema.Directives), ConvertOperationTypeDefinitions(schema.OperationTypes), schema.Location));

            PopPath();
        }

        public void VisitDirectiveDefinition(SyntaxDirectiveDefinitionNode directive)
        {
            PushPath(directive);

            if (_schema._directives.ContainsKey(directive.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(directive, directive.Name, directive.OutputElement(), CurrentPath));
            else
            {
                _schema._directives.Add(directive.Name, new(directive.Description,
                                                            directive.Name,
                                                            ConvertInputValueDefinitions(directive.Arguments),
                                                            directive.Repeatable,
                                                            directive.DirectiveLocations,
                                                            directive.Location));
            }

            PopPath();
        }

        public void VisitScalarTypeDefinition(SyntaxScalarTypeDefinitionNode scalarType)
        {
            PushPath(scalarType);

            if (_schema._types.ContainsKey(scalarType.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(scalarType, scalarType.Name, scalarType.OutputElement(), CurrentPath));
            else
            {
                _schema._types.Add(scalarType.Name, new ScalarTypeDefinition(scalarType.Description,
                                                                             scalarType.Name,
                                                                             ConvertDirectives(scalarType.Directives),
                                                                             scalarType.Location));
            }

            PopPath();
        }

        public void VisitObjectTypeDefinition(SyntaxObjectTypeDefinitionNode objectType)
        {
            PushPath(objectType);

            if (_schema._types.ContainsKey(objectType.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(objectType, objectType.Name, objectType.OutputElement(), CurrentPath));
            else
            {
                _schema._types.Add(objectType.Name, new ObjectTypeDefinition(objectType.Description,
                                                                             objectType.Name,
                                                                             ConvertDirectives(objectType.Directives),
                                                                             ConvertInterfaces(objectType, objectType.ImplementsInterfaces),
                                                                             ConvertFieldDefinitions(objectType, objectType.Fields),
                                                                             objectType.Location));
            }

            PopPath();
        }

        public void VisitInterfaceTypeDefinition(SyntaxInterfaceTypeDefinitionNode interfaceType)
        {
            PushPath(interfaceType);

            if (_schema._types.ContainsKey(interfaceType.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(interfaceType, interfaceType.Name, interfaceType.OutputElement(), CurrentPath));
            else
            {
                _schema._types.Add(interfaceType.Name, new InterfaceTypeDefinition(interfaceType.Description,
                                                                                   interfaceType.Name,
                                                                                   ConvertDirectives(interfaceType.Directives),
                                                                                   ConvertInterfaces(interfaceType, interfaceType.ImplementsInterfaces),
                                                                                   ConvertFieldDefinitions(interfaceType, interfaceType.Fields),
                                                                                   interfaceType.Location));
            }

            PopPath();
        }

        public void VisitUnionTypeDefinition(SyntaxUnionTypeDefinitionNode unionType)
        {
            PushPath(unionType);

            if (_schema._types.ContainsKey(unionType.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(unionType, unionType.Name, unionType.OutputElement(), CurrentPath));
            else
            {
                _schema._types.Add(unionType.Name, new UnionTypeDefinition(unionType.Description,
                                                                           unionType.Name,
                                                                           ConvertDirectives(unionType.Directives),
                                                                           ConvertMemberTypes(unionType, unionType.MemberTypes),
                                                                           unionType.Location));
            }

            PopPath();
        }

        public void VisitEnumTypeDefinition(SyntaxEnumTypeDefinitionNode enumType)
        {
            PushPath(enumType);

            if (_schema._types.ContainsKey(enumType.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(enumType, enumType.Name, enumType.OutputElement(), CurrentPath));
            else
            {
                _schema._types.Add(enumType.Name, new EnumTypeDefinition(enumType.Description,
                                                                         enumType.Name,
                                                                         ConvertDirectives(enumType.Directives),
                                                                         ConvertEnumValueDefinitions(enumType, enumType.EnumValues),
                                                                         enumType.Location));
            }

            PopPath();
        }

        public void VisitInputObjectTypeDefinition(SyntaxInputObjectTypeDefinitionNode inputObjectType)
        {
            PushPath(inputObjectType);

            if (_schema._types.ContainsKey(inputObjectType.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(inputObjectType, inputObjectType.Name, inputObjectType.OutputElement(), CurrentPath));
            else
            {
                _schema._types.Add(inputObjectType.Name, new InputObjectTypeDefinition(inputObjectType.Description,
                                                                                       inputObjectType.Name,
                                                                                       ConvertDirectives(inputObjectType.Directives),
                                                                                       ConvertInputValueDefinitions(inputObjectType.InputFields),
                                                                                       inputObjectType.Location));
            }

            PopPath();
        }

        public void VisitExtendSchemaDefinition(SyntaxExtendSchemaDefinitionNode extendSchema)
        {
            PushPath("extend schema");

            if (_schema._schemas.Count == 0)
                _schema.NonFatalException(ValidationException.ExtendSchemaNotDefined(extendSchema, CurrentPath));
            else
            {
                if ((extendSchema.Directives.Count == 0) && (extendSchema.OperationTypes.Count == 0))
                    _schema.NonFatalException(ValidationException.ExtendSchemaMandatory(extendSchema, CurrentPath));
                else
                {
                    var schemaType = _schema._schemas[0];

                    if (extendSchema.Directives.Count > 0)
                        schemaType.Directives.AddRange(ConvertDirectives(extendSchema.Directives));

                    if (extendSchema.OperationTypes.Count > 0)
                    {
                        foreach (var operationType in extendSchema.OperationTypes)
                        {
                            PushPath(operationType);

                            if (schemaType.Operations.TryGetValue(operationType.Operation, out _))
                                _schema.NonFatalException(ValidationException.ExtendSchemaOperationAlreadyDefined(operationType, operationType.Operation, CurrentPath));
                            else
                                schemaType.Operations.Add(operationType.Operation, new(operationType.Operation, operationType.NamedType, operationType.Location));

                            PopPath();
                        }
                    }
                }
            }

            PopPath();
        }

        public void VisitExtendScalarTypeDefinition(SyntaxExtendScalarTypeDefinitionNode extendScalarType)
        {
            PushPath(extendScalarType);

            if (!_schema._types.TryGetValue(extendScalarType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.ExtendTypeAlreadyDefined(extendScalarType, extendScalarType.Name, "Scalar", CurrentPath));
            else
            {
                if (typeDefinition is not ScalarTypeDefinition scalarType)
                    _schema.NonFatalException(ValidationException.ExtendIncorrectType(extendScalarType, typeDefinition.OutputElement(), typeDefinition, CurrentPath));
                else
                {
                    if (extendScalarType.Directives.Count == 0)
                        _schema.NonFatalException(ValidationException.ExtendScalarMandatory(extendScalarType, extendScalarType.Name, CurrentPath));
                    else
                        scalarType.Directives.AddRange(ConvertDirectives(extendScalarType.Directives));
                }
            }

            PopPath();
        }

        public void VisitExtendObjectTypeDefinition(SyntaxExtendObjectTypeDefinitionNode extendObjectType)
        {
            PushPath(extendObjectType);

            if (!_schema._types.TryGetValue(extendObjectType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.ExtendTypeAlreadyDefined(extendObjectType, extendObjectType.Name, "Type", CurrentPath));
            else
            {
                if (typeDefinition is not ObjectTypeDefinition objectType)
                    _schema.NonFatalException(ValidationException.ExtendIncorrectType(extendObjectType, typeDefinition.OutputElement(), typeDefinition, CurrentPath));
                else
                {
                    if ((extendObjectType.ImplementsInterfaces.Count == 0) && (extendObjectType.Directives.Count == 0) && (extendObjectType.Fields.Count == 0))
                        _schema.NonFatalException(ValidationException.ExtendObjectMandatory(extendObjectType, objectType.OutputElement(), CurrentPath));
                    else
                    {
                        if (extendObjectType.Directives.Count > 0)
                            objectType.Directives.AddRange(ConvertDirectives(extendObjectType.Directives));

                        if (extendObjectType.ImplementsInterfaces.Count > 0)
                        {
                            foreach (var extendImplementsInterface in extendObjectType.ImplementsInterfaces)
                            {
                                PushPath(extendImplementsInterface);

                                if (objectType.ImplementsInterfaces.TryGetValue(extendImplementsInterface.Name, out _))
                                {
                                    _schema.NonFatalException(ValidationException.ExtendObjectImplementAlreadyDefined(extendImplementsInterface,
                                                                                                                      extendObjectType.Name,
                                                                                                                      extendImplementsInterface.Name,
                                                                                                                      CurrentPath));
                                }
                                else
                                {
                                    objectType.ImplementsInterfaces.Add(extendImplementsInterface.Name, new(extendImplementsInterface.Name,
                                                                                                            extendImplementsInterface.Location));
                                }

                                PopPath();
                            }
                        }

                        ExtendFieldsWithArguments(extendObjectType.Fields, objectType.Fields);
                    }
                }
            }

            PopPath();
        }

        public void VisitExtendInterfaceTypeDefinition(SyntaxExtendInterfaceTypeDefinitionNode extendInterfaceType)
        {
            PushPath(extendInterfaceType);

            if (!_schema._types.TryGetValue(extendInterfaceType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.ExtendTypeAlreadyDefined(extendInterfaceType, extendInterfaceType.Name, "Interface", CurrentPath));
            else
            {
                if (typeDefinition is not InterfaceTypeDefinition interfaceType)
                    _schema.NonFatalException(ValidationException.ExtendIncorrectType(extendInterfaceType, typeDefinition.OutputElement(), typeDefinition, CurrentPath));
                else
                {
                    if ((extendInterfaceType.ImplementsInterfaces.Count == 0) && (extendInterfaceType.Directives.Count == 0) && (extendInterfaceType.Fields.Count == 0))
                        _schema.NonFatalException(ValidationException.ExtendInterfaceMandatory(extendInterfaceType, interfaceType.OutputElement(), CurrentPath));
                    else
                    {

                        if (extendInterfaceType.Directives.Count > 0)
                            interfaceType.Directives.AddRange(ConvertDirectives(extendInterfaceType.Directives));

                        if (extendInterfaceType.ImplementsInterfaces.Count > 0)
                        {
                            foreach (var extendImplementsInterface in extendInterfaceType.ImplementsInterfaces)
                            {
                                PushPath(extendImplementsInterface);

                                if (interfaceType.ImplementsInterfaces.TryGetValue(extendImplementsInterface.Name, out _))
                                {
                                    _schema.NonFatalException(ValidationException.ExtendInterfaceImplementAlreadyDefined(extendImplementsInterface,
                                                                                                                         extendInterfaceType.Name,
                                                                                                                         extendImplementsInterface.Name,
                                                                                                                         CurrentPath));
                                }
                                else
                                {
                                    interfaceType.ImplementsInterfaces.Add(extendImplementsInterface.Name, new(extendImplementsInterface.Name,
                                                                                                               extendImplementsInterface.Location));
                                }

                                PopPath();
                            }
                        }

                        ExtendFieldsWithArguments(extendInterfaceType.Fields, interfaceType.Fields);
                    }
                }
            }

            PopPath();
        }

        public void VisitExtendUnionTypeDefinition(SyntaxExtendUnionTypeDefinitionNode extendUnionType)
        {
            PushPath(extendUnionType);

            if (!_schema._types.TryGetValue(extendUnionType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.ExtendTypeAlreadyDefined(extendUnionType, extendUnionType.Name, "Union", CurrentPath));
            else
            {
                if (typeDefinition is not UnionTypeDefinition unionType)
                    _schema.NonFatalException(ValidationException.ExtendIncorrectType(extendUnionType, typeDefinition.OutputElement(), typeDefinition, CurrentPath));
                else
                {
                    if ((extendUnionType.Directives.Count == 0) && (extendUnionType.MemberTypes.Count == 0))
                        _schema.NonFatalException(ValidationException.ExtendUnionMandatory(extendUnionType, extendUnionType.Name, CurrentPath));
                    else
                    {
                        if (extendUnionType.Directives.Count > 0)
                            unionType.Directives.AddRange(ConvertDirectives(extendUnionType.Directives));

                        if (extendUnionType.MemberTypes.Count > 0)
                        {
                            foreach (var extendMemberType in extendUnionType.MemberTypes)
                            {
                                PushPath(extendMemberType);

                                if (unionType.MemberTypes.TryGetValue(extendMemberType.Name, out _))
                                {
                                    _schema.NonFatalException(ValidationException.ExtendUnionAlreadyDefined(extendUnionType,
                                                                                                            extendUnionType.Name,
                                                                                                            extendMemberType.Name,
                                                                                                            CurrentPath));
                                }
                                else
                                    unionType.MemberTypes.Add(extendMemberType.Name, new(extendMemberType.Name, extendMemberType.Location));

                                PopPath();
                            }
                        }
                    }
                }
            }

            PopPath();
        }

        public void VisitExtendEnumDefinition(SyntaxExtendEnumTypeDefinitionNode extendEnumType)
        {
            PushPath(extendEnumType);

            if (!_schema._types.TryGetValue(extendEnumType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.ExtendTypeAlreadyDefined(extendEnumType, extendEnumType.Name, "Enum", CurrentPath));
            else
            {
                if (typeDefinition is not EnumTypeDefinition enumType)
                    _schema.NonFatalException(ValidationException.ExtendIncorrectType(extendEnumType, typeDefinition.OutputElement(), typeDefinition, CurrentPath));
                else
                {
                    if ((extendEnumType.Directives.Count == 0) && (extendEnumType.EnumValues.Count == 0))
                        _schema.NonFatalException(ValidationException.ExtendEnumMandatory(extendEnumType, extendEnumType.Name, CurrentPath));
                    else
                    {
                        if (extendEnumType.Directives.Count > 0)
                            enumType.Directives.AddRange(ConvertDirectives(extendEnumType.Directives));

                        if (extendEnumType.EnumValues.Count > 0)
                        {
                            foreach (var extendEnumValue in extendEnumType.EnumValues)
                            {
                                PushPath(extendEnumValue);

                                if (_uniqueNames1.Contains(extendEnumValue.Name))
                                {
                                    _schema.NonFatalException(ValidationException.ExtendEnumValueAlreadyDefined(extendEnumValue,
                                                                                                                extendEnumType.Name,
                                                                                                                extendEnumValue.Name,
                                                                                                                CurrentPath));
                                }
                                else
                                {
                                    if (!enumType.EnumValues.TryGetValue(extendEnumValue.Name, out var existingEnumValue))
                                    {
                                        enumType.EnumValues.Add(extendEnumValue.Name, new(extendEnumValue.Description,
                                                                                          extendEnumValue.Name,
                                                                                          ConvertDirectives(extendEnumValue.Directives),
                                                                                          extendEnumValue.Location));
                                    }
                                    else
                                    {
                                        if (extendEnumValue.Directives.Count == 0)
                                        {
                                            _schema.NonFatalException(ValidationException.ExtendExistingEnumValueUnchanged(extendEnumValue,
                                                                                                                           extendEnumType.Name,
                                                                                                                           extendEnumValue.Name,
                                                                                                                           CurrentPath));
                                        }
                                        else
                                            existingEnumValue.Directives.AddRange(ConvertDirectives(extendEnumValue.Directives));
                                    }

                                    _uniqueNames1.Add(extendEnumValue.Name);
                                }

                                PopPath();
                            }

                            _uniqueNames1.Clear();
                        }
                    }
                }
            }

            PopPath();
        }

        public void VisitExtendInputObjectTypeDefinition(SyntaxExtendInputObjectTypeDefinitionNode extendInputObjectType)
        {
            PushPath(extendInputObjectType);

            if (!_schema._types.TryGetValue(extendInputObjectType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.ExtendTypeAlreadyDefined(extendInputObjectType, extendInputObjectType.Name, "Input object", CurrentPath));
            else
            {
                if (typeDefinition is not InputObjectTypeDefinition inputObjectType)
                    _schema.NonFatalException(ValidationException.ExtendIncorrectType(extendInputObjectType, typeDefinition.OutputElement(), typeDefinition, CurrentPath));
                else
                {
                    if ((extendInputObjectType.Directives.Count == 0) && (extendInputObjectType.InputFields.Count == 0))
                        _schema.NonFatalException(ValidationException.ExtendInputObjectMandatory(extendInputObjectType, extendInputObjectType.Name, CurrentPath));
                    else
                    {
                        if (extendInputObjectType.Directives.Count > 0)
                            inputObjectType.Directives.AddRange(ConvertDirectives(extendInputObjectType.Directives));

                        ExtendInputFields(extendInputObjectType.InputFields, inputObjectType.InputFields);
                    }
                }
            }

            PopPath();
        }

        private void ExtendFieldsWithArguments(SyntaxFieldDefinitionNodeList extendFields, FieldDefinitions existingFields)
        {
            if (extendFields.Count > 0)
            {
                foreach (var extendField in extendFields)
                {
                    PushPath(extendField);

                    if (_uniqueNames1.Contains(extendField.Name))
                        _schema.NonFatalException(ValidationException.DuplicateName(extendField, extendField.OutputElement(), extendField.Name, CurrentPath));
                    else
                    {
                        if (!existingFields.TryGetValue(extendField.Name, out var existingField))
                        {
                            existingFields.Add(extendField.Name, new(extendField.Description,
                                                                     extendField.Name,
                                                                     ConvertInputValueDefinitions(extendField.Arguments),
                                                                     ConvertTypeNode(extendField.Type),
                                                                     ConvertDirectives(extendField.Directives),
                                                                     extendField.Location));
                        }
                        else
                        {
                            var changed = false;

                            if (extendField.Directives.Count > 0)
                            {
                                existingField.Directives.AddRange(ConvertDirectives(extendField.Directives));
                                changed = true;
                            }

                            foreach (var extendArgument in extendField.Arguments)
                            {
                                PushPath(extendArgument);

                                if (_uniqueNames2.Contains(extendArgument.Name))
                                    _schema.NonFatalException(ValidationException.DuplicateName(extendArgument, extendArgument.OutputElement(), extendArgument.Name, CurrentPath));
                                else
                                {
                                    if (!existingField.Arguments.TryGetValue(extendArgument.Name, out var existingArgument))
                                    {
                                        existingField.Arguments.Add(extendArgument.Name, new(extendArgument.Description,
                                                                                             extendArgument.Name,
                                                                                             ConvertTypeNode(extendArgument.Type),
                                                                                             extendArgument.DefaultValue,
                                                                                             ConvertDirectives(extendArgument.Directives),
                                                                                             extendArgument.Usage,
                                                                                             extendArgument.Location));

                                        changed = true;
                                    }
                                    else
                                    {
                                        if (extendArgument.Directives.Count > 0)
                                        {
                                            existingArgument.Directives.AddRange(ConvertDirectives(extendArgument.Directives));
                                            changed = true;
                                        }
                                    }
                                }

                                PopPath();
                            }

                            _uniqueNames2.Clear();

                            if (!changed)
                                _schema.NonFatalException(ValidationException.ExtendExistingFieldUnchanged(extendField, CurrentPath));
                        }
                    }

                    _uniqueNames1.Add(extendField.Name);
                    PopPath();
                }

                _uniqueNames1.Clear();
            }
        }

        private void ExtendInputFields(SyntaxInputValueDefinitionNodeList extendInputFields, InputValueDefinitions existingInputFields)
        {
            if (extendInputFields.Count > 0)
            {
                HashSet<string> inputFieldNames = [];
                foreach (var extendInputField in extendInputFields)
                {
                    PushPath(extendInputField);

                    if (inputFieldNames.Contains(extendInputField.Name))
                        _schema.NonFatalException(ValidationException.DuplicateName(extendInputField, extendInputField.OutputElement(), extendInputField.Name, CurrentPath));
                    else
                    {
                        if (!existingInputFields.TryGetValue(extendInputField.Name, out var existingField))
                        {
                            existingInputFields.Add(extendInputField.Name, new(extendInputField.Description,
                                                                               extendInputField.Name,
                                                                               ConvertTypeNode(extendInputField.Type),
                                                                               extendInputField.DefaultValue,
                                                                               ConvertDirectives(extendInputField.Directives),
                                                                               extendInputField.Usage,
                                                                               extendInputField.Location));
                        }
                        else
                        {
                            if (extendInputField.Directives.Count == 0)
                                _schema.NonFatalException(ValidationException.ExtendExistingInputFieldUnchanged(extendInputField, CurrentPath));
                            else
                                existingField.Directives.AddRange(ConvertDirectives(extendInputField.Directives));
                        }

                        inputFieldNames.Add(extendInputField.Name);
                    }

                    PopPath();
                }
            }
        }

        private FieldDefinitions ConvertFieldDefinitions(LocationNode parentNode, SyntaxFieldDefinitionNodeList fields)
        {
            var nodes = new FieldDefinitions();

            foreach (var field in fields)
            {
                PushPath(field);

                if (nodes.ContainsKey(field.Name))
                    _schema.NonFatalException(ValidationException.DuplicateName(parentNode, field.OutputElement(), field.Name, CurrentPath));
                else
                {
                    nodes.Add(field.Name, new(field.Description,
                                              field.Name,
                                              ConvertInputValueDefinitions(field.Arguments),
                                              ConvertTypeNode(field.Type),
                                              ConvertDirectives(field.Directives),
                                              field.Location));
                }

                PopPath();
            }

            return nodes;
        }

        private OperationTypeDefinitions ConvertOperationTypeDefinitions(SyntaxOperationTypeDefinitionNodeList operationTypes)
        {
            var nodes = new OperationTypeDefinitions();

            foreach (var operationType in operationTypes)
            {
                PushPath(operationType);

                if (nodes.ContainsKey(operationType.Operation))
                    _schema.NonFatalException(ValidationException.SchemaDefinitionMultipleOperation(operationType, CurrentPath));
                else
                    nodes.Add(operationType.Operation, new(operationType.Operation, operationType.NamedType, operationType.Location));

                PopPath();
            }

            return nodes;
        }

        private InputValueDefinitions ConvertInputValueDefinitions(SyntaxInputValueDefinitionNodeList inputValues)
        {
            var nodes = new InputValueDefinitions();

            foreach (var inputValue in inputValues)
            {
                PushPath(inputValue);

                if (nodes.ContainsKey(inputValue.Name))
                    _schema.NonFatalException(ValidationException.DuplicateName(inputValue, inputValue.OutputElement(), inputValue.Name, CurrentPath));
                else
                {
                    nodes.Add(inputValue.Name, new InputValueDefinition(inputValue.Description,
                                                                        inputValue.Name,
                                                                        ConvertTypeNode(inputValue.Type),
                                                                        inputValue.DefaultValue,
                                                                        ConvertDirectives(inputValue.Directives),
                                                                        inputValue.Usage,
                                                                        inputValue.Location));
                }

                PopPath();
            }

            return nodes;
        }

        private Interfaces ConvertInterfaces(LocationNode parentNode, SyntaxNameList names)
        {
            var nodes = new Interfaces();

            foreach (var name in names)
            {
                PushPath(name);

                if (nodes.ContainsKey(name.Name))
                    _schema.NonFatalException(ValidationException.DuplicateName(parentNode, name.OutputElement(), name.Name, CurrentPath));
                else
                    nodes.Add(name.Name, new(name.Name, name.Location));

                PopPath();
            }

            return nodes;
        }

        private MemberTypes ConvertMemberTypes(LocationNode parentNode, SyntaxNameList names)
        {
            var nodes = new MemberTypes();

            foreach (var name in names)
            {
                PushPath(name);

                if (nodes.ContainsKey(name.Name))
                    _schema.NonFatalException(ValidationException.DuplicateName(parentNode, name.OutputElement(), name.Name, CurrentPath));
                else
                    nodes.Add(name.Name, new(name.Name, name.Location));

                PopPath();
            }

            return nodes;
        }

        private EnumValueDefinitions ConvertEnumValueDefinitions(LocationNode parentNode, SyntaxEnumValueDefinitionList enumValues)
        {
            var nodes = new EnumValueDefinitions();

            foreach (var enumValue in enumValues)
            {
                PushPath(enumValue);

                if (nodes.ContainsKey(enumValue.Name))
                    _schema.NonFatalException(ValidationException.DuplicateName(parentNode, enumValue.OutputElement(), enumValue.Name, CurrentPath));
                else
                {
                    nodes.Add(enumValue.Name, new(enumValue.Description,
                                                  enumValue.Name,
                                                  ConvertDirectives(enumValue.Directives),
                                                  enumValue.Location));
                }

                PopPath();
            }

            return nodes;
        }

        private Directives ConvertDirectives(SyntaxDirectiveNodeList directives)
        {
            var nodes = new Directives();

            foreach (var directive in directives)
            {
                PushPath(directive);
                nodes.Add(new(directive.Name, ConvertObjectFields(directive, directive.Arguments), directive.Location));
                PopPath();
            }

            return nodes;
        }

        private ObjectFields ConvertObjectFields(LocationNode parentNode, SyntaxObjectFieldNodeList fields)
        {
            var nodes = new ObjectFields();

            foreach (var field in fields)
            {
                PushPath(field);

                if (nodes.ContainsKey(field.Name))
                    throw ValidationException.DuplicateName(parentNode, "argument", field.Name, CurrentPath);
                else
                    nodes.Add(field.Name, field);

                PopPath();
            }

            return nodes;
        }

        private TypeNode ConvertTypeNode(SyntaxTypeNode node)
        {
            return node switch
            {
                SyntaxTypeNameNode nameNode => new TypeName(nameNode.Name, nameNode.Location),
                SyntaxTypeNonNullNode nonNullNode => new TypeNonNull(ConvertTypeNode(nonNullNode.Type), nonNullNode.Location),
                SyntaxTypeListNode listNode => new TypeList(ConvertTypeNode(listNode.Type), listNode.Location),
                _ => throw ValidationException.UnrecognizedType(node.Location, node.GetType().Name, CurrentPath)
            };
        }

    }
}

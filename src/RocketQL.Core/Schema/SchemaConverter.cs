namespace RocketQL.Core.Base;

public partial class Schema
{
    private SchemaConverter? _schemaConverter = null;
    private SchemaConverter Converter => _schemaConverter ??= new SchemaConverter(this);

    private class SchemaConverter(Schema schema) : ConverterNodeVisitor, ISyntaxNodeVisitors
    {
        private readonly Schema _schema = schema;

        public void Visit()
        {
            ((ISyntaxNodeVisitors)this).Visit(_schema._nodes);
        }

        public void VisitOperationDefinition(SyntaxOperationDefinitionNode operation)
        {
            _schema.NonFatalException(ValidationException.SchemaDefinitionIgnored(operation.Location, "Operation"));
        }

        public void VisitFragmentDefinition(SyntaxFragmentDefinitionNode fragment)
        {
            _schema.NonFatalException(ValidationException.SchemaDefinitionIgnored(fragment.Location, "Fragment"));
        }

        public void VisitSchemaDefinition(SyntaxSchemaDefinitionNode schema)
        {
            if (_schema._schemas.Count > 0)
                _schema.NonFatalException(ValidationException.SchemaDefinitionAlreadyDefined(schema.Location));
            else
            {
                _schema._schemas.Add(new(
                    schema.Description,
                    ConvertDirectives(schema.Directives),
                    ConvertOperationTypeDefinitions(schema.OperationTypes),
                    schema.Location));
            }
        }

        public void VisitDirectiveDefinition(SyntaxDirectiveDefinitionNode directive)
        {
            if (_schema._directives.ContainsKey(directive.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(directive.Location, "Directive", directive.Name));
            else
            {
                _schema._directives.Add(directive.Name, new(
                    directive.Description,
                    directive.Name,
                    ConvertInputValueDefinitions(directive.Arguments, "Argument", "Directive", directive.Name, "Argument"),
                    directive.Repeatable,
                    directive.DirectiveLocations,
                    directive.Location));
            }
        }

        public void VisitScalarTypeDefinition(SyntaxScalarTypeDefinitionNode scalarType)
        {
            if (_schema._types.ContainsKey(scalarType.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(scalarType.Location, "Scalar", scalarType.Name));
            else
            {
                _schema._types.Add(scalarType.Name, new ScalarTypeDefinition(
                    scalarType.Description,
                    scalarType.Name,
                    ConvertDirectives(scalarType.Directives),
                    scalarType.Location));
            }
        }

        public void VisitObjectTypeDefinition(SyntaxObjectTypeDefinitionNode objectType)
        {
            if (_schema._types.ContainsKey(objectType.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(objectType.Location, "Object", objectType.Name));
            else
            {
                _schema._types.Add(objectType.Name, new ObjectTypeDefinition(
                    objectType.Description,
                    objectType.Name,
                    ConvertDirectives(objectType.Directives),
                    ConvertInterfaces(objectType.ImplementsInterfaces, "Object", objectType.Name),
                    ConvertFieldDefinitions(objectType.Fields, "Object", objectType.Name),
                    objectType.Location));
            }
        }

        public void VisitInterfaceTypeDefinition(SyntaxInterfaceTypeDefinitionNode interfaceType)
        {
            if (_schema._types.ContainsKey(interfaceType.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(interfaceType.Location, "Interface", interfaceType.Name));
            else
            {
                _schema._types.Add(interfaceType.Name, new InterfaceTypeDefinition(
                    interfaceType.Description,
                    interfaceType.Name,
                    ConvertDirectives(interfaceType.Directives),
                    ConvertInterfaces(interfaceType.ImplementsInterfaces, "Interface", interfaceType.Name),
                    ConvertFieldDefinitions(interfaceType.Fields, "Interface", interfaceType.Name),
                    interfaceType.Location));
            }
        }

        public void VisitUnionTypeDefinition(SyntaxUnionTypeDefinitionNode unionType)
        {
            if (_schema._types.ContainsKey(unionType.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(unionType.Location, "Union", unionType.Name));
            else
            {
                _schema._types.Add(unionType.Name, new UnionTypeDefinition(
                    unionType.Description,
                    unionType.Name,
                    ConvertDirectives(unionType.Directives),
                    ConvertMemberTypes(unionType.MemberTypes, "Union", unionType.Name),
                    unionType.Location));
            }
        }

        public void VisitEnumTypeDefinition(SyntaxEnumTypeDefinitionNode enumType)
        {
            if (_schema._types.ContainsKey(enumType.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(enumType.Location, "Enum", enumType.Name));
            else
            {
                _schema._types.Add(enumType.Name, new EnumTypeDefinition(
                    enumType.Description,
                    enumType.Name,
                    ConvertDirectives(enumType.Directives),
                    ConvertEnumValueDefinitions(enumType.Name, enumType.EnumValues),
                    enumType.Location));
            }
        }

        public void VisitInputObjectTypeDefinition(SyntaxInputObjectTypeDefinitionNode inputObjectType)
        {
            if (_schema._types.ContainsKey(inputObjectType.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(inputObjectType.Location, "Input object", inputObjectType.Name));
            else
            {
                _schema._types.Add(inputObjectType.Name, new InputObjectTypeDefinition(
                    inputObjectType.Description,
                    inputObjectType.Name,
                    ConvertDirectives(inputObjectType.Directives),
                    ConvertInputValueDefinitions(inputObjectType.InputFields, "Input field", "Field", inputObjectType.Name, "Argument"),
                    inputObjectType.Location
                ));
            }
        }

        public void VisitExtendSchemaDefinition(SyntaxExtendSchemaDefinitionNode extendSchema)
        {
            if (_schema._schemas.Count == 0)
                _schema.NonFatalException(ValidationException.SchemaNotDefinedForExtend(extendSchema.Location));
            else
            {
                if ((extendSchema.Directives.Count == 0) && (extendSchema.OperationTypes.Count == 0))
                    _schema.NonFatalException(ValidationException.ExtendSchemaMandatory(extendSchema.Location));
                else
                {
                    var schemaType = _schema._schemas[0];

                    if (extendSchema.Directives.Count > 0)
                        schemaType.Directives.AddRange(ConvertDirectives(extendSchema.Directives));

                    if (extendSchema.OperationTypes.Count > 0)
                    {
                        foreach (var operationType in extendSchema.OperationTypes)
                        {
                            if (schemaType.Operations.TryGetValue(operationType.Operation, out _))
                            {
                                _schema.NonFatalException(ValidationException.ExtendSchemaOperationAlreadyDefined(
                                    operationType.Location, 
                                    operationType.Operation));
                            }
                            else
                            {
                                schemaType.Operations.Add(operationType.Operation, new(
                                    operationType.Operation,
                                    operationType.NamedType,
                                    operationType.Location));
                            }
                        }
                    }
                }
            }
        }

        public void VisitExtendScalarTypeDefinition(SyntaxExtendScalarTypeDefinitionNode extendScalarType)
        {
            if (!_schema._types.TryGetValue(extendScalarType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.TypeNotDefinedForExtend(extendScalarType.Location, "Scalar", extendScalarType.Name));
            else
            {
                if (typeDefinition is not ScalarTypeDefinition scalarType)
                    _schema.NonFatalException(ValidationException.IncorrectTypeForExtend(typeDefinition, "Scalar"));
                else
                {
                    if (extendScalarType.Directives.Count == 0)
                        _schema.NonFatalException(ValidationException.ExtendScalarMandatory(extendScalarType.Location, extendScalarType.Name));
                    else
                        scalarType.Directives.AddRange(ConvertDirectives(extendScalarType.Directives));
                }
            }
        }

        public void VisitExtendObjectTypeDefinition(SyntaxExtendObjectTypeDefinitionNode extendObjectType)
        {
            if (!_schema._types.TryGetValue(extendObjectType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.TypeNotDefinedForExtend(extendObjectType.Location, "Object", extendObjectType.Name));
            else
            {
                if (typeDefinition is not ObjectTypeDefinition objectType)
                    _schema.NonFatalException(ValidationException.IncorrectTypeForExtend(typeDefinition, "Object"));
                else
                {
                    if ((extendObjectType.ImplementsInterfaces.Count == 0) &&
                        (extendObjectType.Directives.Count == 0) &&
                        (extendObjectType.Fields.Count == 0))
                    {
                        _schema.NonFatalException(ValidationException.ExtendObjectInterfaceMandatory(
                            extendObjectType.Location, 
                            objectType.OutputElement, 
                            extendObjectType.Name));
                    }
                    else
                    {
                        if (extendObjectType.Directives.Count > 0)
                            objectType.Directives.AddRange(ConvertDirectives(extendObjectType.Directives));

                        if (extendObjectType.ImplementsInterfaces.Count > 0)
                        {
                            foreach (var extendImplementsInterface in extendObjectType.ImplementsInterfaces)
                            {
                                if (objectType.ImplementsInterfaces.TryGetValue(extendImplementsInterface.Name, out _))
                                    _schema.NonFatalException(ValidationException.ExtendImplementAlreadyDefined(extendImplementsInterface.Location,
                                                                                                                "Extend object",
                                                                                                                extendObjectType.Name,
                                                                                                                extendImplementsInterface.Name));
                                else
                                {
                                    objectType.ImplementsInterfaces.Add(extendImplementsInterface.Name, new(
                                        extendImplementsInterface.Name, 
                                        extendImplementsInterface.Location));
                                }
                            }
                        }

                        ExtendFieldsWithArguments(extendObjectType.Name, extendObjectType.Fields, objectType.Fields, "Extend object");
                    }
                }
            }
        }

        public void VisitExtendInterfaceTypeDefinition(SyntaxExtendInterfaceTypeDefinitionNode extendInterfaceType)
        {
            if (!_schema._types.TryGetValue(extendInterfaceType.Name, out var typeDefinition))
            {
                _schema.NonFatalException(ValidationException.TypeNotDefinedForExtend(
                    extendInterfaceType.Location, 
                    "Interface", 
                    extendInterfaceType.Name));
            }
            else
            {
                if (typeDefinition is not InterfaceTypeDefinition interfaceType)
                    _schema.NonFatalException(ValidationException.IncorrectTypeForExtend(typeDefinition, "Interface"));
                else
                {
                    if ((extendInterfaceType.ImplementsInterfaces.Count == 0) &&
                        (extendInterfaceType.Directives.Count == 0) &&
                        (extendInterfaceType.Fields.Count == 0))
                    {
                        _schema.NonFatalException(ValidationException.ExtendObjectInterfaceMandatory(
                            extendInterfaceType.Location, 
                            interfaceType.OutputElement, 
                            extendInterfaceType.Name));
                    }
                    else
                    {

                        if (extendInterfaceType.Directives.Count > 0)
                            interfaceType.Directives.AddRange(ConvertDirectives(extendInterfaceType.Directives));

                        if (extendInterfaceType.ImplementsInterfaces.Count > 0)
                        {
                            foreach (var extendImplementsInterface in extendInterfaceType.ImplementsInterfaces)
                            {
                                if (interfaceType.ImplementsInterfaces.TryGetValue(extendImplementsInterface.Name, out _))
                                    _schema.NonFatalException(ValidationException.ExtendImplementAlreadyDefined(extendImplementsInterface.Location,
                                                                                                                "Extend interface",
                                                                                                                extendInterfaceType.Name,
                                                                                                                extendImplementsInterface.Name));
                                else
                                {
                                    interfaceType.ImplementsInterfaces.Add(extendImplementsInterface.Name, new(
                                        extendImplementsInterface.Name, 
                                        extendImplementsInterface.Location));
                                }
                            }
                        }

                        ExtendFieldsWithArguments(extendInterfaceType.Name, extendInterfaceType.Fields, interfaceType.Fields, "Extend interface");
                    }
                }
            }
        }

        public void VisitExtendUnionTypeDefinition(SyntaxExtendUnionTypeDefinitionNode extendUnionType)
        {
            if (!_schema._types.TryGetValue(extendUnionType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.TypeNotDefinedForExtend(extendUnionType.Location, "Union", extendUnionType.Name));
            else
            {
                if (typeDefinition is not UnionTypeDefinition unionType)
                    _schema.NonFatalException(ValidationException.IncorrectTypeForExtend(typeDefinition, "Union"));
                else
                {
                    if ((extendUnionType.Directives.Count == 0) && (extendUnionType.MemberTypes.Count == 0))
                        _schema.NonFatalException(ValidationException.ExtendUnionMandatory(extendUnionType.Location, extendUnionType.Name));
                    else
                    {
                        if (extendUnionType.Directives.Count > 0)
                            unionType.Directives.AddRange(ConvertDirectives(extendUnionType.Directives));

                        if (extendUnionType.MemberTypes.Count > 0)
                        {
                            foreach (var extendMemberType in extendUnionType.MemberTypes)
                            {
                                if (unionType.MemberTypes.TryGetValue(extendMemberType.Name, out _))
                                {
                                    _schema.NonFatalException(ValidationException.ExtendUnionAlreadyDefined(
                                        extendUnionType.Location, 
                                        extendMemberType.Name, 
                                        extendUnionType.Name));
                                }
                                else
                                    unionType.MemberTypes.Add(extendMemberType.Name, new(extendMemberType.Name, extendMemberType.Location));
                            }
                        }
                    }
                }
            }
        }

        public void VisitExtendEnumDefinition(SyntaxExtendEnumTypeDefinitionNode extendEnumType)
        {
            if (!_schema._types.TryGetValue(extendEnumType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.TypeNotDefinedForExtend(extendEnumType.Location, "Enum", extendEnumType.Name));
            else
            {
                if (typeDefinition is not EnumTypeDefinition enumType)
                    _schema.NonFatalException(ValidationException.IncorrectTypeForExtend(typeDefinition, "Enum"));
                else
                {
                    if ((extendEnumType.Directives.Count == 0) && (extendEnumType.EnumValues.Count == 0))
                        _schema.NonFatalException(ValidationException.ExtendEnumMandatory(extendEnumType.Location, extendEnumType.Name));
                    else
                    {
                        if (extendEnumType.Directives.Count > 0)
                            enumType.Directives.AddRange(ConvertDirectives(extendEnumType.Directives));

                        if (extendEnumType.EnumValues.Count > 0)
                        {
                            HashSet<string> extendValues = [];
                            foreach (var extendEnumValue in extendEnumType.EnumValues)
                            {
                                if (extendValues.Contains(extendEnumValue.Name))
                                {
                                    _schema.NonFatalException(ValidationException.ExtendEnumValueAlreadyDefined(
                                        extendEnumValue.Location, 
                                        extendEnumValue.Name, 
                                        extendEnumType.Name));
                                }
                                else
                                {
                                    if (!enumType.EnumValues.TryGetValue(extendEnumValue.Name, out var existingEnumValue))
                                    {
                                        enumType.EnumValues.Add(extendEnumValue.Name, new(
                                            extendEnumValue.Description,
                                            extendEnumValue.Name,
                                            ConvertDirectives(extendEnumValue.Directives),
                                            extendEnumValue.Location));
                                    }
                                    else
                                    {
                                        if (extendEnumValue.Directives.Count == 0)
                                        {
                                            _schema.NonFatalException(ValidationException.ExtendExistingEnumValueUnchanged(
                                                extendEnumValue.Location, 
                                                extendEnumValue.Name, 
                                                extendEnumType.Name));
                                        }
                                        else
                                            existingEnumValue.Directives.AddRange(ConvertDirectives(extendEnumValue.Directives));
                                    }

                                    extendValues.Add(extendEnumValue.Name);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void VisitExtendInputObjectTypeDefinition(SyntaxExtendInputObjectTypeDefinitionNode extendInputObjectType)
        {
            if (!_schema._types.TryGetValue(extendInputObjectType.Name, out var typeDefinition))
            {
                _schema.NonFatalException(ValidationException.TypeNotDefinedForExtend(
                    extendInputObjectType.Location, 
                    "Input object", 
                    extendInputObjectType.Name));
            }
            else
            {
                if (typeDefinition is not InputObjectTypeDefinition inputObjectType)
                    _schema.NonFatalException(ValidationException.IncorrectTypeForExtend(typeDefinition, "Input object"));
                else
                {
                    if ((extendInputObjectType.Directives.Count == 0) && (extendInputObjectType.InputFields.Count == 0))
                    {
                        _schema.NonFatalException(ValidationException.ExtendInputObjectMandatory(
                            extendInputObjectType.Location, 
                            inputObjectType.OutputElement, 
                            extendInputObjectType.Name));
                    }
                    else
                    {
                        if (extendInputObjectType.Directives.Count > 0)
                            inputObjectType.Directives.AddRange(ConvertDirectives(extendInputObjectType.Directives));

                        ExtendInputFields(
                            extendInputObjectType.Name, 
                            extendInputObjectType.InputFields, 
                            inputObjectType.InputFields, "" +
                            "Extend input object");
                    }
                }
            }
        }

        private void ExtendFieldsWithArguments(string extendName, 
                                               SyntaxFieldDefinitionNodeList extendFields, 
                                               FieldDefinitions existingFields, 
                                               string errorType)
        {
            if (extendFields.Count > 0)
            {
                HashSet<string> fieldNames = [];
                foreach (var extendField in extendFields)
                {
                    if (fieldNames.Contains(extendField.Name))
                    {
                        _schema.NonFatalException(ValidationException.ExtendFieldAlreadyDefined(
                            extendField.Location, 
                            extendField.Name, 
                            errorType, 
                            extendName));
                    }
                    else
                    {
                        if (!existingFields.TryGetValue(extendField.Name, out var existingField))
                        {
                            existingFields.Add(extendField.Name, new(
                                extendField.Description,
                                extendField.Name,
                                ConvertInputValueDefinitions(
                                    extendField.Arguments, 
                                    "Argument", 
                                    "Field", 
                                    extendField.Name, 
                                    "Argument", 
                                    "Object", 
                                    extendName),
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

                            HashSet<string> argumentNames = [];
                            foreach (var extendArgument in extendField.Arguments)
                            {
                                if (argumentNames.Contains(extendArgument.Name))
                                    _schema.NonFatalException(ValidationException.ExtendFieldArgumentAlreadyDefined(
                                        extendField.Location,
                                        extendField.Name,
                                        extendArgument.Name,
                                        errorType,
                                        extendName));
                                else
                                {
                                    if (!existingField.Arguments.TryGetValue(extendArgument.Name, out var existingArgument))
                                    {
                                        existingField.Arguments.Add(extendArgument.Name, new(
                                            extendArgument.Description,
                                            extendArgument.Name,
                                            ConvertTypeNode(extendArgument.Type),
                                            extendArgument.DefaultValue,
                                            ConvertDirectives(extendArgument.Directives),
                                            extendArgument.Location,
                                            "Argument"));

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
                            }

                            if (!changed)
                            {
                                _schema.NonFatalException(ValidationException.ExtendExistingFieldUnchanged(
                                    extendField.Location, 
                                    extendField.Name, 
                                    errorType, 
                                    extendName));
                            }
                        }
                    }

                    fieldNames.Add(extendField.Name);
                }
            }
        }

        private void ExtendInputFields(string extendName, 
                                       SyntaxInputValueDefinitionNodeList extendInputFields, 
                                       InputValueDefinitions existingInputFields, 
                                       string errorType)
        {
            if (extendInputFields.Count > 0)
            {
                HashSet<string> inputFieldNames = [];
                foreach (var extendInputField in extendInputFields)
                {
                    if (inputFieldNames.Contains(extendInputField.Name))
                    {
                        _schema.NonFatalException(ValidationException.ExtendInputFieldAlreadyDefined(
                            extendInputField.Location, 
                            extendInputField.Name,
                            errorType, 
                            extendName));
                    }
                    else
                    {
                        if (!existingInputFields.TryGetValue(extendInputField.Name, out var existingField))
                        {
                            existingInputFields.Add(extendInputField.Name, new(
                                extendInputField.Description,
                                extendInputField.Name,
                                ConvertTypeNode(extendInputField.Type),
                                extendInputField.DefaultValue,
                                ConvertDirectives(extendInputField.Directives),
                                extendInputField.Location,
                                "Input field"));
                        }
                        else
                        {
                            if (extendInputField.Directives.Count == 0)
                            {
                                _schema.NonFatalException(ValidationException.ExtendExistingInputFieldUnchanged(
                                    extendInputField.Location, 
                                    extendInputField.Name, 
                                    errorType, 
                                    extendName));
                            }
                            else
                                existingField.Directives.AddRange(ConvertDirectives(extendInputField.Directives));
                        }

                        inputFieldNames.Add(extendInputField.Name);
                    }
                }
            }
        }

        private FieldDefinitions ConvertFieldDefinitions(SyntaxFieldDefinitionNodeList fields, string parentNode, string parentName)
        {
            var nodes = new FieldDefinitions();

            foreach (var field in fields)
            {
                if (nodes.ContainsKey(field.Name))
                {
                    _schema.NonFatalException(ValidationException.ListEntryDuplicateName(
                        field.Location, 
                        parentNode, 
                        parentName, 
                        "field",
                        field.Name));
                }
                else
                {
                    nodes.Add(field.Name, new(
                        field.Description,
                        field.Name,
                        ConvertInputValueDefinitions(field.Arguments, "Argument", "Field", field.Name, "Argument", parentNode, parentName),
                        ConvertTypeNode(field.Type),
                        ConvertDirectives(field.Directives),
                        field.Location));
                }

            }

            return nodes;
        }

        private OperationTypeDefinitions ConvertOperationTypeDefinitions(SyntaxOperationTypeDefinitionNodeList operationTypes)
        {
            var nodes = new OperationTypeDefinitions();

            foreach (var operationType in operationTypes)
            {
                if (nodes.ContainsKey(operationType.Operation))
                {
                    _schema.NonFatalException(ValidationException.SchemaDefinitionMultipleOperation(
                        operationType.Location, 
                        operationType.Operation));
                }
                else
                {
                    nodes.Add(operationType.Operation, new(
                        operationType.Operation,
                        operationType.NamedType,
                        operationType.Location));
                }
            }

            return nodes;
        }

        private InputValueDefinitions ConvertInputValueDefinitions(SyntaxInputValueDefinitionNodeList inputValues, 
                                                                   string elementUsage,
                                                                   string parentNode, 
                                                                   string parentName, 
                                                                   string listType, 
                                                                   string? grandParentNode = null, 
                                                                   string? grandParentName = null)
        {
            var nodes = new InputValueDefinitions();

            foreach (var inputValue in inputValues)
            {
                if (nodes.ContainsKey(inputValue.Name))
                {
                    if ((grandParentNode is not null) && (grandParentName is not null))
                    {
                        _schema.NonFatalException(ValidationException.ListEntryDuplicateName(
                            inputValue.Location,
                            grandParentNode,
                            grandParentName,
                            parentNode,
                            parentName,
                            listType.ToLower(),
                            inputValue.Name));
                    }
                    else
                    {
                        _schema.NonFatalException(ValidationException.ListEntryDuplicateName(
                            inputValue.Location, 
                            parentNode, parentName, 
                            listType.ToLower(), 
                            inputValue.Name));
                    }
                }
                else
                {
                    nodes.Add(inputValue.Name, new InputValueDefinition(
                        inputValue.Description,
                        inputValue.Name,
                        ConvertTypeNode(inputValue.Type),
                        inputValue.DefaultValue,
                        ConvertDirectives(inputValue.Directives),
                        inputValue.Location,
                        elementUsage));
                }
            }

            return nodes;
        }

        private Interfaces ConvertInterfaces(SyntaxNameList names, string parentNode, string parentName)
        {
            var nodes = new Interfaces();

            foreach (var name in names)
            {
                if (nodes.ContainsKey(name.Name))
                {
                    _schema.NonFatalException(ValidationException.ListEntryDuplicateName(
                        name.Location, 
                        parentNode, 
                        parentName, 
                        "interface", 
                        name.Name));
                }
                else
                    nodes.Add(name.Name, new(name.Name, name.Location));
            }

            return nodes;
        }

        private MemberTypes ConvertMemberTypes(SyntaxNameList names, string parentNode, string parentName)
        {
            var nodes = new MemberTypes();

            foreach (var name in names)
            {
                if (nodes.ContainsKey(name.Name))
                {
                    _schema.NonFatalException(ValidationException.ListEntryDuplicateName(
                        name.Location, 
                        parentNode, 
                        parentName, 
                        "member type", 
                        name.Name));
                }
                else
                    nodes.Add(name.Name, new(name.Name, name.Location));
            }

            return nodes;
        }

        private EnumValueDefinitions ConvertEnumValueDefinitions(string enumTypeName, SyntaxEnumValueDefinitionList enumValues)
        {
            var nodes = new EnumValueDefinitions();

            foreach (var enumValue in enumValues)
            {
                if (nodes.ContainsKey(enumValue.Name))
                    _schema.NonFatalException(ValidationException.EnumValueAlreadyDefined(enumValue.Location, enumValue.Name, enumTypeName));
                else
                {
                    nodes.Add(enumValue.Name, new(
                        enumValue.Description,
                        enumValue.Name,
                        ConvertDirectives(enumValue.Directives),
                        enumValue.Location));
                }
            }

            return nodes;
        }
    }
}
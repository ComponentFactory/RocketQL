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
                _schema._schemas.Add(new()
                {
                    Description = schema.Description,
                    Directives = ConvertDirectives(schema.Directives),
                    Operations = ConvertOperationTypeDefinitions(schema.OperationTypes),
                    Location = schema.Location
                }); ;
            }
        }

        public void VisitDirectiveDefinition(SyntaxDirectiveDefinitionNode directive)
        {
            if (_schema._directives.ContainsKey(directive.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(directive.Location, "Directive", directive.Name));
            else
            {
                _schema._directives.Add(directive.Name, new()
                {
                    Description = directive.Description,
                    Name = directive.Name,
                    Arguments = ConvertInputValueDefinitions(directive.Arguments, "Argument", "Directive", directive.Name, "Argument"),
                    Repeatable = directive.Repeatable,
                    DirectiveLocations = directive.DirectiveLocations,
                    Location = directive.Location
                });
            }
        }

        public void VisitScalarTypeDefinition(SyntaxScalarTypeDefinitionNode scalarType)
        {
            if (_schema._types.ContainsKey(scalarType.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(scalarType.Location, "Scalar", scalarType.Name));
            else
            {
                _schema._types.Add(scalarType.Name, new ScalarTypeDefinition()
                {
                    Description = scalarType.Description,
                    Name = scalarType.Name,
                    Directives = ConvertDirectives(scalarType.Directives),
                    Location = scalarType.Location
                }); ;
            }
        }

        public void VisitObjectTypeDefinition(SyntaxObjectTypeDefinitionNode objectType)
        {
            if (_schema._types.ContainsKey(objectType.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(objectType.Location, "Object", objectType.Name));
            else
            {
                _schema._types.Add(objectType.Name, new ObjectTypeDefinition()
                {
                    Description = objectType.Description,
                    Name = objectType.Name,
                    ImplementsInterfaces = ConvertInterfaces(objectType.ImplementsInterfaces, "Object", objectType.Name),
                    Directives = ConvertDirectives(objectType.Directives),
                    Fields = ConvertFieldDefinitions(objectType.Fields, "Object", objectType.Name),
                    Location = objectType.Location
                }); ;
            }
        }

        public void VisitInterfaceTypeDefinition(SyntaxInterfaceTypeDefinitionNode interfaceType)
        {
            if (_schema._types.ContainsKey(interfaceType.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(interfaceType.Location, "Interface", interfaceType.Name));
            else
            {
                _schema._types.Add(interfaceType.Name, new InterfaceTypeDefinition()
                {
                    Description = interfaceType.Description,
                    Name = interfaceType.Name,
                    ImplementsInterfaces = ConvertInterfaces(interfaceType.ImplementsInterfaces, "Interface", interfaceType.Name),
                    Directives = ConvertDirectives(interfaceType.Directives),
                    Fields = ConvertFieldDefinitions(interfaceType.Fields, "Interface", interfaceType.Name),
                    Location = interfaceType.Location
                });
            }
        }

        public void VisitUnionTypeDefinition(SyntaxUnionTypeDefinitionNode unionType)
        {
            if (_schema._types.ContainsKey(unionType.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(unionType.Location, "Union", unionType.Name));
            else
            {
                _schema._types.Add(unionType.Name, new UnionTypeDefinition()
                {
                    Description = unionType.Description,
                    Name = unionType.Name,
                    Directives = ConvertDirectives(unionType.Directives),
                    MemberTypes = ConvertMemberTypes(unionType.MemberTypes, "Union", unionType.Name),
                    Location = unionType.Location
                });
            }
        }

        public void VisitEnumTypeDefinition(SyntaxEnumTypeDefinitionNode enumType)
        {
            if (_schema._types.ContainsKey(enumType.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(enumType.Location, "Enum", enumType.Name));
            else
            {
                _schema._types.Add(enumType.Name, new EnumTypeDefinition()
                {
                    Description = enumType.Description,
                    Name = enumType.Name,
                    Directives = ConvertDirectives(enumType.Directives),
                    EnumValues = ConvertEnumValueDefinitions(enumType.Name, enumType.EnumValues),
                    Location = enumType.Location
                });

            }
        }

        public void VisitInputObjectTypeDefinition(SyntaxInputObjectTypeDefinitionNode inputObjectType)
        {
            if (_schema._types.ContainsKey(inputObjectType.Name))
                _schema.NonFatalException(ValidationException.NameAlreadyDefined(inputObjectType.Location, "Input object", inputObjectType.Name));
            else
            {
                _schema._types.Add(inputObjectType.Name, new InputObjectTypeDefinition()
                {
                    Description = inputObjectType.Description,
                    Name = inputObjectType.Name,
                    Directives = ConvertDirectives(inputObjectType.Directives),
                    InputFields = ConvertInputValueDefinitions(inputObjectType.InputFields, "Input field", "Field", inputObjectType.Name, "Argument"),
                    Location = inputObjectType.Location
                });
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
                                _schema.NonFatalException(ValidationException.ExtendSchemaOperationAlreadyDefined(operationType.Location, operationType.Operation));
                            else
                            {
                                schemaType.Operations.Add(operationType.Operation, new()
                                {
                                    Operation = operationType.Operation,
                                    NamedType = operationType.NamedType,
                                    Definition = null,
                                    Location = operationType.Location
                                });
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
                        _schema.NonFatalException(ValidationException.ExtendObjectInterfaceMandatory(extendObjectType.Location, objectType.OutputElement, extendObjectType.Name));
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
                                    objectType.ImplementsInterfaces.Add(extendImplementsInterface.Name, new()
                                    {
                                        Name = extendImplementsInterface.Name,
                                        Definition = null,
                                        Location = extendImplementsInterface.Location
                                    });
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
                _schema.NonFatalException(ValidationException.TypeNotDefinedForExtend(extendInterfaceType.Location, "Interface", extendInterfaceType.Name));
            else
            {
                if (typeDefinition is not InterfaceTypeDefinition interfaceType)
                    _schema.NonFatalException(ValidationException.IncorrectTypeForExtend(typeDefinition, "Interface"));
                else
                {
                    if ((extendInterfaceType.ImplementsInterfaces.Count == 0) &&
                        (extendInterfaceType.Directives.Count == 0) &&
                        (extendInterfaceType.Fields.Count == 0))
                        _schema.NonFatalException(ValidationException.ExtendObjectInterfaceMandatory(extendInterfaceType.Location, interfaceType.OutputElement, extendInterfaceType.Name));
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
                                    interfaceType.ImplementsInterfaces.Add(extendImplementsInterface.Name, new()
                                    {
                                        Name = extendImplementsInterface.Name,
                                        Definition = null,
                                        Location = extendImplementsInterface.Location
                                    });
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
                                    _schema.NonFatalException(ValidationException.ExtendUnionAlreadyDefined(extendUnionType.Location, extendMemberType.Name, extendUnionType.Name));
                                else
                                {
                                    unionType.MemberTypes.Add(extendMemberType.Name, new()
                                    {
                                        Name = extendMemberType.Name,
                                        Definition = null,
                                        Location = extendMemberType.Location
                                    });
                                }
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
                                    _schema.NonFatalException(ValidationException.ExtendEnumValueAlreadyDefined(extendEnumValue.Location, extendEnumValue.Name, extendEnumType.Name));
                                else
                                {
                                    if (!enumType.EnumValues.TryGetValue(extendEnumValue.Name, out var existingEnumValue))
                                    {
                                        enumType.EnumValues.Add(extendEnumValue.Name, new()
                                        {
                                            Description = extendEnumValue.Description,
                                            Name = extendEnumValue.Name,
                                            Directives = ConvertDirectives(extendEnumValue.Directives),
                                            Location = extendEnumValue.Location
                                        });
                                    }
                                    else
                                    {
                                        if (extendEnumValue.Directives.Count == 0)
                                            _schema.NonFatalException(ValidationException.ExtendExistingEnumValueUnchanged(extendEnumValue.Location, extendEnumValue.Name, extendEnumType.Name));
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
                _schema.NonFatalException(ValidationException.TypeNotDefinedForExtend(extendInputObjectType.Location, "Input object", extendInputObjectType.Name));
            else
            {
                if (typeDefinition is not InputObjectTypeDefinition inputObjectType)
                    _schema.NonFatalException(ValidationException.IncorrectTypeForExtend(typeDefinition, "Input object"));
                else
                {
                    if ((extendInputObjectType.Directives.Count == 0) && (extendInputObjectType.InputFields.Count == 0))
                        _schema.NonFatalException(ValidationException.ExtendInputObjectMandatory(extendInputObjectType.Location, inputObjectType.OutputElement, extendInputObjectType.Name));
                    else
                    {
                        if (extendInputObjectType.Directives.Count > 0)
                            inputObjectType.Directives.AddRange(ConvertDirectives(extendInputObjectType.Directives));

                        ExtendInputFields(extendInputObjectType.Name, extendInputObjectType.InputFields, inputObjectType.InputFields, "Extend input object");
                    }
                }
            }
        }

        private void ExtendFieldsWithArguments(string extendName, SyntaxFieldDefinitionNodeList extendFields, FieldDefinitions existingFields, string errorType)
        {
            if (extendFields.Count > 0)
            {
                HashSet<string> fieldNames = [];
                foreach (var extendField in extendFields)
                {
                    if (fieldNames.Contains(extendField.Name))
                        _schema.NonFatalException(ValidationException.ExtendFieldAlreadyDefined(extendField.Location, extendField.Name, errorType, extendName));
                    else
                    {
                        if (!existingFields.TryGetValue(extendField.Name, out var existingField))
                        {
                            existingFields.Add(extendField.Name, new()
                            {
                                Description = extendField.Description,
                                Name = extendField.Name,
                                Arguments = ConvertInputValueDefinitions(extendField.Arguments, "Argument", "Field", extendField.Name, "Argument", "Object", extendName),
                                Type = ConvertTypeNode(extendField.Type),
                                Directives = ConvertDirectives(extendField.Directives),
                                Location = extendField.Location
                            });
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
                                    _schema.NonFatalException(ValidationException.ExtendFieldArgumentAlreadyDefined(extendField.Location,
                                                                                                                    extendField.Name,
                                                                                                                    extendArgument.Name,
                                                                                                                    errorType,
                                                                                                                    extendName));
                                else
                                {
                                    if (!existingField.Arguments.TryGetValue(extendArgument.Name, out var existingArgument))
                                    {
                                        existingField.Arguments.Add(extendArgument.Name, new()
                                        {
                                            Description = extendArgument.Description,
                                            Name = extendArgument.Name,
                                            Type = ConvertTypeNode(extendArgument.Type),
                                            DefaultValue = extendArgument.DefaultValue,
                                            Directives = ConvertDirectives(extendArgument.Directives),
                                            Location = extendArgument.Location,
                                            ElementUsage = "Argument"
                                        });

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
                                _schema.NonFatalException(ValidationException.ExtendExistingFieldUnchanged(extendField.Location, extendField.Name, errorType, extendName));
                        }
                    }

                    fieldNames.Add(extendField.Name);
                }
            }
        }

        private void ExtendInputFields(string extendName, SyntaxInputValueDefinitionNodeList extendInputFields, InputValueDefinitions existingInputFields, string errorType)
        {
            if (extendInputFields.Count > 0)
            {
                HashSet<string> inputFieldNames = [];
                foreach (var extendInputField in extendInputFields)
                {
                    if (inputFieldNames.Contains(extendInputField.Name))
                        _schema.NonFatalException(ValidationException.ExtendInputFieldAlreadyDefined(extendInputField.Location, extendInputField.Name, errorType, extendName));
                    else
                    {
                        if (!existingInputFields.TryGetValue(extendInputField.Name, out var existingField))
                        {
                            existingInputFields.Add(extendInputField.Name, new()
                            {
                                Description = extendInputField.Description,
                                Name = extendInputField.Name,
                                Type = ConvertTypeNode(extendInputField.Type),
                                DefaultValue = extendInputField.DefaultValue,
                                Directives = ConvertDirectives(extendInputField.Directives),
                                Location = extendInputField.Location,
                                ElementUsage = "Input field"
                            });
                        }
                        else
                        {
                            if (extendInputField.Directives.Count == 0)
                                _schema.NonFatalException(ValidationException.ExtendExistingInputFieldUnchanged(extendInputField.Location, extendInputField.Name, errorType, extendName));
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
                    _schema.NonFatalException(ValidationException.ListEntryDuplicateName(field.Location, parentNode, parentName, "field", field.Name));
                else
                {
                    nodes.Add(field.Name, new()
                    {
                        Description = field.Description,
                        Name = field.Name,
                        Arguments = ConvertInputValueDefinitions(field.Arguments, "Argument", "Field", field.Name, "Argument", parentNode, parentName),
                        Type = ConvertTypeNode(field.Type),
                        Directives = ConvertDirectives(field.Directives),
                        Location = field.Location
                    });
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
                    _schema.NonFatalException(ValidationException.SchemaDefinitionMultipleOperation(operationType.Location, operationType.Operation));
                else
                {
                    nodes.Add(operationType.Operation, new()
                    {
                        Operation = operationType.Operation,
                        NamedType = operationType.NamedType,
                        Definition = null,
                        Location = operationType.Location
                    });
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
                        _schema.NonFatalException(ValidationException.ListEntryDuplicateName(inputValue.Location,
                                                                                             grandParentNode,
                                                                                             grandParentName,
                                                                                             parentNode,
                                                                                             parentName,
                                                                                             listType.ToLower(),
                                                                                             inputValue.Name));
                    else
                        _schema.NonFatalException(ValidationException.ListEntryDuplicateName(inputValue.Location, parentNode, parentName, listType.ToLower(), inputValue.Name));
                }
                else
                {
                    nodes.Add(inputValue.Name, new InputValueDefinition()
                    {
                        Description = inputValue.Description,
                        Name = inputValue.Name,
                        Type = ConvertTypeNode(inputValue.Type),
                        DefaultValue = inputValue.DefaultValue,
                        Directives = ConvertDirectives(inputValue.Directives),
                        Location = inputValue.Location,
                        ElementUsage = elementUsage
                    });
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
                    _schema.NonFatalException(ValidationException.ListEntryDuplicateName(name.Location, parentNode, parentName, "interface", name.Name));
                else
                {
                    nodes.Add(name.Name, new()
                    {
                        Name = name.Name,
                        Definition = null,
                        Location = name.Location
                    });
                }
            }

            return nodes;
        }

        private MemberTypes ConvertMemberTypes(SyntaxNameList names, string parentNode, string parentName)
        {
            var nodes = new MemberTypes();

            foreach (var name in names)
            {
                if (nodes.ContainsKey(name.Name))
                    _schema.NonFatalException(ValidationException.ListEntryDuplicateName(name.Location, parentNode, parentName, "member type", name.Name));
                else
                {
                    nodes.Add(name.Name, new()
                    {
                        Name = name.Name,
                        Definition = null,
                        Location = name.Location
                    });
                }
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
                    nodes.Add(enumValue.Name, new()
                    {
                        Description = enumValue.Description,
                        Name = enumValue.Name,
                        Directives = ConvertDirectives(enumValue.Directives),
                        Location = enumValue.Location
                    });
                }
            }

            return nodes;
        }
    }
}
using System.Xml.Linq;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private SchemaConverter? _schemaConverter = null;
    private SchemaConverter Converter => _schemaConverter ??= new SchemaConverter(this);

    private class SchemaConverter(Schema schema) : ISyntaxNodeVisitors
    {
        private readonly Schema _schema = schema;

        public void Visit()
        {
            ((ISyntaxNodeVisitors)this).Visit(_schema._nodes);
        }

        public void VisitSchemaDefinition(SyntaxSchemaDefinitionNode schema)
        {
            if (_schema.Schemas.Count > 0)
                throw ValidationException.SchemaDefinitionAlreadyDefined(schema.Location);

            _schema.Schemas.Add(new()
            {
                Description = schema.Description,
                Directives = ConvertDirectives(schema.Directives),
                Operations = ConvertOperationTypeDefinitions(schema.OperationTypes),
                Location = schema.Location
            });
        }

        public void VisitDirectiveDefinition(SyntaxDirectiveDefinitionNode directive)
        {
            if (_schema.Directives.ContainsKey(directive.Name))
                throw ValidationException.NameAlreadyDefined(directive.Location, "Directive", directive.Name);

            _schema.Directives.Add(directive.Name, new()
            {
                Description = directive.Description,
                Name = directive.Name,
                Arguments = ConvertInputValueDefinitions(directive.Arguments, "Argument", "Directive", directive.Name, "Argument"),
                Repeatable = directive.Repeatable,
                DirectiveLocations = directive.DirectiveLocations,
                Location = directive.Location
            });
        }

        public void VisitScalarTypeDefinition(SyntaxScalarTypeDefinitionNode scalarType)
        {
            if (_schema.Types.ContainsKey(scalarType.Name))
                throw ValidationException.NameAlreadyDefined(scalarType.Location, "Scalar", scalarType.Name);

            _schema.Types.Add(scalarType.Name, new ScalarTypeDefinition()
            {
                Description = scalarType.Description,
                Name = scalarType.Name,
                Directives = ConvertDirectives(scalarType.Directives),
                Location = scalarType.Location
            });
        }

        public void VisitObjectTypeDefinition(SyntaxObjectTypeDefinitionNode objectType)
        {
            if (_schema.Types.ContainsKey(objectType.Name))
                throw ValidationException.NameAlreadyDefined(objectType.Location, "Object", objectType.Name);

            _schema.Types.Add(objectType.Name, new ObjectTypeDefinition()
            {
                Description = objectType.Description,
                Name = objectType.Name,
                ImplementsInterfaces = ConvertInterfaces(objectType.ImplementsInterfaces),
                Directives = ConvertDirectives(objectType.Directives),
                Fields = ConvertFieldDefinitions(objectType.Fields, "Object", objectType.Name),
                Location = objectType.Location
            });
        }

        public void VisitInterfaceTypeDefinition(SyntaxInterfaceTypeDefinitionNode interfaceType)
        {
            if (_schema.Types.ContainsKey(interfaceType.Name))
                throw ValidationException.NameAlreadyDefined(interfaceType.Location, "Interface", interfaceType.Name);

            _schema.Types.Add(interfaceType.Name, new InterfaceTypeDefinition()
            {
                Description = interfaceType.Description,
                Name = interfaceType.Name,
                ImplementsInterfaces = ConvertInterfaces(interfaceType.ImplementsInterfaces),
                Directives = ConvertDirectives(interfaceType.Directives),
                Fields = ConvertFieldDefinitions(interfaceType.Fields, "Interface", interfaceType.Name),
                Location = interfaceType.Location
            });
        }

        public void VisitUnionTypeDefinition(SyntaxUnionTypeDefinitionNode unionType)
        {
            if (_schema.Types.ContainsKey(unionType.Name))
                throw ValidationException.NameAlreadyDefined(unionType.Location, "Union", unionType.Name);

            _schema.Types.Add(unionType.Name, new UnionTypeDefinition()
            {
                Description = unionType.Description,
                Name = unionType.Name,
                Directives = ConvertDirectives(unionType.Directives),
                MemberTypes = ConvertMemberTypes(unionType.MemberTypes),
                Location = unionType.Location
            });
        }

        public void VisitEnumTypeDefinition(SyntaxEnumTypeDefinitionNode enumType)
        {
            if (_schema.Types.ContainsKey(enumType.Name))
                throw ValidationException.NameAlreadyDefined(enumType.Location, "Enum", enumType.Name);

            _schema.Types.Add(enumType.Name, new EnumTypeDefinition()
            {
                Description = enumType.Description,
                Name = enumType.Name,
                Directives = ConvertDirectives(enumType.Directives),
                EnumValues = ConvertEnumValueDefinitions(enumType.Name, enumType.EnumValues),
                Location = enumType.Location
            });
        }

        public void VisitInputObjectTypeDefinition(SyntaxInputObjectTypeDefinitionNode inputObjectType)
        {
            if (_schema.Types.ContainsKey(inputObjectType.Name))
                throw ValidationException.NameAlreadyDefined(inputObjectType.Location, "Input object", inputObjectType.Name);

            _schema.Types.Add(inputObjectType.Name, new InputObjectTypeDefinition()
            {
                Description = inputObjectType.Description,
                Name = inputObjectType.Name,
                Directives = ConvertDirectives(inputObjectType.Directives),
                InputFields = ConvertInputValueDefinitions(inputObjectType.InputFields, "Input field", "Field", inputObjectType.Name, "Argument"),
                Location = inputObjectType.Location
            });
        }

        public void VisitExtendSchemaDefinition(SyntaxExtendSchemaDefinitionNode extendSchema)
        {
            if (_schema.Schemas.Count == 0)
                throw ValidationException.SchemaNotDefinedForExtend(extendSchema.Location);

            if ((extendSchema.Directives.Count == 0) && (extendSchema.OperationTypes.Count == 0))
                throw ValidationException.ExtendSchemaMandatory(extendSchema.Location);

            var schemaType = _schema.Schemas[0];

            if (extendSchema.Directives.Count > 0)
                schemaType.Directives.AddRange(ConvertDirectives(extendSchema.Directives));

            if (extendSchema.OperationTypes.Count > 0)
            {
                foreach(var operationType in extendSchema.OperationTypes)
                {
                    if (schemaType.Operations.TryGetValue(operationType.Operation, out _))
                        throw ValidationException.ExtendSchemaOperationAlreadyDefined(operationType.Location, operationType.Operation);

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

        public void VisitExtendScalarTypeDefinition(SyntaxExtendScalarTypeDefinitionNode extendScalarType)
        {
            if (!_schema.Types.TryGetValue(extendScalarType.Name, out var typeDefinition))
                throw ValidationException.TypeNotDefinedForExtend(extendScalarType.Location, "Scalar", extendScalarType.Name);

            if (typeDefinition is not ScalarTypeDefinition scalarType)
                throw ValidationException.IncorrectTypeForExtend(typeDefinition, "Scalar");

            if (extendScalarType.Directives.Count == 0)
                throw ValidationException.ExtendScalarMandatory(extendScalarType.Location, extendScalarType.Name);

            scalarType.Directives.AddRange(ConvertDirectives(extendScalarType.Directives));
        }

        public void VisitExtendObjectTypeDefinition(SyntaxExtendObjectTypeDefinitionNode extendObjectType)
        {
            if (!_schema.Types.TryGetValue(extendObjectType.Name, out var typeDefinition))
                throw ValidationException.TypeNotDefinedForExtend(extendObjectType.Location, "Object", extendObjectType.Name);

            if (typeDefinition is not ObjectTypeDefinition objectType)
                throw ValidationException.IncorrectTypeForExtend(typeDefinition, "Object");

            if ((extendObjectType.ImplementsInterfaces.Count == 0) &&
                (extendObjectType.Directives.Count == 0) &&
                (extendObjectType.Fields.Count == 0))
                throw ValidationException.ExtendObjectInterfaceMandatory(extendObjectType.Location, objectType.OutputElement, extendObjectType.Name);

            if (extendObjectType.Directives.Count > 0)
                objectType.Directives.AddRange(ConvertDirectives(extendObjectType.Directives));

            if (extendObjectType.ImplementsInterfaces.Count > 0)
            {
                foreach (var extendImplementsInterface in extendObjectType.ImplementsInterfaces)
                {
                    if (objectType.ImplementsInterfaces.TryGetValue(extendImplementsInterface.Name, out _))
                        throw ValidationException.ExtendImplementAlreadyDefined(extendImplementsInterface.Location, 
                                                                                "Extend object", 
                                                                                extendObjectType.Name, 
                                                                                extendImplementsInterface.Name);

                    objectType.ImplementsInterfaces.Add(extendImplementsInterface.Name, new()
                    {
                        Name = extendImplementsInterface.Name,
                        Definition = null,
                        Location = extendImplementsInterface.Location
                    });
                }
            }

            ExtendFieldsWithArguments(extendObjectType.Name, extendObjectType.Fields, objectType.Fields, "Extend object");
        }

        public void VisitExtendInterfaceTypeDefinition(SyntaxExtendInterfaceTypeDefinitionNode extendInterfaceType)
        {
            if (!_schema.Types.TryGetValue(extendInterfaceType.Name, out var typeDefinition))
                throw ValidationException.TypeNotDefinedForExtend(extendInterfaceType.Location, "Interface", extendInterfaceType.Name);

            if (typeDefinition is not InterfaceTypeDefinition interfaceType)
                throw ValidationException.IncorrectTypeForExtend(typeDefinition, "Interface");

            if ((extendInterfaceType.ImplementsInterfaces.Count == 0) &&
                (extendInterfaceType.Directives.Count == 0) &&
                (extendInterfaceType.Fields.Count == 0))
                throw ValidationException.ExtendObjectInterfaceMandatory(extendInterfaceType.Location, interfaceType.OutputElement, extendInterfaceType.Name);

            if (extendInterfaceType.Directives.Count > 0)
                interfaceType.Directives.AddRange(ConvertDirectives(extendInterfaceType.Directives));

            if (extendInterfaceType.ImplementsInterfaces.Count > 0)
            {
                foreach (var extendImplementsInterface in extendInterfaceType.ImplementsInterfaces)
                {
                    if (interfaceType.ImplementsInterfaces.TryGetValue(extendImplementsInterface.Name, out _))
                        throw ValidationException.ExtendImplementAlreadyDefined(extendImplementsInterface.Location, 
                                                                                "Extend interface", 
                                                                                extendInterfaceType.Name, 
                                                                                extendImplementsInterface.Name);

                    interfaceType.ImplementsInterfaces.Add(extendImplementsInterface.Name, new()
                    {
                        Name = extendImplementsInterface.Name,
                        Definition = null,
                        Location = extendImplementsInterface.Location
                    });
                }
            }

            ExtendFieldsWithArguments(extendInterfaceType.Name, extendInterfaceType.Fields, interfaceType.Fields, "Extend interface");
        }

        public void VisitExtendUnionTypeDefinition(SyntaxExtendUnionTypeDefinitionNode extendUnionType)
        {
            if (!_schema.Types.TryGetValue(extendUnionType.Name, out var typeDefinition))
                throw ValidationException.TypeNotDefinedForExtend(extendUnionType.Location, "Union", extendUnionType.Name);

            if (typeDefinition is not UnionTypeDefinition unionType)
                throw ValidationException.IncorrectTypeForExtend(typeDefinition, "Union");

            if ((extendUnionType.Directives.Count == 0) && (extendUnionType.MemberTypes.Count == 0))
                throw ValidationException.ExtendUnionMandatory(extendUnionType.Location, extendUnionType.Name);

            if (extendUnionType.Directives.Count > 0)
                unionType.Directives.AddRange(ConvertDirectives(extendUnionType.Directives));

            if (extendUnionType.MemberTypes.Count > 0)
            {
                foreach (var extendMemberType in extendUnionType.MemberTypes)
                {
                    if (unionType.MemberTypes.TryGetValue(extendMemberType.Name, out _))
                        throw ValidationException.ExtendUnionAlreadyDefined(extendUnionType.Location, extendMemberType.Name, extendUnionType.Name);

                    unionType.MemberTypes.Add(extendMemberType.Name, new()
                    {
                        Name = extendMemberType.Name,
                        Definition = null,
                        Location = extendMemberType.Location
                    });
                }
            }
        }

        public void VisitExtendEnumDefinition(SyntaxExtendEnumTypeDefinitionNode extendEnumType)
        {
            if (!_schema.Types.TryGetValue(extendEnumType.Name, out var typeDefinition))
                throw ValidationException.TypeNotDefinedForExtend(extendEnumType.Location, "Enum", extendEnumType.Name);

            if (typeDefinition is not EnumTypeDefinition enumType)
                throw ValidationException.IncorrectTypeForExtend(typeDefinition, "Enum");

            if ((extendEnumType.Directives.Count == 0) && (extendEnumType.EnumValues.Count == 0))
                throw ValidationException.ExtendEnumMandatory(extendEnumType.Location, extendEnumType.Name);

            if (extendEnumType.Directives.Count > 0)
                enumType.Directives.AddRange(ConvertDirectives(extendEnumType.Directives));

            if (extendEnumType.EnumValues.Count > 0)
            {
                HashSet<string> extendValues = [];
                foreach(var extendEnumValue in extendEnumType.EnumValues)
                {
                    if (extendValues.Contains(extendEnumValue.Name))
                        throw ValidationException.ExtendEnumValueAlreadyDefined(extendEnumValue.Location, extendEnumValue.Name, extendEnumType.Name);

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
                            throw ValidationException.ExtendExistingEnumValueUnchanged(extendEnumValue.Location, extendEnumValue.Name, extendEnumType.Name);

                        existingEnumValue.Directives.AddRange(ConvertDirectives(extendEnumValue.Directives));
                    }

                    extendValues.Add(extendEnumValue.Name);
                }
            }
        }

        public void VisitExtendInputObjectTypeDefinition(SyntaxExtendInputObjectTypeDefinitionNode extendInputObjectType)
        {
            if (!_schema.Types.TryGetValue(extendInputObjectType.Name, out var typeDefinition))
                throw ValidationException.TypeNotDefinedForExtend(extendInputObjectType.Location, "Input object", extendInputObjectType.Name);

            if (typeDefinition is not InputObjectTypeDefinition inputObjectType)
                throw ValidationException.IncorrectTypeForExtend(typeDefinition, "Input object");

            if ((extendInputObjectType.Directives.Count == 0) && (extendInputObjectType.InputFields.Count == 0))
                throw ValidationException.ExtendInputObjectMandatory(extendInputObjectType.Location, inputObjectType.OutputElement, extendInputObjectType.Name);

            if (extendInputObjectType.Directives.Count > 0)
                inputObjectType.Directives.AddRange(ConvertDirectives(extendInputObjectType.Directives));

            ExtendInputFields(extendInputObjectType.Name, extendInputObjectType.InputFields, inputObjectType.InputFields, "Extend input object");
        }

        private void ExtendFieldsWithArguments(string extendName, SyntaxFieldDefinitionNodeList extendFields, FieldDefinitions existingFields, string errorType)
        {
            if (extendFields.Count > 0)
            {
                HashSet<string> fieldNames = [];
                foreach (var extendField in extendFields)
                {
                    if (fieldNames.Contains(extendField.Name))
                        throw ValidationException.ExtendFieldAlreadyDefined(extendField.Location, extendField.Name, errorType, extendName);

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
                                throw ValidationException.ExtendFieldArgumentAlreadyDefined(extendField.Location, 
                                                                                            extendField.Name, 
                                                                                            extendArgument.Name, 
                                                                                            errorType, 
                                                                                            extendName);

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

                        if (!changed)
                            throw ValidationException.ExtendExistingFieldUnchanged(extendField.Location, extendField.Name, errorType, extendName);
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
                        throw ValidationException.ExtendInputFieldAlreadyDefined(extendInputField.Location, extendInputField.Name, errorType, extendName);

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
                            throw ValidationException.ExtendExistingInputFieldUnchanged(extendInputField.Location, extendInputField.Name, errorType, extendName);

                        existingField.Directives.AddRange(ConvertDirectives(extendInputField.Directives));
                    }

                    inputFieldNames.Add(extendInputField.Name);
                }
            }
        }

        private static FieldDefinitions ConvertFieldDefinitions(SyntaxFieldDefinitionNodeList fields, string parentNode, string parentName)
        {
            var nodes = new FieldDefinitions();

            foreach (var field in fields)
            {
                if (nodes.ContainsKey(field.Name))
                    throw ValidationException.ListEntryDuplicateName(field.Location, parentNode, parentName, "field", field.Name);

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

            return nodes;
        }

        private static OperationTypeDefinitions ConvertOperationTypeDefinitions(SyntaxOperationTypeDefinitionNodeList operationTypes)
        {
            var nodes = new OperationTypeDefinitions();

            foreach (var operationType in operationTypes)
            {
                if (nodes.ContainsKey(operationType.Operation))
                    throw ValidationException.SchemaDefinitionMultipleOperation(operationType.Location, operationType.Operation);

                nodes.Add(operationType.Operation, new()
                {
                    Operation = operationType.Operation,
                    NamedType = operationType.NamedType,
                    Definition = null,
                    Location = operationType.Location
                });
            }

            return nodes;
        }

        private static Directives ConvertDirectives(SyntaxDirectiveNodeList directives)
        {
            var nodes = new Directives();

            foreach (var directive in directives)
            {
                nodes.Add(new()
                {
                    Name = directive.Name,
                    Definition = null,
                    Arguments = ConvertObjectFields(directive.Arguments),
                    Location = directive.Location
                });
            }

            return nodes;
        }

        private static InputValueDefinitions ConvertInputValueDefinitions(SyntaxInputValueDefinitionNodeList inputValues, 
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
                        throw ValidationException.ListEntryDuplicateName(inputValue.Location, 
                                                                         grandParentNode, 
                                                                         grandParentName, 
                                                                         parentNode, 
                                                                         parentName, 
                                                                         listType.ToLower(), 
                                                                         inputValue.Name);
                    else
                        throw ValidationException.ListEntryDuplicateName(inputValue.Location, parentNode, parentName, listType.ToLower(), inputValue.Name);
                }

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

            return nodes;
        }

        private static TypeNode ConvertTypeNode(SyntaxTypeNode node)
        {
            return node switch
            {
                SyntaxTypeNameNode nameNode => new TypeName()
                {
                    Name = nameNode.Name,
                    Definition = null,
                    Location = nameNode.Location,
                },
                SyntaxTypeNonNullNode nonNullNode => new TypeNonNull()
                {
                    Type = ConvertTypeNode(nonNullNode.Type),
                    Location = nonNullNode.Location,
                },
                SyntaxTypeListNode listNode => new TypeList()
                {
                    Type = ConvertTypeNode(listNode.Type),
                    Location = listNode.Location,
                },
                _ => throw ValidationException.UnrecognizedType(node.Location, node.GetType().Name)
            }; ;
        }

        private static ObjectFields ConvertObjectFields(SyntaxObjectFieldNodeList fields)
        {
            var nodes = new ObjectFields();

            foreach (var field in fields)
                nodes.Add(field.Name, field);

            return nodes;
        }

        private static Interfaces ConvertInterfaces(SyntaxNameList names)
        {
            var nodes = new Interfaces();

            foreach (var name in names)
                nodes.Add(name.Name, new()
                {
                    Name = name.Name,
                    Definition = null,
                    Location = name.Location
                });

            return nodes;
        }

        private static MemberTypes ConvertMemberTypes(SyntaxNameList names)
        {
            var nodes = new MemberTypes();

            foreach (var name in names)
                nodes.Add(name.Name, new()
                {
                    Name = name.Name,
                    Definition = null,
                    Location = name.Location
                });

            return nodes;
        }

        private EnumValueDefinitions ConvertEnumValueDefinitions(string enumTypeName, SyntaxEnumValueDefinitionList enumValues)
        {
            var nodes = new EnumValueDefinitions();

            foreach (var enumValue in enumValues)
            {
                if (nodes.ContainsKey(enumValue.Name))
                    throw ValidationException.EnumValueAlreadyDefined(enumValue.Location, enumValue.Name, enumTypeName);

                nodes.Add(enumValue.Name, new()
                {
                    Description = enumValue.Description,
                    Name = enumValue.Name,
                    Directives = ConvertDirectives(enumValue.Directives),
                    Location = enumValue.Location
                });
            }

            return nodes;
        }
    }
}
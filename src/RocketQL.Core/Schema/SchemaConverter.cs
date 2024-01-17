using RocketQL.Core.Enumerations;
using RocketQL.Core.Nodes;
using System.Xml.Linq;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private SchemaConverter? _schemaConverter = null;
    private SchemaConverter Converter => _schemaConverter ??= new SchemaConverter(this);

    private class SchemaConverter(Schema schema) : NodeVisitor, ISyntaxNodeVisitors
    {
        private readonly Schema _schema = schema;

        public void Visit()
        {
            ((ISyntaxNodeVisitors)this).Visit(_schema._nodes);
        }

        public void VisitOperationDefinition(SyntaxOperationDefinitionNode operation)
        {
            _schema.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(operation, "Operation"));
        }

        public void VisitFragmentDefinition(SyntaxFragmentDefinitionNode fragment)
        {
            _schema.NonFatalException(ValidationException.DefinitionNotAllowedInSchema(fragment, "Fragment"));
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
            PushPath($"directive {directive.Name}");

            if (_schema._directives.ContainsKey(directive.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(directive, directive.Name, "Directive", CurrentPath));
            else
                _schema._directives.Add(directive.Name, new(directive.Description,
                                                            directive.Name,
                                                            ConvertInputValueDefinitions(directive.Arguments, "Argument"),
                                                            directive.Repeatable,
                                                            directive.DirectiveLocations,
                                                            directive.Location));
            PopPath();
        }

        public void VisitScalarTypeDefinition(SyntaxScalarTypeDefinitionNode scalarType)
        {
            PushPath($"scalar {scalarType.Name}");

            if (_schema._types.ContainsKey(scalarType.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(scalarType, scalarType.Name, "Scalar", CurrentPath));
            else
                _schema._types.Add(scalarType.Name, new ScalarTypeDefinition(scalarType.Description,
                                                                             scalarType.Name,
                                                                             ConvertDirectives(scalarType.Directives),
                                                                             scalarType.Location));

            PopPath();
        }

        public void VisitObjectTypeDefinition(SyntaxObjectTypeDefinitionNode objectType)
        {
            PushPath($"type {objectType.Name}");

            if (_schema._types.ContainsKey(objectType.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(objectType, objectType.Name, "Object", CurrentPath));
            else
                _schema._types.Add(objectType.Name, new ObjectTypeDefinition(objectType.Description,
                                                                             objectType.Name,
                                                                             ConvertDirectives(objectType.Directives),
                                                                             ConvertInterfaces(objectType, objectType.ImplementsInterfaces),
                                                                             ConvertFieldDefinitions(objectType, objectType.Fields),
                                                                             objectType.Location));

            PopPath();
        }

        public void VisitInterfaceTypeDefinition(SyntaxInterfaceTypeDefinitionNode interfaceType)
        {
            PushPath($"interface {interfaceType.Name}");

            if (_schema._types.ContainsKey(interfaceType.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(interfaceType, interfaceType.Name, "Interface", CurrentPath));
            else
                _schema._types.Add(interfaceType.Name, new InterfaceTypeDefinition(interfaceType.Description,
                                                                                   interfaceType.Name,
                                                                                   ConvertDirectives(interfaceType.Directives),
                                                                                   ConvertInterfaces(interfaceType, interfaceType.ImplementsInterfaces),
                                                                                   ConvertFieldDefinitions(interfaceType, interfaceType.Fields),
                                                                                   interfaceType.Location));

            PopPath();
        }

        public void VisitUnionTypeDefinition(SyntaxUnionTypeDefinitionNode unionType)
        {
            PushPath($"union {unionType.Name}");

            if (_schema._types.ContainsKey(unionType.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(unionType, unionType.Name, "Union", CurrentPath));
            else
                _schema._types.Add(unionType.Name, new UnionTypeDefinition(unionType.Description,
                                                                           unionType.Name,
                                                                           ConvertDirectives(unionType.Directives),
                                                                           ConvertMemberTypes(unionType, unionType.MemberTypes),
                                                                           unionType.Location));

            PopPath();
        }

        public void VisitEnumTypeDefinition(SyntaxEnumTypeDefinitionNode enumType)
        {
            PushPath($"enum {enumType.Name}");

            if (_schema._types.ContainsKey(enumType.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(enumType, enumType.Name, "Enum", CurrentPath));
            else
                _schema._types.Add(enumType.Name, new EnumTypeDefinition(enumType.Description,
                                                                         enumType.Name,
                                                                         ConvertDirectives(enumType.Directives),
                                                                         ConvertEnumValueDefinitions(enumType, enumType.EnumValues),
                                                                         enumType.Location));

            PopPath();
        }

        public void VisitInputObjectTypeDefinition(SyntaxInputObjectTypeDefinitionNode inputObjectType)
        {
            PushPath($"input object {inputObjectType.Name}");

            if (_schema._types.ContainsKey(inputObjectType.Name))
                _schema.NonFatalException(ValidationException.TypeNameAlreadyDefined(inputObjectType, inputObjectType.Name, "Input object", CurrentPath));
            else
                _schema._types.Add(inputObjectType.Name, new InputObjectTypeDefinition(inputObjectType.Description,
                                                                                       inputObjectType.Name,
                                                                                       ConvertDirectives(inputObjectType.Directives),
                                                                                       ConvertInputValueDefinitions(inputObjectType.InputFields, "Input field"),
                                                                                       inputObjectType.Location));

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
                            PushPath($"{operationType.Operation.ToString().ToLower()} {operationType.NamedType}");

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
            PushPath($"extend scalar {extendScalarType.Name}");

            if (!_schema._types.TryGetValue(extendScalarType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.ExtendTypeAlreadyDefined(extendScalarType, extendScalarType.Name, "Scalar", CurrentPath));
            else
            {
                if (typeDefinition is not ScalarTypeDefinition scalarType)
                    _schema.NonFatalException(ValidationException.ExtendIncorrectType(extendScalarType, "Scalar", typeDefinition, CurrentPath));
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
            PushPath($"extend type {extendObjectType.Name}");

            if (!_schema._types.TryGetValue(extendObjectType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.ExtendTypeAlreadyDefined(extendObjectType, extendObjectType.Name, "Object", CurrentPath));
            else
            {
                if (typeDefinition is not ObjectTypeDefinition objectType)
                    _schema.NonFatalException(ValidationException.ExtendIncorrectType(extendObjectType, "Object", typeDefinition, CurrentPath));
                else
                {
                    if ((extendObjectType.ImplementsInterfaces.Count == 0) &&
                        (extendObjectType.Directives.Count == 0) &&
                        (extendObjectType.Fields.Count == 0))
                    {
                        _schema.NonFatalException(ValidationException.ExtendObjectMandatory(extendObjectType, objectType.OutputElement, CurrentPath));
                    }
                    else
                    {
                        if (extendObjectType.Directives.Count > 0)
                            objectType.Directives.AddRange(ConvertDirectives(extendObjectType.Directives));

                        if (extendObjectType.ImplementsInterfaces.Count > 0)
                        {
                            foreach (var extendImplementsInterface in extendObjectType.ImplementsInterfaces)
                            {
                                PushPath($"implement {extendImplementsInterface.Name}");

                                if (objectType.ImplementsInterfaces.TryGetValue(extendImplementsInterface.Name, out _))
                                    _schema.NonFatalException(ValidationException.ExtendObjectImplementAlreadyDefined(extendImplementsInterface,
                                                                                                                      extendObjectType.Name,
                                                                                                                      extendImplementsInterface.Name,
                                                                                                                      CurrentPath));
                                else
                                    objectType.ImplementsInterfaces.Add(extendImplementsInterface.Name, new(extendImplementsInterface.Name,
                                                                                                            extendImplementsInterface.Location));

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
            PushPath($"extend interface {extendInterfaceType.Name}");

            if (!_schema._types.TryGetValue(extendInterfaceType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.ExtendTypeAlreadyDefined(extendInterfaceType, extendInterfaceType.Name, "Interface", CurrentPath));
            else
            {
                if (typeDefinition is not InterfaceTypeDefinition interfaceType)
                    _schema.NonFatalException(ValidationException.ExtendIncorrectType(extendInterfaceType, "Interface", typeDefinition, CurrentPath));
                else
                {
                    if ((extendInterfaceType.ImplementsInterfaces.Count == 0) &&
                        (extendInterfaceType.Directives.Count == 0) &&
                        (extendInterfaceType.Fields.Count == 0))
                    {
                        _schema.NonFatalException(ValidationException.ExtendInterfaceMandatory(extendInterfaceType, interfaceType.OutputElement, CurrentPath));
                    }
                    else
                    {

                        if (extendInterfaceType.Directives.Count > 0)
                            interfaceType.Directives.AddRange(ConvertDirectives(extendInterfaceType.Directives));

                        if (extendInterfaceType.ImplementsInterfaces.Count > 0)
                        {
                            foreach (var extendImplementsInterface in extendInterfaceType.ImplementsInterfaces)
                            {
                                PushPath($"implement {extendImplementsInterface.Name}");

                                if (interfaceType.ImplementsInterfaces.TryGetValue(extendImplementsInterface.Name, out _))
                                    _schema.NonFatalException(ValidationException.ExtendInterfaceImplementAlreadyDefined(extendImplementsInterface,
                                                                                                                         extendInterfaceType.Name,
                                                                                                                         extendImplementsInterface.Name,
                                                                                                                         CurrentPath));
                                else
                                    interfaceType.ImplementsInterfaces.Add(extendImplementsInterface.Name, new(extendImplementsInterface.Name,
                                                                                                               extendImplementsInterface.Location));

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
            PushPath($"extend union {extendUnionType.Name}");

            if (!_schema._types.TryGetValue(extendUnionType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.ExtendTypeAlreadyDefined(extendUnionType, extendUnionType.Name, "Union", CurrentPath));
            else
            {
                if (typeDefinition is not UnionTypeDefinition unionType)
                    _schema.NonFatalException(ValidationException.ExtendIncorrectType(extendUnionType, "Union", typeDefinition, CurrentPath));
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
                                PushPath($"member type {extendMemberType.Name}");

                                if (unionType.MemberTypes.TryGetValue(extendMemberType.Name, out _))
                                    _schema.NonFatalException(ValidationException.ExtendUnionAlreadyDefined(extendUnionType,
                                                                                                            extendUnionType.Name,
                                                                                                            extendMemberType.Name,
                                                                                                            CurrentPath));
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
            PushPath($"extend enum {extendEnumType.Name}");

            if (!_schema._types.TryGetValue(extendEnumType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.ExtendTypeAlreadyDefined(extendEnumType, extendEnumType.Name, "Enum", CurrentPath));
            else
            {
                if (typeDefinition is not EnumTypeDefinition enumType)
                    _schema.NonFatalException(ValidationException.ExtendIncorrectType(extendEnumType, "Enum", typeDefinition, CurrentPath));
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
                            HashSet<string> extendValues = [];
                            foreach (var extendEnumValue in extendEnumType.EnumValues)
                            {
                                PushPath($"enum value {extendEnumValue.Name}");

                                if (extendValues.Contains(extendEnumValue.Name))
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
                                            _schema.NonFatalException(ValidationException.ExtendExistingEnumValueUnchanged(extendEnumValue,
                                                                                                                           extendEnumType.Name,
                                                                                                                           extendEnumValue.Name,
                                                                                                                           CurrentPath));
                                        else
                                            existingEnumValue.Directives.AddRange(ConvertDirectives(extendEnumValue.Directives));
                                    }

                                    extendValues.Add(extendEnumValue.Name);
                                }

                                PopPath();
                            }
                        }
                    }
                }
            }

            PopPath();
        }

        public void VisitExtendInputObjectTypeDefinition(SyntaxExtendInputObjectTypeDefinitionNode extendInputObjectType)
        {
            PushPath($"extend inpt object {extendInputObjectType.Name}");

            if (!_schema._types.TryGetValue(extendInputObjectType.Name, out var typeDefinition))
                _schema.NonFatalException(ValidationException.ExtendTypeAlreadyDefined(extendInputObjectType, extendInputObjectType.Name, "Input object", CurrentPath));
            else
            {
                if (typeDefinition is not InputObjectTypeDefinition inputObjectType)
                    _schema.NonFatalException(ValidationException.ExtendIncorrectType(extendInputObjectType, "Input object", typeDefinition, CurrentPath));
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
                HashSet<string> fieldNames = [];
                foreach (var extendField in extendFields)
                {
                    PushPath($"field {extendField.Name}");

                    if (fieldNames.Contains(extendField.Name))
                        _schema.NonFatalException(ValidationException.DuplicateName(extendField, "field", extendField.Name, CurrentPath));
                    else
                    {
                        if (!existingFields.TryGetValue(extendField.Name, out var existingField))
                        {
                            existingFields.Add(extendField.Name, new(extendField.Description,
                                                                     extendField.Name,
                                                                     ConvertInputValueDefinitions(extendField.Arguments, "Argument"),
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
                                PushPath($"argument {extendArgument.Name}");

                                if (argumentNames.Contains(extendArgument.Name))
                                    _schema.NonFatalException(ValidationException.DuplicateName(extendArgument, "argument", extendArgument.Name, CurrentPath));
                                else
                                {
                                    if (!existingField.Arguments.TryGetValue(extendArgument.Name, out var existingArgument))
                                    {
                                        existingField.Arguments.Add(extendArgument.Name, new(extendArgument.Description,
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

                                PopPath();
                            }

                            if (!changed)
                                _schema.NonFatalException(ValidationException.ExtendExistingFieldUnchanged(extendField, CurrentPath));
                        }
                    }

                    fieldNames.Add(extendField.Name);
                    PopPath();
                }
            }
        }

        private void ExtendInputFields(SyntaxInputValueDefinitionNodeList extendInputFields, InputValueDefinitions existingInputFields)
        {
            if (extendInputFields.Count > 0)
            {
                HashSet<string> inputFieldNames = [];
                foreach (var extendInputField in extendInputFields)
                {
                    PushPath($"input field {extendInputField.Name}");

                    if (inputFieldNames.Contains(extendInputField.Name))
                        _schema.NonFatalException(ValidationException.DuplicateName(extendInputField, "input field", extendInputField.Name, CurrentPath));
                    else
                    {
                        if (!existingInputFields.TryGetValue(extendInputField.Name, out var existingField))
                        {
                            existingInputFields.Add(extendInputField.Name, new(extendInputField.Description,
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
                PushPath($"field {field.Name}");

                if (nodes.ContainsKey(field.Name))
                    _schema.NonFatalException(ValidationException.DuplicateName(parentNode, "field", field.Name, CurrentPath));
                else
                    nodes.Add(field.Name, new(field.Description,
                                              field.Name,
                                              ConvertInputValueDefinitions(field.Arguments, "Argument"),
                                              ConvertTypeNode(field.Type),
                                              ConvertDirectives(field.Directives),
                                              field.Location));

                PopPath();
            }

            return nodes;
        }

        private OperationTypeDefinitions ConvertOperationTypeDefinitions(SyntaxOperationTypeDefinitionNodeList operationTypes)
        {
            var nodes = new OperationTypeDefinitions();

            foreach (var operationType in operationTypes)
            {
                PushPath($"{operationType.Operation.ToString().ToLower()} {operationType.NamedType}");

                if (nodes.ContainsKey(operationType.Operation))
                    _schema.NonFatalException(ValidationException.SchemaDefinitionMultipleOperation(operationType, CurrentPath));
                else
                    nodes.Add(operationType.Operation, new(operationType.Operation, operationType.NamedType, operationType.Location));

                PopPath();
            }

            return nodes;
        }

        private InputValueDefinitions ConvertInputValueDefinitions(SyntaxInputValueDefinitionNodeList inputValues, string elementUsage)
        {
            var nodes = new InputValueDefinitions();

            foreach (var inputValue in inputValues)
            {
                PushPath($"{elementUsage.ToLower()} {inputValue.Name}");

                if (nodes.ContainsKey(inputValue.Name))
                    _schema.NonFatalException(ValidationException.DuplicateName(inputValue, elementUsage, inputValue.Name, CurrentPath));
                else
                    nodes.Add(inputValue.Name, new InputValueDefinition(inputValue.Description,
                                                                        inputValue.Name,
                                                                        ConvertTypeNode(inputValue.Type),
                                                                        inputValue.DefaultValue,
                                                                        ConvertDirectives(inputValue.Directives),
                                                                        inputValue.Location,
                                                                        elementUsage));

                PopPath();
            }

            return nodes;
        }

        private Interfaces ConvertInterfaces(LocationNode parentNode, SyntaxNameList names)
        {
            var nodes = new Interfaces();

            foreach (var name in names)
            {
                PushPath($"implements {name.Name}");

                if (nodes.ContainsKey(name.Name))
                    _schema.NonFatalException(ValidationException.DuplicateName(parentNode, "interface", name.Name, CurrentPath));
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
                PushPath($"member type {name.Name}");

                if (nodes.ContainsKey(name.Name))
                    _schema.NonFatalException(ValidationException.DuplicateName(parentNode, "member type", name.Name, CurrentPath));
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
                PushPath($"enum value {enumValue.Name}");

                if (nodes.ContainsKey(enumValue.Name))
                    _schema.NonFatalException(ValidationException.DuplicateName(parentNode, "enum value", enumValue.Name, CurrentPath));
                else
                    nodes.Add(enumValue.Name, new(enumValue.Description,
                                                  enumValue.Name,
                                                  ConvertDirectives(enumValue.Directives),
                                                  enumValue.Location));

                PopPath();
            }

            return nodes;
        }

        private Directives ConvertDirectives(SyntaxDirectiveNodeList directives)
        {
            var nodes = new Directives();

            foreach (var directive in directives)
            {
                PushPath($"directive {directive.Name}");
                nodes.Add(new(directive.Name, ConvertObjectFields(directive, directive.Arguments, "argument"), directive.Location));
                PopPath();
            }

            return nodes;
        }

        private ObjectFields ConvertObjectFields(LocationNode parentNode, SyntaxObjectFieldNodeList fields, string elementUsage)
        {
            var nodes = new ObjectFields();

            foreach (var field in fields)
            {
                PushPath($"{elementUsage.ToLower()} {field.Name}");

                if (nodes.ContainsKey(field.Name))
                    throw ValidationException.DuplicateName(parentNode, elementUsage, field.Name, CurrentPath);
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

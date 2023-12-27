namespace RocketQL.Core.Base;

public partial class Schema
{
    private class SchemaConverter(Schema schema) : ISyntaxNodeVisitors
    {
        private readonly Schema _schema = schema;

        public void Visit()
        {
            ((ISyntaxNodeVisitors)this).Visit(_schema._syntaxNodes);
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
                Arguments = ConvertInputValueDefinitions(directive.Arguments, "Directive", directive.Name, "Argument"),
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
                EnumValues = ConvertEnumValueDefinitions(enumType.EnumValues),
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
                InputFields = ConvertInputValueDefinitions(inputObjectType.InputFields, "Field", inputObjectType.Name, "Argument"),
                Location = inputObjectType.Location
            });
        }

        private FieldDefinitions ConvertFieldDefinitions(SyntaxFieldDefinitionNodeList fields, string? grandParentNode = null, string? grandParentName = null)
        {
            var nodes = new FieldDefinitions();

            foreach (var field in fields)
                nodes.Add(field.Name, new()
                {
                    Description = field.Description,
                    Name = field.Name,
                    Arguments = ConvertInputValueDefinitions(field.Arguments, "Field", field.Name, "Argument", grandParentNode, grandParentName),
                    Type = ConvertTypeNode(field.Type),
                    Definition = null,
                    Directives = ConvertDirectives(field.Directives),
                    Location = field.Location
                });

            return nodes;
        }

        private static OperationTypeDefinitions ConvertOperationTypeDefinitions(SyntaxOperationTypeDefinitionNodeList operationTypes)
        {
            var nodes = new OperationTypeDefinitions();

            foreach (var operationType in operationTypes)
            {
                if (nodes.ContainsKey(operationType.Operation))
                    throw ValidationException.OperationTypeAlreadyDefined(operationType.Location, operationType.Operation);

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
                nodes.Add(directive.Name, new()
                {
                    Name = directive.Name,
                    Definition = null,
                    Arguments = ConvertObjectFields(directive.Arguments),
                    Location = directive.Location
                });
            }

            return nodes;
        }

        private static InputValueDefinitions ConvertInputValueDefinitions(SyntaxInputValueDefinitionNodeList inputValues, string parentNode, string parentName, string listType, string? grandParentNode = null, string? grandParentName = null)
        {
            var nodes = new InputValueDefinitions();

            foreach (var inputValue in inputValues)
            {
                if (nodes.ContainsKey(inputValue.Name))
                {
                    if ((grandParentNode is not null) && (grandParentName is not null))
                        throw ValidationException.ListEntryDuplicateName(inputValue.Location, grandParentNode, grandParentName, parentNode, parentName, listType.ToLower(), inputValue.Name);
                    else
                        throw ValidationException.ListEntryDuplicateName(inputValue.Location, parentNode, parentName, listType.ToLower(), inputValue.Name);
                }

                nodes.Add(inputValue.Name, new()
                {
                    Description = inputValue.Description,
                    Name = inputValue.Name,
                    Type = ConvertTypeNode(inputValue.Type),
                    DefaultValue = inputValue.DefaultValue,
                    Directives = ConvertDirectives(inputValue.Directives),
                    Location = inputValue.Location
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
                    NonNull = nameNode.NonNull,
                    Location = nameNode.Location,
                },
                SyntaxTypeListNode listNode => new TypeList()
                {
                    Type = ConvertTypeNode(listNode.Type),
                    NonNull = listNode.NonNull,
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
                nodes.Add(name, new()
                {
                    Name = name,
                    Definition = null,
                    Location = new()
                });

            return nodes;
        }

        private static MemberTypes ConvertMemberTypes(SyntaxNameList names)
        {
            var nodes = new MemberTypes();

            foreach (var name in names)
                nodes.Add(name, new()
                {
                    Name = name,
                    Definition = null,
                    Location = new()
                });

            return nodes;
        }

        private EnumValueDefinitions ConvertEnumValueDefinitions(SyntaxEnumValueDefinitionList enumValues)
        {
            var nodes = new EnumValueDefinitions();

            foreach (var enumValue in enumValues)
                nodes.Add(enumValue.Name, new()
                {
                    Description = enumValue.Description,
                    Name = enumValue.Name,
                    Directives = ConvertDirectives(enumValue.Directives),
                    Location = enumValue.Location
                });

            return nodes;
        }
    }
}
namespace RocketQL.Core.Base;

public partial class Schema
{
    private SchemaLinker? _schemaLinker = null;
    private SchemaLinker Linker => _schemaLinker ??= new SchemaLinker(this);

    private class SchemaLinker(Schema schema) : IDocumentNodeVisitors
    {
        private readonly Schema _schema = schema;

        public void Visit()
        {
            IDocumentNodeVisitors visitor = this;
            visitor.Visit(_schema._directives.Values);
            visitor.Visit(_schema._types.Values);
            visitor.Visit(_schema._schemas);
        }

        public void VisitOperationDefinition(OperationDefinition operation)
        {
        }

        public void VisitFragmentDefinition(FragmentDefinition fragment)
        {
        }

        public void VisitDirectiveDefinition(DirectiveDefinition directive)
        {
            foreach (var argument in directive.Arguments.Values)
            {
                argument.Parent = directive;

                InterlinkDirectives(argument.Directives, argument, directive);
                InterlinkTypeNode(argument.Type, argument, directive, argument);
            }
        }

        public void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType)
        {
            InterlinkDirectives(scalarType.Directives, scalarType);
        }

        public void VisitObjectTypeDefinition(ObjectTypeDefinition objectType)
        {
            InterlinkDirectives(objectType.Directives, objectType);
            InterlinkInterfaces(objectType.ImplementsInterfaces, objectType);
            InterlinkFields(objectType.Fields, objectType, objectType);
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            InterlinkDirectives(interfaceType.Directives, interfaceType);
            InterlinkInterfaces(interfaceType.ImplementsInterfaces, interfaceType);
            InterlinkFields(interfaceType.Fields, interfaceType, interfaceType);
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
            InterlinkDirectives(unionType.Directives, unionType);
            InterlinkMemberTypes(unionType.MemberTypes, unionType);
        }

        public void VisitEnumTypeDefinition(EnumTypeDefinition enumType)
        {
            InterlinkDirectives(enumType.Directives, enumType);

            foreach (var enumValue in enumType.EnumValues.Values)
            {
                enumValue.Parent = enumType;
                InterlinkDirectives(enumValue.Directives, enumValue, enumType);
            }
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
            InterlinkDirectives(inputObjectType.Directives, inputObjectType);
            InterlinkInputValues(inputObjectType.InputFields, inputObjectType, inputObjectType);
        }

        public void VisitSchemaDefinition(SchemaRoot schemaRoot)
        {
        }

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
            InterlinkDirectives(schemaDefinition.Directives, schemaDefinition);

            foreach(var operationTypeDefinition in schemaDefinition.Operations.Values)
            {
                if (!_schema._types.TryGetValue(operationTypeDefinition.NamedType, out var typeDefinition))
                    FatalException(ValidationException.TypeNotDefinedForSchemaOperation(operationTypeDefinition));

                if (typeDefinition is not ObjectTypeDefinition objectTypeDefinition)
                    FatalException(ValidationException.SchemaOperationTypeNotObject(operationTypeDefinition, typeDefinition!));

                operationTypeDefinition.Definition = typeDefinition as ObjectTypeDefinition;
            }
        }

        private void InterlinkDirectives(Directives directives, DocumentNode parentNode, DocumentNode? grandParentNode = null)
        {
            foreach (var directive in directives)
            {
                directive.Parent = parentNode;

                if (!_schema._directives.TryGetValue(directive.Name, out DirectiveDefinition? directiveDefinition))
                {
                    if (grandParentNode is not null)
                        FatalException(ValidationException.UndefinedDirective(directive, parentNode.OutputElement, parentNode.OutputName, grandParentNode));
                    else
                        FatalException(ValidationException.UndefinedDirective(directive, parentNode));
                }
                else
                {
                    directive.Definition = directiveDefinition;
                    directiveDefinition.References.Add(directive!);
                }
            }
        }

        private void InterlinkInterfaces(Interfaces interfaces, DocumentNode parentNode)
        {
            foreach (var interfaceEntry in interfaces.Values)
            {
                interfaceEntry.Parent = parentNode;

                if (!_schema._types.TryGetValue(interfaceEntry.Name, out TypeDefinition? typeDefinition))
                    FatalException(ValidationException.UndefinedInterface(interfaceEntry, parentNode));

                if (typeDefinition is not InterfaceTypeDefinition interfaceTypeDefinition)
                    FatalException(ValidationException.TypeIsNotAnInterface(interfaceEntry, parentNode, typeDefinition!));
                else
                {
                    interfaceEntry.Definition = interfaceTypeDefinition;
                    interfaceTypeDefinition.References.Add(interfaceEntry!);
                }
            }
        }

        private void InterlinkFields(FieldDefinitions fields, DocumentNode parentNode, DocumentNode rootNode)
        {
            foreach (var field in fields.Values)
            {
                field.Parent = parentNode;

                InterlinkDirectives(field.Directives, field, rootNode);
                InterlinkTypeNode(field.Type, field, rootNode, field);
                InterlinkInputValues(field.Arguments, field, rootNode);
            }
        }

        private void InterlinkInputValues(InputValueDefinitions inputValues, DocumentNode parentNode, DocumentNode rootNode)
        {
            foreach (var inputValue in inputValues.Values)
            {
                inputValue.Parent = parentNode;

                InterlinkDirectives(inputValue.Directives, inputValue, rootNode);
                InterlinkTypeNode(inputValue.Type, inputValue, rootNode, inputValue);
            }
        }

        private void InterlinkTypeNode(TypeNode typeLocation, DocumentNode parentNode, DocumentNode rootNode, DocumentNode typeParentNode)
        {
            typeLocation.Parent = typeParentNode;

            if (typeLocation is TypeList typeList)
                InterlinkTypeNode(typeList.Type, parentNode, rootNode, typeList);
            if (typeLocation is TypeNonNull typeNonNull)
                InterlinkTypeNode(typeNonNull.Type, parentNode, rootNode, typeNonNull);
            else if (typeLocation is TypeName typeName)
            {
                if (!_schema._types.TryGetValue(typeName.Name, out var type))
                    FatalException(ValidationException.UndefinedTypeForListEntry(typeName.Location, typeName.Name, parentNode.OutputElement, parentNode.OutputName, rootNode));
                else
                {
                    typeName.Definition = type;
                    type.References.Add(typeName);
                }
            }
        }

        private void InterlinkMemberTypes(MemberTypes memberTypes, UnionTypeDefinition unionType)
        {
            foreach (var memberType in memberTypes.Values)
            {
                memberType.Parent = unionType;

                if (!_schema._types.TryGetValue(memberType.Name, out TypeDefinition? typeDefinition))
                    FatalException(ValidationException.UndefinedMemberType(memberType, unionType));

                if (typeDefinition is ObjectTypeDefinition objectTypeDefinition)
                {
                    memberType.Definition = objectTypeDefinition;
                    objectTypeDefinition.References.Add(memberType);
                }
                else
                {
                    FatalException(ValidationException.TypeIsNotAnObject(memberType, unionType, typeDefinition!,
                                                                        ((typeDefinition is ScalarTypeDefinition) || (typeDefinition is UnionTypeDefinition)) ? "a" : "an"));
                }
            }
        }
    }
}
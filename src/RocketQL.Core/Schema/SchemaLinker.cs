namespace RocketQL.Core.Base;

public partial class Schema
{
    private SchemaLinker? _schemaLinker = null;
    private SchemaLinker Linker => _schemaLinker ??= new SchemaLinker(this);

    private class SchemaLinker(Schema schema) : LinkerNodeVisitor, IDocumentNodeVisitors
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

                InterlinkDirectives(argument.Directives, argument, directive, _schema._directives);
                InterlinkTypeNode(argument.Type, argument, directive, argument, _schema._types);
            }
        }

        public void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType)
        {
            InterlinkDirectives(scalarType.Directives, scalarType, null, _schema._directives);
        }

        public void VisitObjectTypeDefinition(ObjectTypeDefinition objectType)
        {
            InterlinkDirectives(objectType.Directives, objectType, null, _schema._directives);
            InterlinkInterfaces(objectType.ImplementsInterfaces, objectType);
            InterlinkFields(objectType.Fields, objectType, objectType);
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            InterlinkDirectives(interfaceType.Directives, interfaceType, null, _schema._directives);
            InterlinkInterfaces(interfaceType.ImplementsInterfaces, interfaceType);
            InterlinkFields(interfaceType.Fields, interfaceType, interfaceType);
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
            InterlinkDirectives(unionType.Directives, unionType, null, _schema._directives);
            InterlinkMemberTypes(unionType.MemberTypes, unionType);
        }

        public void VisitEnumTypeDefinition(EnumTypeDefinition enumType)
        {
            InterlinkDirectives(enumType.Directives, enumType, null, _schema._directives);

            foreach (var enumValue in enumType.EnumValues.Values)
            {
                enumValue.Parent = enumType;
                InterlinkDirectives(enumValue.Directives, enumValue, enumType, _schema._directives);
            }
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
            InterlinkDirectives(inputObjectType.Directives, inputObjectType, null, _schema._directives);
            InterlinkInputValues(inputObjectType.InputFields, inputObjectType, inputObjectType);
        }

        public void VisitSchemaDefinition(SchemaRoot schemaRoot)
        {
        }

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
            InterlinkDirectives(schemaDefinition.Directives, schemaDefinition, null, _schema._directives);

            foreach (var operationTypeDefinition in schemaDefinition.Operations.Values)
            {
                if (!_schema._types.TryGetValue(operationTypeDefinition.NamedType, out var typeDefinition))
                    FatalException(ValidationException.TypeNotDefinedForSchemaOperation(operationTypeDefinition));

                if (typeDefinition is not ObjectTypeDefinition objectTypeDefinition)
                    FatalException(ValidationException.SchemaOperationTypeNotObject(operationTypeDefinition, typeDefinition!));

                operationTypeDefinition.Definition = typeDefinition as ObjectTypeDefinition;
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

                InterlinkDirectives(field.Directives, field, rootNode, _schema._directives);
                InterlinkTypeNode(field.Type, field, rootNode, field, _schema._types);
                InterlinkInputValues(field.Arguments, field, rootNode);
            }
        }

        private void InterlinkInputValues(InputValueDefinitions inputValues, DocumentNode parentNode, DocumentNode rootNode)
        {
            foreach (var inputValue in inputValues.Values)
            {
                inputValue.Parent = parentNode;

                InterlinkDirectives(inputValue.Directives, inputValue, rootNode, _schema._directives);
                InterlinkTypeNode(inputValue.Type, inputValue, rootNode, inputValue, _schema._types);
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
                                                                        ((typeDefinition is ScalarTypeDefinition) || 
                                                                         (typeDefinition is UnionTypeDefinition)) ? "a" : "an"));
                }
            }
        }
    }
}
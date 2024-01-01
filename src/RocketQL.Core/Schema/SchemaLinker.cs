namespace RocketQL.Core.Base;

public partial class Schema
{
    private SchemaLinker? _schemaLink = null;
    private SchemaLinker Linker => _schemaLink ??= new SchemaLinker(this);

    private class SchemaLinker(Schema schema) : ISchemaNodeVisitors
    {
        private readonly Schema _schema = schema;

        public void Visit()
        {
            ISchemaNodeVisitors visitor = this;
            visitor.Visit(_schema.Directives.Values);
            visitor.Visit(_schema.Types.Values);
            visitor.Visit(_schema.Schemas);
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

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
            InterlinkDirectives(schemaDefinition.Directives, schemaDefinition);

            foreach(var operationTypeDefinition in schemaDefinition.Operations.Values)
            {
                if (!_schema.Types.TryGetValue(operationTypeDefinition.NamedType, out var typeDefinition))
                    throw ValidationException.TypeNotDefinedForSchemaOperation(operationTypeDefinition);

                operationTypeDefinition.Definition = typeDefinition;
            }
        }

        private void InterlinkDirectives(Directives directives, SchemaNode parentNode, SchemaNode? grandParentNode = null)
        {
            foreach (var directive in directives)
            {
                directive.Parent = parentNode;

                if (!_schema.Directives.TryGetValue(directive.Name, out DirectiveDefinition? directiveDefinition))
                {
                    if (grandParentNode is not null)
                        throw ValidationException.UndefinedDirective(directive, parentNode.OutputElement, parentNode.OutputName, grandParentNode);
                    else
                        throw ValidationException.UndefinedDirective(directive, parentNode);
                }

                directive.Definition = directiveDefinition;
                directiveDefinition.References.Add(directive);
            }
        }

        private void InterlinkInterfaces(Interfaces interfaces, SchemaNode parentNode)
        {
            foreach (var interfaceEntry in interfaces.Values)
            {
                interfaceEntry.Parent = parentNode;

                if (!_schema.Types.TryGetValue(interfaceEntry.Name, out TypeDefinition? typeDefinition))
                    throw ValidationException.UndefinedInterface(interfaceEntry, parentNode);

                if (typeDefinition is not InterfaceTypeDefinition interfaceTypeDefinition)
                    throw ValidationException.TypeIsNotAnInterface(interfaceEntry, parentNode, typeDefinition);

                interfaceEntry.Definition = interfaceTypeDefinition;
                interfaceTypeDefinition.References.Add(interfaceEntry);
            }
        }

        private void InterlinkFields(FieldDefinitions fields, SchemaNode parentNode, SchemaNode rootNode)
        {
            foreach (var field in fields.Values)
            {
                field.Parent = parentNode;

                InterlinkDirectives(field.Directives, field, rootNode);
                InterlinkTypeNode(field.Type, field, rootNode, field);
                InterlinkInputValues(field.Arguments, field, rootNode);
            }
        }

        private void InterlinkInputValues(InputValueDefinitions inputValues, SchemaNode parentNode, SchemaNode rootNode)
        {
            foreach (var inputValue in inputValues.Values)
            {
                inputValue.Parent = parentNode;

                InterlinkDirectives(inputValue.Directives, inputValue, rootNode);
                InterlinkTypeNode(inputValue.Type, inputValue, rootNode, inputValue);
            }
        }

        private void InterlinkTypeNode(TypeNode typeLocation, SchemaNode parentNode, SchemaNode rootNode, SchemaNode typeParentNode)
        {
            typeLocation.Parent = typeParentNode;

            if (typeLocation is TypeList typeList)
                InterlinkTypeNode(typeList.Type, parentNode, rootNode, typeList);
            else if (typeLocation is TypeName typeName)
            {
                if (!_schema.Types.TryGetValue(typeName.Name, out var type))
                    throw ValidationException.UndefinedTypeForListEntry(typeName.Location, typeName.Name, parentNode.OutputElement, parentNode.OutputName, rootNode);

                typeName.Definition = type;
                type.References.Add(typeName);
            }
        }

        private void InterlinkMemberTypes(MemberTypes memberTypes, UnionTypeDefinition unionType)
        {
            foreach (var memberType in memberTypes.Values)
            {
                memberType.Parent = unionType; 

                if (!_schema.Types.TryGetValue(memberType.Name, out TypeDefinition? typeDefinition))
                    throw ValidationException.UndefinedMemberType(memberType, unionType);

                if (typeDefinition is ObjectTypeDefinition objectTypeDefinition)
                {
                    memberType.Definition = objectTypeDefinition;
                    objectTypeDefinition.References.Add(memberType);
                }
                else
                {
                    throw ValidationException.TypeIsNotAnObject(memberType, unionType, typeDefinition, 
                                                                ((typeDefinition is ScalarTypeDefinition) || (typeDefinition is UnionTypeDefinition)) ? "a" : "an");
                }
            }
        }
    }
}
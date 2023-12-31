using RocketQL.Core.Nodes;

namespace RocketQL.Core.Base;

public partial class Schema
{
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
                InterlinkDirectives(argument.Directives, argument, directive);
                InterlinkTypeNode(argument.Type, argument, directive);
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
            InterlinkFields(objectType.Fields, objectType);
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            InterlinkDirectives(interfaceType.Directives, interfaceType);
            InterlinkInterfaces(interfaceType.ImplementsInterfaces, interfaceType);
            InterlinkFields(interfaceType.Fields, interfaceType);
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
                InterlinkDirectives(enumValue.Directives, enumValue, enumType);
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
            InterlinkDirectives(inputObjectType.Directives, inputObjectType);
            InterlinkInputValues(inputObjectType.InputFields, inputObjectType);
        }

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
        }

        private void InterlinkDirectives(Directives directives, SchemaNode parentNode, SchemaNode? grandParentNode = null)
        {
            foreach (var directive in directives)
            {
                if (!_schema.Directives.TryGetValue(directive.Name, out DirectiveDefinition? directiveDefinition))
                {
                    if (grandParentNode is null)
                        throw ValidationException.UndefinedDirective(directive, parentNode);
                    else
                        throw ValidationException.UndefinedDirective(directive, parentNode.OutputElement, parentNode.OutputName, grandParentNode);
                }

                directive.Definition = directiveDefinition;
                directiveDefinition.References.Add(directive);
            }
        }

        private void InterlinkInterfaces(Interfaces interfaces, SchemaNode parentNode)
        {
            foreach (var interfaceEntry in interfaces.Values)
            {
                if (!_schema.Types.TryGetValue(interfaceEntry.Name, out TypeDefinition? typeDefinition))
                    throw ValidationException.UndefinedInterface(interfaceEntry, parentNode);

                if (typeDefinition is not InterfaceTypeDefinition interfaceTypeDefinition)
                    throw ValidationException.TypeIsNotAnInterface(interfaceEntry, parentNode, typeDefinition);

                interfaceEntry.Definition = interfaceTypeDefinition;
                interfaceTypeDefinition.References.Add(interfaceEntry);
            }
        }

        private void InterlinkFields(FieldDefinitions fields, SchemaNode parentNode)
        {
            foreach (var field in fields.Values)
            {
                InterlinkDirectives(field.Directives, field, parentNode);
                InterlinkTypeNode(field.Type, field, parentNode);
                InterlinkInputValues(field.Arguments, parentNode);
            }
        }

        private void InterlinkInputValues(InputValueDefinitions inputValues, SchemaNode parentNode)
        {
            foreach (var inputValue in inputValues.Values)
            {
                InterlinkDirectives(inputValue.Directives, inputValue, parentNode);
                InterlinkTypeNode(inputValue.Type, inputValue, parentNode);
            }
        }

        private void InterlinkTypeNode(TypeNode typeLocation, SchemaNode listNode, SchemaNode parentNode)
        {
            if (typeLocation is TypeList typeList)
                InterlinkTypeNode(typeList.Type, listNode, parentNode);
            else if (typeLocation is TypeName typeName)
            {
                if (_schema.Types.TryGetValue(typeName.Name, out var type))
                {
                    typeName.Definition = type;
                    type.References.Add(typeName);
                }
                else
                    throw ValidationException.UndefinedTypeForListEntry(typeName.Location, typeName.Name, listNode.OutputElement, listNode.OutputName, parentNode);
            }
        }

        private void InterlinkMemberTypes(MemberTypes memberTypes, UnionTypeDefinition unionType)
        {
            foreach (var memberType in memberTypes.Values)
            {
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
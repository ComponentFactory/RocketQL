using RocketQL.Core.Nodes;
using System.Data;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private class SchemaLink(Schema schema) : ISchemaNodeVisitors
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
            // TODO Fields
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            InterlinkDirectives(interfaceType.Directives, interfaceType);
            InterlinkInterfaces(interfaceType.ImplementsInterfaces, interfaceType);
            // TODO Fields
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
            InterlinkDirectives(unionType.Directives, unionType);
            // TODO MemberTypes
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
            // TODO Input Fields
        }

        private void InterlinkDirectives(Directives directives, SchemaNode parentNode, SchemaNode? grandParentNode = null)
        {
            foreach (var directive in directives.Values)
            {
                if (!_schema.Directives.TryGetValue(directive.Name, out DirectiveDefinition? directiveDefinition))
                {
                    if (grandParentNode is null)
                        throw ValidationException.UndefinedDirective(directive, parentNode);
                    else
                        throw ValidationException.UndefinedDirective(directive, parentNode, grandParentNode);
                }

                directive.Definition = directiveDefinition;
            }
        }

        private void InterlinkInterfaces(Interfaces interfaces, SchemaNode parentNode)
        {
            foreach (var interfaceEntry in interfaces.Values)
            {
                if (!_schema.Types.TryGetValue(interfaceEntry.Name, out TypeDefinition? typeDefinition))
                    throw ValidationException.UndefinedInterface(interfaceEntry, parentNode);

                if (typeDefinition is InterfaceTypeDefinition interfaceTypeDefinition)
                    interfaceEntry.Definition = interfaceTypeDefinition;
                else
                    throw ValidationException.TypeIsNotAnInterface(interfaceEntry, parentNode, typeDefinition);
            }
        }

        private void InterlinkTypeNode(TypeNode typeLocation, SchemaNode listNode, SchemaNode parentNode)
        {
            if (typeLocation is TypeList typeList)
                InterlinkTypeNode(typeList.Type, listNode, parentNode);
            else if (typeLocation is TypeName typeName)
            {
                if (_schema.Types.TryGetValue(typeName.Name, out var type))
                    typeName.Definition = type;
                else
                    throw ValidationException.UndefinedTypeForListEntry(typeName.Location, typeName.Name, listNode, parentNode);
            }
        }

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
        }
    }
}
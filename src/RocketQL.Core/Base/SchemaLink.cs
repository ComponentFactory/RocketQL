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

        public void VisitDirectiveDefinition(DirectiveDefinition directiveDefinition)
        {
            foreach (var argumentDefinition in directiveDefinition.Arguments.Values)
            {
                InterlinkDirectives(argumentDefinition.Directives, argumentDefinition, directiveDefinition);
                InterlinkTypeNode(argumentDefinition.Type, argumentDefinition, directiveDefinition);
            }
        }

        public void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType)
        {
            InterlinkDirectives(scalarType.Directives, scalarType);
        }

        public void VisitObjectTypeDefinition(ObjectTypeDefinition objectType)
        {
            InterlinkDirectives(objectType.Directives, objectType);
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            InterlinkDirectives(interfaceType.Directives, interfaceType);
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
            InterlinkDirectives(unionType.Directives, unionType);
        }

        public void VisitEnumTypeDefinition(EnumTypeDefinition enumType)
        {
            InterlinkDirectives(enumType.Directives, enumType);
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
            InterlinkDirectives(inputObjectType.Directives, inputObjectType);
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
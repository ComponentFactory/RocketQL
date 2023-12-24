using RocketQL.Core.Nodes;
using System.Data;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private class SchemaValidate(Schema schema) : ISchemaNodeVisitors
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
            if (directiveDefinition.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(directiveDefinition);

            foreach (var argumentDefinition in directiveDefinition.Arguments.Values)
            {
                if (argumentDefinition.Name.StartsWith("__"))
                    throw ValidationException.ListEntryDoubleUnderscore(argumentDefinition.Location, directiveDefinition.OutputElement, directiveDefinition.OutputName, argumentDefinition.OutputElement.ToLower(), argumentDefinition.Name);
            }
        }

        public void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType)
        {
            if (scalarType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(scalarType);
        }

        public void VisitObjectTypeDefinition(ObjectTypeDefinition objectType)
        {
            if (objectType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(objectType);
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            if (interfaceType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(interfaceType);
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
            if (unionType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(unionType);
        }

        public void VisitEnumTypeDefinition(EnumTypeDefinition enumType)
        {
            if (enumType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(enumType);
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
            if (inputObjectType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(inputObjectType);
        }

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
        }
    }
}
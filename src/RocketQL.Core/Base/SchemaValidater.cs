namespace RocketQL.Core.Base;

public partial class Schema
{
    private class SchemaValidater(Schema schema) : ISchemaNodeVisitors
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
                    throw ValidationException.ListEntryDoubleUnderscore(argumentDefinition.Location, 
                                                                        directiveDefinition.OutputElement, directiveDefinition.OutputName, 
                                                                        argumentDefinition.OutputElement, argumentDefinition.Name);
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

            VisitFieldDefinintions(objectType.Fields.Values, objectType);
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            if (interfaceType.Name.StartsWith("__"))
                throw ValidationException.NameDoubleUnderscore(interfaceType);

            VisitFieldDefinintions(interfaceType.Fields.Values, interfaceType);
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

            foreach (var fieldDefinition in inputObjectType.InputFields.Values)
            {
                if (fieldDefinition.Name.StartsWith("__"))
                    throw ValidationException.ListEntryDoubleUnderscore(fieldDefinition.Location, 
                                                                        inputObjectType.OutputElement, inputObjectType.OutputName, 
                                                                        "Field", fieldDefinition.Name);
            }
        }

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
        }

        private static void VisitFieldDefinintions(IEnumerable<FieldDefinition> fieldDefinitions, SchemaNode parentNode)
        {
            foreach (var fieldDefinition in fieldDefinitions)
            {
                if (fieldDefinition.Name.StartsWith("__"))
                    throw ValidationException.ListEntryDoubleUnderscore(fieldDefinition.Location, 
                                                                        parentNode.OutputElement, parentNode.OutputName, 
                                                                        fieldDefinition.OutputElement, fieldDefinition.Name);

                foreach (var argumentDefinition in fieldDefinition.Arguments.Values)
                {
                    if (argumentDefinition.Name.StartsWith("__"))
                        throw ValidationException.ListEntryDoubleUnderscore(argumentDefinition.Location, 
                                                                            parentNode.OutputElement, parentNode.OutputName, 
                                                                            fieldDefinition.OutputElement, fieldDefinition.OutputName, 
                                                                            argumentDefinition.OutputElement, argumentDefinition.Name);
                }
            }
        }
    }
}
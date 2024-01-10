namespace RocketQL.Core.Base;

public partial class Schema
{
    private SchemaRooted? _schemaRooted = null;
    private SchemaRooted Rooted => _schemaRooted ??= new SchemaRooted(this);

    private class SchemaRooted(Schema schema) : ISchemaNodeVisitors
    {
        private readonly Schema _schema = schema;

        public void Visit()
        {
            ISchemaNodeVisitors visitor = this;
            visitor.Visit(_schema._schemas);
        }

        public void VisitSchemaDefinition(SchemaRoot schemaRoot)
        {
        }

        public void VisitSchemaDefinition(SchemaDefinition schema)
        {
            VisitDirectives(schema.Directives);

            foreach (var operationType in schema.Operations.Values)
                if (operationType.Definition is not null)
                    ((ISchemaNodeVisitors)this).Visit(operationType.Definition);
        }

        public void VisitDirectiveDefinition(DirectiveDefinition directive)
        {
            if (!directive.IsRooted)
            {
                directive.IsRooted = true;
                VisitInputValueDefinitions(directive.Arguments);
            }
        }

        public void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType)
        {
            if (!scalarType.IsRooted)
            {
                scalarType.IsRooted = true;
                VisitDirectives(scalarType.Directives);
            }
        }

        public void VisitObjectTypeDefinition(ObjectTypeDefinition objectType)
        {
            if (!objectType.IsRooted)
            {
                objectType.IsRooted = true;
                VisitDirectives(objectType.Directives);
                VisitInterfaces(objectType.ImplementsInterfaces);
                VisitFieldsDefinitions(objectType.Fields);
            }
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            if (!interfaceType.IsRooted)
            {
                interfaceType.IsRooted = true;
                VisitDirectives(interfaceType.Directives);
                VisitInterfaces(interfaceType.ImplementsInterfaces);
                VisitFieldsDefinitions(interfaceType.Fields);
            }
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
            if (!unionType.IsRooted)
            {
                unionType.IsRooted = true;
                VisitDirectives(unionType.Directives);
                VisitMemberTypes(unionType.MemberTypes);
            }
        }

        public void VisitEnumTypeDefinition(EnumTypeDefinition enumType)
        {
            if (!enumType.IsRooted)
            {
                enumType.IsRooted = true;
                VisitDirectives(enumType.Directives);
                VisitEnumValueDefinitions(enumType.EnumValues);
            }
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
            if (!inputObjectType.IsRooted)
            {
                inputObjectType.IsRooted = true;
                VisitDirectives(inputObjectType.Directives);
                VisitInputValueDefinitions(inputObjectType.InputFields);
            }
        }

        private void VisitDirectives(Directives directives)
        {
            foreach (var directive in directives)
                if (directive.Definition is not null)
                    VisitDirectiveDefinition(directive.Definition);
        }

        private void VisitInputValueDefinitions(InputValueDefinitions inputValues)
        {
            foreach(var inputValue in inputValues.Values) 
            {
                VisitDirectives(inputValue.Directives);

                if (inputValue.Type.Definition is not null)
                    ((ISchemaNodeVisitors)this).Visit(inputValue.Type.Definition);
            }
        }

        private void VisitInterfaces(Interfaces interfaces)
        {
            foreach (var interfaceValue in interfaces.Values)
                if (interfaceValue.Definition is not null)
                    ((ISchemaNodeVisitors)this).Visit(interfaceValue.Definition);
        }

        private void VisitFieldsDefinitions(FieldDefinitions fieldDefinitions)
        {
            foreach(var fieldDefinition in fieldDefinitions.Values)
            {
                VisitDirectives(fieldDefinition.Directives);
                VisitInputValueDefinitions(fieldDefinition.Arguments);

                if (fieldDefinition.Type.Definition is not null)
                    ((ISchemaNodeVisitors)this).Visit(fieldDefinition.Type.Definition);
            }
        }

        private void VisitMemberTypes(MemberTypes memberTypes)
        {
            foreach (var memberType in memberTypes.Values)
                if (memberType.Definition is not null)
                    ((ISchemaNodeVisitors)this).Visit(memberType.Definition);
        }

        private void VisitEnumValueDefinitions(EnumValueDefinitions enumValueDefinitions)
        {
            foreach (var enumValueDefinition in enumValueDefinitions.Values)
                VisitDirectives(enumValueDefinition.Directives);
        }
    }
}
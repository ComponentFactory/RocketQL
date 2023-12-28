using System.Linq;

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
            var interfaceDefinitions = CheckObjectImplementsInterfaces(objectType);
            CheckObjectHasInterfaceFields(objectType, interfaceDefinitions);
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

                if (fieldDefinition.Type.Definition is null)
                    throw ValidationException.UnrecognizedType(fieldDefinition.Location, fieldDefinition.Name);

                if (!fieldDefinition.Type.Definition.IsOutputType)
                    throw ValidationException.TypeIsNotAnOutputType(fieldDefinition, parentNode, fieldDefinition.Type.Definition.OutputName);

                foreach (var argumentDefinition in fieldDefinition.Arguments.Values)
                {
                    if (argumentDefinition.Name.StartsWith("__"))
                        throw ValidationException.ListEntryDoubleUnderscore(argumentDefinition.Location, 
                                                                            parentNode.OutputElement, parentNode.OutputName, 
                                                                            fieldDefinition.OutputElement, fieldDefinition.OutputName, 
                                                                            argumentDefinition.OutputElement, argumentDefinition.Name);

                    if (argumentDefinition.Type.Definition is null)
                        throw ValidationException.UnrecognizedType(argumentDefinition.Location, argumentDefinition.Name);

                    if (!argumentDefinition.Type.Definition.IsOutputType)
                        throw ValidationException.TypeIsNotAnInputType(fieldDefinition, parentNode, argumentDefinition, argumentDefinition.Type.Definition.OutputName);

                    if (argumentDefinition.Type.NonNull && (argumentDefinition.DefaultValue is null) && argumentDefinition.Directives.ContainsKey("deprecated"))
                        throw ValidationException.NonNullArgumentCannotBeDeprecated(fieldDefinition, parentNode, argumentDefinition);
                }
            }
        }

        private InterfaceTypeDefinitions CheckObjectImplementsInterfaces(ObjectTypeDefinition objectType)
        {
            InterfaceTypeDefinitions interfaceDefinitions = [];
            foreach (var interfaceEntry in objectType.ImplementsInterfaces.Values)
            {
                if (!_schema.Types.TryGetValue(interfaceEntry.Name, out TypeDefinition? typeDefinition))
                    throw ValidationException.UndefinedInterface(interfaceEntry, objectType);

                if (typeDefinition is not InterfaceTypeDefinition interfaceTypeDefinition)
                    throw ValidationException.TypeIsNotAnInterface(interfaceEntry, objectType, typeDefinition);

                interfaceDefinitions.Add(interfaceTypeDefinition.Name, interfaceTypeDefinition);
            }

            HashSet<string> processed = [];
            foreach (var objectImplement in interfaceDefinitions)
                CheckInterfaceImplemented(interfaceDefinitions, processed, objectImplement.Value, objectType, objectType);

            return interfaceDefinitions;
        }

        private void CheckInterfaceImplemented(InterfaceTypeDefinitions objectImplements,
                                               HashSet<string> processed,
                                               InterfaceTypeDefinition checkInterface,
                                               SchemaNode parentNode,
                                               ObjectTypeDefinition objectType)
        {
            if (!processed.Contains(checkInterface.Name))
            {
                processed.Add(checkInterface.Name);

                if (!_schema.Types.TryGetValue(checkInterface.Name, out TypeDefinition? typeDefinition))
                    throw ValidationException.UndefinedInterface(checkInterface, parentNode);

                if (typeDefinition is not InterfaceTypeDefinition interfaceTypeDefinition)
                    throw ValidationException.TypeIsNotAnInterface(checkInterface, parentNode, typeDefinition);

                foreach (var implementsInterface in interfaceTypeDefinition.ImplementsInterfaces.Values)
                {
                    if (!objectImplements.ContainsKey(implementsInterface.Name))
                        throw ValidationException.ObjectMissingImplements(objectType, implementsInterface.Name, checkInterface.Name);

                    CheckInterfaceImplemented(objectImplements, processed, interfaceTypeDefinition, checkInterface, objectType);
                }
            }
        }

        private void CheckObjectHasInterfaceFields(ObjectTypeDefinition objectType, InterfaceTypeDefinitions interfaceDefinitions)
        {
            foreach(var interfaceDefinition in interfaceDefinitions.Values)
            {
                foreach(var interfaceField in interfaceDefinition.Fields)
                {
                    if (!objectType.Fields.TryGetValue(interfaceField.Key, out FieldDefinition? interfaceFieldDefinition))
                        throw ValidationException.ObjectMissingFieldFromInterface(objectType, interfaceField.Key, interfaceDefinition.Name);
                }
            }
        }
    }
}
﻿namespace RocketQL.Core.Base;

public partial class SchemaBuilder
{
    private SchemaBuilderLinker? _linker = null;
    private SchemaBuilderLinker Linker => _linker ??= new SchemaBuilderLinker(this);

    private class SchemaBuilderLinker(SchemaBuilder schema) : NodePathTracker, IVisitDocumentNode
    {
        private readonly SchemaBuilder _schema = schema;

        public void Visit()
        {
            IVisitDocumentNode visitor = this;
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
            PushPath(directive);

            foreach (var argument in directive.Arguments.Values)
            {
                PushPath(argument);
                argument.Parent = directive;
                InterlinkDirectives(argument.Directives, argument);
                InterlinkTypeNode(argument.Type, argument);
                PopPath();
            }

            PopPath();
        }

        public void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType)
        {
            PushPath(scalarType);
            InterlinkDirectives(scalarType.Directives, scalarType);
            PopPath();
        }

        public void VisitObjectTypeDefinition(ObjectTypeDefinition objectType)
        {
            PushPath(objectType);
            InterlinkDirectives(objectType.Directives, objectType);
            InterlinkInterfaces(objectType.ImplementsInterfaces, objectType);
            InterlinkFields(objectType.Fields, objectType);
            PopPath();
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            PushPath(interfaceType);
            InterlinkDirectives(interfaceType.Directives, interfaceType);
            InterlinkInterfaces(interfaceType.ImplementsInterfaces, interfaceType);
            InterlinkFields(interfaceType.Fields, interfaceType);
            PopPath();
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
            PushPath(unionType);
            InterlinkDirectives(unionType.Directives, unionType);
            InterlinkMemberTypes(unionType.MemberTypes, unionType);
            PopPath();
        }

        public void VisitEnumTypeDefinition(EnumTypeDefinition enumType)
        {
            PushPath(enumType);
            InterlinkDirectives(enumType.Directives, enumType);

            foreach (var enumValue in enumType.EnumValues.Values)
            {
                PushPath(enumValue);
                enumValue.Parent = enumType;
                InterlinkDirectives(enumValue.Directives, enumValue);
                PopPath();
            }

            PopPath();
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
            PushPath(inputObjectType);
            InterlinkDirectives(inputObjectType.Directives, inputObjectType);
            InterlinkInputValues(inputObjectType.InputFields, inputObjectType);
            PopPath();
        }

        public void VisitSchemaDefinition(SchemaRoot schemaRoot)
        {
        }

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
            PushPath("schema");
            InterlinkDirectives(schemaDefinition.Directives, schemaDefinition);

            foreach (var operationTypeDefinition in schemaDefinition.Operations.Values)
            {
                PushPath(operationTypeDefinition);

                if (!_schema._types.TryGetValue(operationTypeDefinition.NamedType, out var typeDefinition))
                    _schema.NonFatalException(ValidationException.SchemaOperationTypeNotDefined(operationTypeDefinition, CurrentPath));
                else if (typeDefinition is not ObjectTypeDefinition objectTypeDefinition)
                    _schema.NonFatalException(ValidationException.SchemaOperationTypeNotObject(operationTypeDefinition, typeDefinition!, CurrentPath));
                else
                    operationTypeDefinition.Definition = objectTypeDefinition;

                PopPath();
            }

            PopPath();
        }

        private void InterlinkInterfaces(Interfaces interfaces, TypeDefinition parentNode)
        {
            foreach (var interfaceEntry in interfaces.Values)
            {
                PushPath(interfaceEntry);
                interfaceEntry.Parent = parentNode;

                if (!_schema._types.TryGetValue(interfaceEntry.Name, out var typeDefinition))
                    _schema.NonFatalException(ValidationException.UndefinedInterface(interfaceEntry, parentNode, CurrentPath));
                else if (typeDefinition is not InterfaceTypeDefinition interfaceTypeDefinition)
                    _schema.NonFatalException(ValidationException.TypeIsNotAnInterface(interfaceEntry, parentNode, typeDefinition!, CurrentPath));
                else
                {
                    interfaceEntry.Definition = interfaceTypeDefinition;
                    interfaceTypeDefinition.References.Add(interfaceEntry!);
                }

                PopPath();
            }
        }

        private void InterlinkFields(FieldDefinitions fields, DocumentNode parentNode)
        {
            foreach (var field in fields.Values)
            {
                PushPath(field);
                field.Parent = parentNode;
                InterlinkDirectives(field.Directives, field);
                InterlinkTypeNode(field.Type, field);
                InterlinkInputValues(field.Arguments, field);
                PopPath();
            }
        }

        private void InterlinkInputValues(InputValueDefinitions inputValues, DocumentNode parentNode)
        {
            foreach (var inputValue in inputValues.Values)
            {
                PushPath(inputValue);
                inputValue.Parent = parentNode;
                InterlinkDirectives(inputValue.Directives, inputValue);
                InterlinkTypeNode(inputValue.Type, inputValue);
                PopPath();
            }
        }

        private void InterlinkMemberTypes(MemberTypes memberTypes, UnionTypeDefinition unionType)
        {
            foreach (var memberType in memberTypes.Values)
            {
                PushPath(memberType);
                memberType.Parent = unionType;

                if (!_schema._types.TryGetValue(memberType.Name, out var typeDefinition))
                    _schema.NonFatalException(ValidationException.UndefinedMemberType(memberType, CurrentPath));
                else if (typeDefinition is not ObjectTypeDefinition objectTypeDefinition)
                {
                    _schema.NonFatalException(ValidationException.TypeIsNotAnObject(memberType,
                                                                                    typeDefinition!,
                                                                                    ((typeDefinition is ScalarTypeDefinition) || (typeDefinition is UnionTypeDefinition)) ? "a" : "an",
                                                                                    CurrentPath));
                }
                else
                {
                    memberType.Definition = objectTypeDefinition;
                    objectTypeDefinition.References.Add(memberType);
                }

                PopPath();
            }
        }

        private void InterlinkDirectives(Directives directives, DocumentNode parentNode)
        {
            foreach (var directive in directives)
            {
                PushPath(directive);
                directive.Parent = parentNode;

                if (!_schema._directives.TryGetValue(directive.Name, out var directiveDefinition))
                    _schema.NonFatalException(ValidationException.UndefinedDirective(directive, parentNode, CurrentPath));
                else
                {
                    directive.Definition = directiveDefinition;
                    directiveDefinition.References.Add(directive!);
                }

                PopPath();
            }
        }

        private void InterlinkTypeNode(TypeNode typeLocation, DocumentNode typeParentNode)
        {
            typeLocation.Parent = typeParentNode;

            if (typeLocation is TypeList typeList)
                InterlinkTypeNode(typeList.Type, typeList);
            if (typeLocation is TypeNonNull typeNonNull)
                InterlinkTypeNode(typeNonNull.Type, typeNonNull);
            else if (typeLocation is TypeName typeName)
            {
                if (!_schema._types.TryGetValue(typeName.Name, out var type))
                    _schema.NonFatalException(ValidationException.UndefinedTypeForListEntry(typeName, typeName.Name, typeParentNode, CurrentPath));
                else
                {
                    typeName.Definition = type;
                    type.References.Add(typeName);
                }
            }
        }
    }
}

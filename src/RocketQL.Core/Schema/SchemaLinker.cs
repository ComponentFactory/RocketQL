using System.Data;
using RocketQL.Core.Enumerations;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private SchemaLinker? _schemaLinker = null;
    private SchemaLinker Linker => _schemaLinker ??= new SchemaLinker(this);

    private class SchemaLinker(Schema schema) : NodeVisitor, IDocumentNodeVisitors
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
            PushPath($"directive {directive.Name}");

            foreach (var argument in directive.Arguments.Values)
            {
                PushPath($"argument {argument.Name}");
                argument.Parent = directive;
                InterlinkDirectives(argument.Directives, argument);
                InterlinkTypeNode(argument.Type, argument);
                PopPath();
            }

            PopPath();
        }

        public void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType)
        {
            PushPath($"scalar {scalarType.Name}");
            InterlinkDirectives(scalarType.Directives, scalarType);
            PopPath();
        }

        public void VisitObjectTypeDefinition(ObjectTypeDefinition objectType)
        {
            PushPath($"type {objectType.Name}");
            InterlinkDirectives(objectType.Directives, objectType);
            InterlinkInterfaces(objectType.ImplementsInterfaces, objectType);
            InterlinkFields(objectType.Fields, objectType);
            PopPath();
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            PushPath($"interface {interfaceType.Name}");
            InterlinkDirectives(interfaceType.Directives, interfaceType);
            InterlinkInterfaces(interfaceType.ImplementsInterfaces, interfaceType);
            InterlinkFields(interfaceType.Fields, interfaceType);
            PopPath();
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
            PushPath($"union {unionType.Name}");
            InterlinkDirectives(unionType.Directives, unionType);
            InterlinkMemberTypes(unionType.MemberTypes, unionType);
            PopPath();
        }

        public void VisitEnumTypeDefinition(EnumTypeDefinition enumType)
        {
            PushPath($"enum {enumType.Name}");
            InterlinkDirectives(enumType.Directives, enumType);

            foreach (var enumValue in enumType.EnumValues.Values)
            {
                PushPath($"enum value {enumValue.Name}");
                enumValue.Parent = enumType;
                InterlinkDirectives(enumValue.Directives, enumValue);
                PopPath();
            }

            PopPath();
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
            PushPath($"input object {inputObjectType.Name}");
            InterlinkDirectives(inputObjectType.Directives, inputObjectType);
            InterlinkInputValues(inputObjectType.InputFields, inputObjectType, "input field");
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
                PushPath($"{operationTypeDefinition.Operation.ToString().ToLower()} {operationTypeDefinition.NamedType}");

                if (!_schema._types.TryGetValue(operationTypeDefinition.NamedType, out var typeDefinition))
                    FatalException(ValidationException.SchemaOperationTypeNotDefined(operationTypeDefinition, CurrentPath));

                if (typeDefinition is not ObjectTypeDefinition objectTypeDefinition)
                    FatalException(ValidationException.SchemaOperationTypeNotObject(operationTypeDefinition, typeDefinition!, CurrentPath));
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
                PushPath($"implements {interfaceEntry.Name}");
                interfaceEntry.Parent = parentNode;

                if (!_schema._types.TryGetValue(interfaceEntry.Name, out var typeDefinition))
                    FatalException(ValidationException.UndefinedInterface(interfaceEntry, parentNode, CurrentPath));

                if (typeDefinition is not InterfaceTypeDefinition interfaceTypeDefinition)
                    FatalException(ValidationException.TypeIsNotAnInterface(interfaceEntry, parentNode, typeDefinition!, CurrentPath));
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
                PushPath($"field {field.Name}");
                field.Parent = parentNode;
                InterlinkDirectives(field.Directives, field);
                InterlinkTypeNode(field.Type, field);
                InterlinkInputValues(field.Arguments, field, "argument");
                PopPath();
            }
        }

        private void InterlinkInputValues(InputValueDefinitions inputValues, DocumentNode parentNode, string elementUsage)
        {
            foreach (var inputValue in inputValues.Values)
            {
                PushPath($"{elementUsage.ToLower()} {inputValue.Name}");
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
                PushPath($"member type {memberType.Name}");
                memberType.Parent = unionType;

                if (!_schema._types.TryGetValue(memberType.Name, out var typeDefinition))
                    FatalException(ValidationException.UndefinedMemberType(memberType, CurrentPath));

                if (typeDefinition is ObjectTypeDefinition objectTypeDefinition)
                {
                    memberType.Definition = objectTypeDefinition;
                    objectTypeDefinition.References.Add(memberType);
                }
                else
                    FatalException(ValidationException.TypeIsNotAnObject(memberType,
                                                                         typeDefinition!,
                                                                         ((typeDefinition is ScalarTypeDefinition) || (typeDefinition is UnionTypeDefinition)) ? "a" : "an",
                                                                         CurrentPath));

                PopPath();
            }
        }

        private void InterlinkDirectives(Directives directives, DocumentNode parentNode)
        {
            foreach (var directive in directives)
            {
                PushPath($"directive {directive.Name}");
                directive.Parent = parentNode;

                if (!_schema._directives.TryGetValue(directive.Name, out var directiveDefinition))
                    FatalException(ValidationException.UndefinedDirective(directive, parentNode, CurrentPath));
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
                    FatalException(ValidationException.UndefinedTypeForListEntry(typeName, typeName.Name, typeParentNode, CurrentPath));
                else
                {
                    typeName.Definition = type;
                    type.References.Add(typeName);
                }
            }
        }
    }
}

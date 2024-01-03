using RocketQL.Core.Nodes;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private static SchemaPrintOptions _defaultPrintOptions = new();

    public string Print()
    {
        return Print(_defaultPrintOptions);
    }

    public string Print(SchemaPrintOptions options)
    {
        var printer = new SchemaPrinter(this);
        printer.Visit(options);
        return printer.ToString();
    }

    private class SchemaPrinter(Schema schema) : ISchemaNodeVisitors
    {
        private readonly Schema _schema = schema;
        private readonly StringBuilder _builder = new();
        private SchemaPrintOptions _options = _defaultPrintOptions;
        private char _indentCharacter = ' ';
        private int _indents;

        public void Visit(SchemaPrintOptions options)
        {
            _options = options;
            _indentCharacter = _options.IndentCharacter == PrintIndentCharacter.Space ? ' ' : '\t';

            ISchemaNodeVisitors visitor = this;
            visitor.Visit(_schema.Schemas);
            visitor.Visit(_schema.Types.Values.Where(t => t is ObjectTypeDefinition));
            visitor.Visit(_schema.Types.Values.Where(t => t is InterfaceTypeDefinition));
            visitor.Visit(_schema.Types.Values.Where(t => t is UnionTypeDefinition));
            visitor.Visit(_schema.Types.Values.Where(t => t is InputObjectTypeDefinition));
            visitor.Visit(_schema.Types.Values.Where(t => t is EnumTypeDefinition));
            visitor.Visit(_schema.Types.Values.Where(t => t is ScalarTypeDefinition));
            visitor.Visit(_schema.Directives.Values);
        }

        public void VisitDirectiveDefinition(DirectiveDefinition directiveDefinition)
        {
            if (directiveDefinition.IsPredefined && !_options.PrintPredefined)
                return;

            PrintDescription(directiveDefinition.Description);
            StartLine();
            Print($"directive @{directiveDefinition.Name}");

            if (directiveDefinition.Arguments.Count > 0)
            {
                EndLine();
                PrintLine("(");
                StartIndent();

                bool firstArgument = true;
                bool argumentDescription = false;
                foreach (var argument in directiveDefinition.Arguments.Values)
                {
                    if (!firstArgument && (argumentDescription || !IsDescriptionEmpty(argument.Description)))
                        PrintBlankLine();

                    PrintDescription(argument.Description);
                    StartLine();
                    Print($"{argument.Name}: ");
                    PrintType(argument.Type);

                    if (argument.DefaultValue is not null)
                    {
                        Print(" = ");
                        PrintValue(argument.DefaultValue);
                    }

                    PrintDirectives(argument.Directives);
                    EndLine();

                    firstArgument = false;
                    argumentDescription = !IsDescriptionEmpty(argument.Description);
                }

                EndIndent();
                StartLine();
                Print(")");
            }

            if (directiveDefinition.Repeatable)
                Print(" repeatable");

            Print(" on ");

            bool firstLocation = true;
            DirectiveLocations[] locations = Enum.GetValues<DirectiveLocations>();
            foreach(var location in locations)
            {
                var locationName = Enum.GetName(location)!;
                if (!locationName.Contains("_LOCATIONS"))
                {
                    if ((directiveDefinition.DirectiveLocations & location) == location)
                    {
                        if (!firstLocation)
                            Print(" | ");

                        Print(locationName);
                        firstLocation = false;
                    }
                }
            }

            EndLine();
            PrintBlankLine();
        }

        public void VisitScalarTypeDefinition(ScalarTypeDefinition scalarType)
        {
            if (scalarType.IsPredefined && !_options.PrintPredefined)
                return;

            PrintDescription(scalarType.Description);
            StartLine();
            Print($"scalar {scalarType.Name}");
            PrintDirectives(scalarType.Directives);
            EndLine();
            PrintBlankLine();
        }

        public void VisitObjectTypeDefinition(ObjectTypeDefinition objectType)
        {
            if (objectType.IsPredefined && !_options.PrintPredefined)
                return;

            PrintDescription(objectType.Description);
            StartLine();
            Print($"type {objectType.Name}");

            if (objectType.ImplementsInterfaces.Count > 0)
            {
                Print($" implements");

                bool first = true;
                foreach (var implementsInterface in objectType.ImplementsInterfaces.Values)
                {
                    if (!first)
                        Print(" &");

                    Print($" {implementsInterface.Name}");
                    first = false;
                }
            }

            PrintDirectives(objectType.Directives);
            EndLine();
            PrintLine("{");
            StartIndent();

            bool firstField = true;
            bool fieldDescription = false;
            foreach (var field in objectType.Fields.Values)
            {
                if (!firstField && (fieldDescription || !IsDescriptionEmpty(field.Description)))
                    PrintBlankLine();

                PrintDescription(field.Description);
                StartLine();
                Print($"{field.Name}");

                if (field.Arguments.Count > 0)
                {
                    EndLine();
                    PrintLine($"(");
                    StartIndent();

                    bool firstArgument = true;
                    bool argumentDescription = false;
                    foreach (var argument in field.Arguments.Values)
                    {
                        if (!firstArgument && (argumentDescription || !IsDescriptionEmpty(argument.Description)))
                            PrintBlankLine();

                        PrintDescription(argument.Description);
                        StartLine();
                        Print($"{argument.Name}: ");
                        PrintType(argument.Type);

                        if (argument.DefaultValue is not null)
                        {
                            Print(" = ");
                            PrintValue(argument.DefaultValue);
                        }
                        PrintDirectives(argument.Directives);
                        EndLine();

                        firstArgument = false;
                        argumentDescription = !IsDescriptionEmpty(argument.Description);
                    }

                    EndIndent();
                    StartLine();
                    Print($")");
                }

                Print($": ");
                PrintType(field.Type);
                PrintDirectives(field.Directives);
                EndLine();

                firstField = false;
                fieldDescription = !IsDescriptionEmpty(field.Description);
            }

            EndIndent();
            PrintLine("}");
            PrintBlankLine();
        }

        public void VisitInterfaceTypeDefinition(InterfaceTypeDefinition interfaceType)
        {
            if (interfaceType.IsPredefined && !_options.PrintPredefined)
                return;

            PrintDescription(interfaceType.Description);
            StartLine();
            Print($"interface {interfaceType.Name}");

            if (interfaceType.ImplementsInterfaces.Count > 0)
            {
                Print($" implements");

                bool first = true;
                foreach (var implementsInterface in interfaceType.ImplementsInterfaces.Values)
                {
                    if (!first)
                        Print(" &");

                    Print($" {implementsInterface.Name}");
                    first = false;
                }
            }

            PrintDirectives(interfaceType.Directives);
            EndLine();
            PrintLine("{");
            StartIndent();

            bool firstField = true;
            bool fieldDescription = false;
            foreach (var field in interfaceType.Fields.Values)
            {
                if (!firstField && (fieldDescription || !IsDescriptionEmpty(field.Description)))
                    PrintBlankLine();

                PrintDescription(field.Description);
                StartLine();
                Print($"{field.Name}");

                if (field.Arguments.Count > 0)
                {
                    EndLine();
                    PrintLine($"(");
                    StartIndent();

                    bool firstArgument = true;
                    bool argumentDescription = false;
                    foreach (var argument in field.Arguments.Values)
                    {
                        if (!firstArgument && (argumentDescription || !IsDescriptionEmpty(argument.Description)))
                            PrintBlankLine();

                        PrintDescription(argument.Description);
                        StartLine();
                        Print($"{argument.Name}: ");
                        PrintType(argument.Type);

                        if (argument.DefaultValue is not null)
                        {
                            Print(" = ");
                            PrintValue(argument.DefaultValue);
                        }

                        PrintDirectives(argument.Directives);
                        EndLine();

                        firstArgument = false;
                        argumentDescription = !IsDescriptionEmpty(argument.Description);
                    }

                    EndIndent();
                    StartLine();
                    Print($")");
                }

                Print($": ");
                PrintType(field.Type);
                PrintDirectives(field.Directives);
                EndLine();

                firstField = false;
                fieldDescription = !IsDescriptionEmpty(field.Description);
            }

            EndIndent();
            PrintLine("}");
            PrintBlankLine();
        }

        public void VisitUnionTypeDefinition(UnionTypeDefinition unionType)
        {
            if (unionType.IsPredefined && !_options.PrintPredefined)
                return;

            PrintDescription(unionType.Description);
            StartLine();
            Print($"union {unionType.Name}");
            PrintDirectives(unionType.Directives);

            if (unionType.MemberTypes.Count > 0)
            {
                Print($" =");

                bool first = true;
                foreach (var memberType in unionType.MemberTypes.Values)
                {
                    if (!first)
                        Print(" |");

                    Print($" {memberType.Name}");
                    first = false;
                }
            }

            EndLine();
            PrintBlankLine();
        }

        public void VisitEnumTypeDefinition(EnumTypeDefinition enumType)
        {
            if (enumType.IsPredefined && !_options.PrintPredefined)
                return;

            PrintDescription(enumType.Description);
            StartLine();
            Print($"enum {enumType.Name}");
            PrintDirectives(enumType.Directives);
            EndLine();
            PrintLine("{");
            StartIndent();

            bool firstValue = true;
            bool valueDescription = false;
            foreach (var enumValue in enumType.EnumValues.Values)
            {
                if (!firstValue && (valueDescription || !IsDescriptionEmpty(enumValue.Description)))
                    PrintBlankLine();

                PrintDescription(enumValue.Description);
                StartLine();
                Print($"{enumValue.Name}");
                PrintDirectives(enumValue.Directives);
                EndLine();

                firstValue = false;
                valueDescription = !IsDescriptionEmpty(enumValue.Description);
            }

            EndIndent();
            PrintLine("}");
            PrintBlankLine();
        }

        public void VisitInputObjectTypeDefinition(InputObjectTypeDefinition inputObjectType)
        {
            if (inputObjectType.IsPredefined && !_options.PrintPredefined)
                return;

            PrintDescription(inputObjectType.Description);
            StartLine();
            Print($"input {inputObjectType.Name}");
            PrintDirectives(inputObjectType.Directives);
            EndLine();
            PrintLine("{");
            StartIndent();

            bool firstInputField = true;
            bool inputFieldDescription = false;
            foreach (var inputField in inputObjectType.InputFields.Values)
            {
                if (!firstInputField && (inputFieldDescription || !IsDescriptionEmpty(inputField.Description)))
                    PrintBlankLine();

                PrintDescription(inputField.Description);
                StartLine();
                Print($"{inputField.Name}: ");
                PrintType(inputField.Type);

                if (inputField.DefaultValue is not null)
                {
                    Print(" = ");
                    PrintValue(inputField.DefaultValue);
                }

                PrintDirectives(inputField.Directives);
                EndLine();

                firstInputField = false;
                inputFieldDescription = !IsDescriptionEmpty(inputField.Description);
            }

            EndIndent();
            PrintLine("}");
            PrintBlankLine();
        }

        public void VisitSchemaDefinition(SchemaDefinition schemaDefinition)
        {
            PrintDescription(schemaDefinition.Description);
            StartLine();
            Print($"schema");
            PrintDirectives(schemaDefinition.Directives);
            EndLine();

            PrintLine("{");
            StartIndent();

            foreach (var operation in schemaDefinition.Operations.Values)
                PrintLine($"{operation.Operation.ToString().ToLower()}: {operation.NamedType}");

            EndIndent();
            PrintLine("}");
            PrintBlankLine();
        }

        public override string ToString() 
        {
            return _builder.ToString();
        }

        private bool IsDescriptionEmpty(string description)
        {
            return !_options.PrintDescriptions || string.IsNullOrEmpty(description);
        }

        private static bool IsDescriptionMultiline(string description)
        {
            return description.Contains('\n');
        }

        private void PrintDescription(string description)
        {
            if (!IsDescriptionEmpty(description))
            {
                if (IsDescriptionMultiline(description))
                {
                    PrintLine($"\"\"\"");
                    foreach (var line in description.Split('\n'))
                        PrintLine(line);
                    PrintLine($"\"\"\"");
                }
                else
                    PrintLine($"\"{description}\"");
            }
        }

        private void PrintType(TypeNode node)
        {
            switch (node)
            {
                case TypeName typeName:
                    Print(typeName.Name);
                    break;
                case TypeNonNull typeNonNull:
                    PrintType(typeNonNull.Type);
                    Print("!");
                    break;
                case TypeList typeList:
                    Print("[");
                    PrintType(typeList.Type);
                    Print("]");
                    break;
            }
        }

        private void PrintValue(ValueNode node)
        {
            switch (node)
            {
                case NullValueNode:
                    Print("null");
                    break;
                case BooleanValueNode boolNode:
                    Print(boolNode.Value.ToString().ToLower());
                    break;
                case IntValueNode intNode:
                    Print(intNode.Value.ToString());
                    break;
                case FloatValueNode floatNode:
                    Print(floatNode.Value.ToString());
                    break;
                case StringValueNode stringNode:
                    Print($"\"{stringNode.Value}\"");
                    break;
                case EnumValueNode enumNode:
                    Print($"{enumNode.Value}");
                    break;
                case VariableValueNode variableNode:
                    Print($"${variableNode.Value}");
                    break;
                case ListValueNode listNode:
                    {
                        Print("[");

                        var firstEntry = true;
                        foreach (var listEntry in listNode.Values)
                        {
                            if (!firstEntry)
                                Print(", ");

                            PrintValue(listEntry);
                            firstEntry = false;
                        }
                        Print("]");
                    }
                    break;
                case ObjectValueNode objectNode:
                    {
                        Print("{ ");

                        var firstEntry = true;
                        foreach (var objectEntry in objectNode.ObjectFields)
                        {
                            if (!firstEntry)
                                Print(", ");

                            Print($"{objectEntry.Name}: ");
                            PrintValue(objectEntry.Value);
                            firstEntry = false;
                        }
                        Print(" }");
                    }
                    break;
            }
        }

        private void PrintDirectives(Directives directives)
        {
            foreach (var directive in directives)
            {
                Print($" @{directive.Name}");

                if (directive.Arguments.Count > 0)
                {
                    Print("(");
                    bool firstArgument = true;
                    foreach (var argument in directive.Arguments.Values)
                    {
                        if (!firstArgument)
                            Print(" ");

                        Print($"{argument.Name}: ");
                        PrintValue(argument.Value);
                        firstArgument = false;
                    }
                    Print(")");
                }
            }
        }

        private void StartIndent() => _indents++;
        private void EndIndent() => _indents--;
        private void StartLine() => _builder.Append(new string(_indentCharacter, _indents * _options.IndentCount));
        private void EndLine() => _builder.AppendLine("");
        private void Print(string text) => _builder.Append(text);
        private void PrintBlankLine() => PrintLine("");

        private void PrintLine(string text)
        {
            StartLine();
            _builder.Append(text);
            EndLine();
        }
    }
}
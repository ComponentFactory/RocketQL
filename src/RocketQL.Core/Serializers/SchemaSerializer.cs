namespace RocketQL.Core.Serializers;

public ref struct SchemaSerializer(Schema schema)
{
    private static SchemaSerializeOptions _defaultOptions = new();

    public readonly string Serialize(SchemaSerializeOptions? options = null)
    {
        var printer = new SchemaSerialize(schema);
        printer.Visit(options ?? _defaultOptions);
        return printer.ToString();
    }

    private class SchemaSerialize(Schema schema) : IDocumentNodeVisitors
    {
        private static readonly ThreadLocal<StringBuilder> _cachedBuilder = new(() => new(4096));

        private readonly Schema _schema = schema;
        private readonly StringBuilder _builder = _cachedBuilder.Value!;
        private SchemaSerializeOptions _options = _defaultOptions;
        private char _indentCharacter = ' ';
        private int _indents;

        public void Visit(SchemaSerializeOptions options)
        {
            if (!_schema.IsValidated)
                throw ValidationException.CannotSerializeInvalidSchema();

            _options = options;
            _indentCharacter = _options.IndentCharacter == IndentCharacter.Space ? ' ' : '\t';
            _builder.Clear();

            IDocumentNodeVisitors visitor = this;
            visitor.Visit(_schema.Root!);
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
            if ((directiveDefinition.IsBuiltIn && !_options.IncludeBuiltIn) ||
                (!directiveDefinition.IsRooted && !_options.IncludeUnrooted))
                return;

            PrintDescription(directiveDefinition.Description);
            StartLine();
            Print($"directive {directiveDefinition.Name}");

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
            foreach (var location in locations)
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
            if ((scalarType.IsBuiltIn && !_options.IncludeBuiltIn) ||
                (!scalarType.IsRooted && !_options.IncludeUnrooted))
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
            if ((objectType.IsBuiltIn && !_options.IncludeBuiltIn) ||
                (!objectType.IsRooted && !_options.IncludeUnrooted))
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
            if ((interfaceType.IsBuiltIn && !_options.IncludeBuiltIn) ||
                (!interfaceType.IsRooted && !_options.IncludeUnrooted))
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
            if ((unionType.IsBuiltIn && !_options.IncludeBuiltIn) ||
                (!unionType.IsRooted && !_options.IncludeUnrooted))
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
            if ((enumType.IsBuiltIn && !_options.IncludeBuiltIn) ||
                (!enumType.IsRooted && !_options.IncludeUnrooted))
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
            if ((inputObjectType.IsBuiltIn && !_options.IncludeBuiltIn) ||
                (!inputObjectType.IsRooted && !_options.IncludeUnrooted))
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
        }

        public void VisitSchemaDefinition(SchemaRoot schemaRoot)
        {
            PrintDescription(schemaRoot.Description);
            StartLine();
            Print($"schema");
            PrintDirectives(schemaRoot.Directives);
            EndLine();

            PrintLine("{");
            StartIndent();

            if (schemaRoot.Query is not null)
                PrintLine($"{schemaRoot.Query.Operation.ToString().ToLower()}: {schemaRoot.Query.NamedType}");

            if (schemaRoot.Mutation is not null)
                PrintLine($"{schemaRoot.Mutation.Operation.ToString().ToLower()}: {schemaRoot.Mutation.NamedType}");

            if (schemaRoot.Subscription is not null)
                PrintLine($"{schemaRoot.Subscription.Operation.ToString().ToLower()}: {schemaRoot.Subscription.NamedType}");

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
            return !_options.IncludeDescription || string.IsNullOrEmpty(description);
        }

        private void PrintDescription(string description)
        {
            if (!IsDescriptionEmpty(description))
            {
                PrintLine($"\"\"\"");
                foreach (var line in description.Split('\n'))
                    PrintLine(line.Replace("\r", ""));
                PrintLine($"\"\"\"");
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
                Print($" {directive.Name}");

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

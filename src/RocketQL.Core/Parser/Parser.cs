﻿namespace RocketQL.Core;

public ref struct Parser
{
    private Tokenizer _tokenizer;
    private string? _description = null;

    public Parser(ReadOnlySpan<char> text)
    {
        _tokenizer = new Tokenizer(text);
    }

    public DocumentNode Parse()
    {
        DirectiveDefinitionNodeList? _directiveDefinitions = null;
        ScalarTypeDefinitionNodeList? _scalarTypeDefinitions = null;

        // Move to the first real token
        _tokenizer.Next();

        // Keep processing until we reach the end of the tokens or throw because of an exception
        while(_tokenizer.TokenKind != TokenKind.EndOfText)
        {
            switch(_tokenizer.TokenKind) 
            {
                case TokenKind.StringValue:
                    _description = _tokenizer.TokenString;
                    _tokenizer.Next();
                    break;
                case TokenKind.Name:
                    switch (_tokenizer.TokenValue)
                    {
                        case "directive":
                            _directiveDefinitions ??= new();
                            _directiveDefinitions.Add(ParseDirectiveDefinition());
                            break;
                        case "scalar":
                            _scalarTypeDefinitions ??= new();
                            _scalarTypeDefinitions.Add(ParseScalarTypeDefinition());
                            break;
                    }
                    break;
                default:
                    throw SyntaxException.UnrecognizedToken(_tokenizer.Location, _tokenizer.TokenKind);
            }
        }

        return new DocumentNode(_directiveDefinitions, _scalarTypeDefinitions);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectiveDefinitionNode ParseDirectiveDefinition() 
    {
        MandatoryNextToken(TokenKind.At);
        MandatoryNextToken(TokenKind.Name);
        string name  = _tokenizer.TokenValue;
        MandatoryNext();
        var arguments = ParseArgumentsOptionalDefinition();
        var repeatable = OptionalKeyword("repeatable");
        MandatoryKeyword("on");
        var directiveLocations = ParseDirectiveLocations();

        return new DirectiveDefinitionNode(UseTopLevelDescription(), name, arguments, repeatable, directiveLocations);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ScalarTypeDefinitionNode ParseScalarTypeDefinition()
    {
        MandatoryNextToken(TokenKind.Name);
        string name = _tokenizer.TokenValue;
        _tokenizer.Next();
        var directives = ParseDirectivesOptional();

        return new ScalarTypeDefinitionNode(UseTopLevelDescription(), name, directives);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private InputValueDefinitionNodeList? ParseArgumentsOptionalDefinition()
    {
        if (_tokenizer.TokenKind == TokenKind.LeftParenthesis)
        {
            MandatoryNext();
            var arguments = ParseInputValueListDefinition();
            MandatoryTokenNext(TokenKind.RightParenthesis);
            return arguments;
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private InputValueDefinitionNodeList? ParseInputValueListDefinition()
    {
        InputValueDefinitionNodeList? list = null;

        do
        {
            var description = OptionalString();
            MandatoryToken(TokenKind.Name);
            string name = _tokenizer.TokenValue;
            MandatoryNextToken(TokenKind.Colon);
            MandatoryNext();
            var type = ParseType();
            var defaultValue = ParseDefaultValueOptional();
            var directives = ParseDirectivesOptional();
            list ??= new();
            list.Add(new InputValueDefinitionNode(description ?? string.Empty, name, type, defaultValue, directives));

        } while ((_tokenizer.TokenKind == TokenKind.Name) || (_tokenizer.TokenKind == TokenKind.StringValue));

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueNode? ParseDefaultValueOptional()
    {
        if (_tokenizer.TokenKind != TokenKind.Equals)
            return null;

        MandatoryNext();
        return ParseValue(constant: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueNode ParseValue(bool constant)
    {
        ValueNode node;

        switch (_tokenizer.TokenKind)
        {
            // TODO Variables
            case TokenKind.IntValue:
                node = new IntValueNode(_tokenizer.TokenValue);
                break;
            case TokenKind.FloatValue:
                node = new FloatValueNode(_tokenizer.TokenValue);
                break;
            case TokenKind.StringValue:
                node = new StringValueNode(_tokenizer.TokenString);
                break;
            case TokenKind.Name:
                {
                    var name = _tokenizer.TokenValue;
                    node = name switch
                    {
                        "true" => new BooleanValueNode(true),
                        "false" => new BooleanValueNode(false),
                        "null" => new NullValueNode(),
                        _ => new EnumValueNode(name),
                    };
                }
                break;
            case TokenKind.LeftSquareBracket:
                {
                    ValueNodeList? values = null;

                    MandatoryNext();
                    while (_tokenizer.TokenKind != TokenKind.RightSquareBracket)
                    {
                        values ??= new();
                        values.Add(ParseValue(constant: constant));
                    }

                    node = new ListValueNode(values);
                }
                break;
            case TokenKind.LeftCurlyBracket:
                {
                    ObjectFieldNodeList? objectsFields = null;

                    MandatoryNext();
                    while (_tokenizer.TokenKind != TokenKind.RightCurlyBracket)
                    {
                        MandatoryToken(TokenKind.Name);
                        string name = _tokenizer.TokenValue;
                        MandatoryNextToken(TokenKind.Colon);
                        MandatoryNext();
                        objectsFields ??= new();
                        objectsFields.Add(new ObjectFieldNode(name, ParseValue(constant: constant)));
                    }

                    node = new ObjectValueNode(objectsFields);
                }
                break;
            default:
                throw SyntaxException.TokenNotAllowedHere(_tokenizer.Location, _tokenizer.TokenKind);
        }

        MandatoryNext();
        return node;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectiveNodeList? ParseDirectivesOptional()
    {
        DirectiveNodeList? list = null;

        while(_tokenizer.TokenKind == TokenKind.At)
        {
            MandatoryNextToken(TokenKind.Name);
            string name = _tokenizer.TokenValue;
            _tokenizer.Next();
            var arguments = ParseArgumentsOptional(true);
            list ??= new();
            list.Add(new DirectiveNode(name, arguments));
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ObjectFieldNodeList? ParseArgumentsOptional(bool constant)
    {
        ObjectFieldNodeList? list = null;

        if (_tokenizer.TokenKind == TokenKind.LeftParenthesis)
        {
            MandatoryNext();
            while (_tokenizer.TokenKind != TokenKind.RightParenthesis)
            {
                MandatoryToken(TokenKind.Name);
                string name = _tokenizer.TokenValue;
                MandatoryNextToken(TokenKind.Colon);
                MandatoryNext();
                list ??= new();
                list.Add(new ObjectFieldNode(name, ParseValue(constant: constant)));
            }

            MandatoryNext();
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TypeNode ParseType()
    {
        switch(_tokenizer.TokenKind) 
        {
            case TokenKind.Name:
                {
                    var name = _tokenizer.TokenValue;
                    MandatoryNext();
                    var nonNull = OptionalToken(TokenKind.Exclamation);
                    return new TypeNameNode(name, nonNull);
                }
            case TokenKind.LeftSquareBracket:
                {
                    MandatoryNext();
                    var listType = ParseType();
                    MandatoryTokenNext(TokenKind.RightSquareBracket);
                    var nonNull = OptionalToken(TokenKind.Exclamation);
                    return new TypeListNode(listType, nonNull);
                }
            default:
                throw SyntaxException.TypeMustBeNameOrList(_tokenizer.Location, _tokenizer.TokenKind);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectiveLocations ParseDirectiveLocations()
    {
        MandatoryNextToken(TokenKind.Name);
        DirectiveLocations directiveLocations = StringToDriveLocations(_tokenizer.TokenValue);

        if (_tokenizer.Next())
        {
            while (_tokenizer.TokenKind == TokenKind.Vertical)
            {
                MandatoryNextToken(TokenKind.Name);
                directiveLocations |= StringToDriveLocations(_tokenizer.TokenValue);
                _tokenizer.Next();
            }
        }

        return directiveLocations;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DirectiveLocations StringToDriveLocations(string location)
    {
        return location switch
        {
            // Executable Directive Locations
            "QUERY" => DirectiveLocations.QUERY,
            "MUTATION" => DirectiveLocations.MUTATION,
            "SUBSCRIPTION" => DirectiveLocations.SUBSCRIPTION,
            "FIELD" => DirectiveLocations.FIELD,
            "FRAGMENT_DEFINITION" => DirectiveLocations.FRAGMENT_DEFINITION,
            "FRAGMENT_SPREAD" => DirectiveLocations.FRAGMENT_SPREAD,
            "INLINE_FRAGMENT" => DirectiveLocations.INLINE_FRAGMENT,
            "VARIABLE_DEFINITION" => DirectiveLocations.VARIABLE_DEFINITION,

            // Type System Directive Locations
            "SCHEMA" => DirectiveLocations.SCHEMA,
            "SCALAR" => DirectiveLocations.SCALAR,
            "OBJECT" => DirectiveLocations.OBJECT,
            "FIELD_DEFINITION" => DirectiveLocations.FIELD_DEFINITION,
            "ARGUMENT_DEFINITION" => DirectiveLocations.ARGUMENT_DEFINITION,
            "INTERFACE" => DirectiveLocations.INTERFACE,
            "UNION" => DirectiveLocations.UNION,
            "ENUM" => DirectiveLocations.ENUM,
            "ENUM_VALUE" => DirectiveLocations.ENUM_VALUE,
            "INPUT_OBJECT" => DirectiveLocations.INPUT_OBJECT,
            "INPUT_FIELD_DEFINITION" => DirectiveLocations.INPUT_FIELD_DEFINITION,

            _ => throw SyntaxException.ExpectedDirectiveLocationNotFound(_tokenizer.Location, location)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string UseTopLevelDescription()
    {
        string? description = _description;
        _description = null;
        return description ?? String.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryNext()
    {
        if ((_tokenizer.TokenKind == TokenKind.EndOfText) || !_tokenizer.Next())
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryToken(TokenKind token) 
    {
        if (_tokenizer.TokenKind == TokenKind.EndOfText)
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != token)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, token, _tokenizer.TokenKind);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryNextToken(TokenKind token)
    {
        if ((_tokenizer.TokenKind == TokenKind.EndOfText) || !_tokenizer.Next())
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != token)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, token, _tokenizer.TokenKind);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryTokenNext(TokenKind token)
    {
        if (_tokenizer.TokenKind == TokenKind.EndOfText)
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != token)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, token, _tokenizer.TokenKind);

        if (!_tokenizer.Next())
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MandatoryKeyword(string keyword)
    {
        if (_tokenizer.TokenKind == TokenKind.EndOfText)
            throw SyntaxException.UnexpectedEndOfFile(_tokenizer.Location);

        if (_tokenizer.TokenKind != TokenKind.Name)
            throw SyntaxException.ExpectedTokenNotFound(_tokenizer.Location, TokenKind.Name, _tokenizer.TokenKind);

        if (_tokenizer.TokenValue != keyword)
            throw SyntaxException.ExpectedKeywordNotFound(_tokenizer.Location, "on", _tokenizer.TokenValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool OptionalToken(TokenKind tokenKind)
    {
        if (_tokenizer.TokenKind == tokenKind)
        {
            _tokenizer.Next();
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool OptionalKeyword(string keyword)
    {
        if ((_tokenizer.TokenKind == TokenKind.Name) && (_tokenizer.TokenValue == keyword))
        {
            _tokenizer.Next();
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string? OptionalString()
    {
        if (_tokenizer.TokenKind == TokenKind.StringValue)
        {
            string ret = _tokenizer.TokenString;
            _tokenizer.Next();
            return ret;
        }

        return null;
    }
}

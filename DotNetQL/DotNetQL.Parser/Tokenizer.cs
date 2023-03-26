namespace DotNetQL.Parser
{
    public ref struct Tokenizer
    {
        private static readonly TokenKind[] _map = new TokenKind[65536];
        private static readonly TokenKind[] _hex = new TokenKind[65536];

        private int _length = 0;
        private int _index = 0;
        private int _lineIndex = 0;
        private int _lineNumber = 1;
        private int _tokenIndex = 0;
        private TokenKind _tokenKind = TokenKind.StartOfText;
        private readonly ReadOnlySpan<char> _text;

        static Tokenizer()
        {
            // Skip the Byte Order Mark (BOM) and simple whitespace
            _map[0xFEFF] = TokenKind.Skip;
            _map[' '] = TokenKind.Skip;
            _map['\t'] = TokenKind.Skip;
            _map[','] = TokenKind.Skip;

            // Line terminators
            _map['\n'] = TokenKind.NewLine;
            _map['\r'] = TokenKind.CarriageReturn;

            // Single line comment
            _map['#'] = TokenKind.Hash;

            // Punctuators
            _map['!'] = TokenKind.Exclamation;
            _map['$'] = TokenKind.Dollar;
            _map['&'] = TokenKind.Ampersand;
            _map['('] = TokenKind.LeftParenthesis;
            _map[')'] = TokenKind.RightParenthesis;
            _map[':'] = TokenKind.Colon;
            _map['='] = TokenKind.Equals;
            _map['@'] = TokenKind.At;
            _map['['] = TokenKind.LeftSquareBracket;
            _map[']'] = TokenKind.RightSquareBracket;
            _map['{'] = TokenKind.LeftCurlyBracket;
            _map['}'] = TokenKind.RightCurlyBracket;
            _map['|'] = TokenKind.Vertical;
            _map['.'] = TokenKind.Dot;

            // Names (letters and numbers)
            _map['_'] = TokenKind.Underscore;
            for (char c = 'A'; c <= 'Z'; c++)
                _map[c] = TokenKind.Letter;
            for (char c = 'a'; c <= 'z'; c++)
                _map[c] = TokenKind.Letter;
            for (char c = '0'; c <= '9'; c++)
                _map[c] = TokenKind.Digit;

            // Signs for numbers
            _map['-'] = TokenKind.Minus;
            _map['+'] = TokenKind.Plus;

            // Strings
            _map['"'] = TokenKind.DoubleQuote;

            // Hexadecimal values
            for (char c = 'A'; c <= 'Z'; c++)
                _hex[c] = TokenKind.Hexadecimal;
            for (char c = 'a'; c <= 'z'; c++)
                _hex[c] = TokenKind.Hexadecimal;
            for (char c = '0'; c <= '9'; c++)
                _hex[c] = TokenKind.Hexadecimal;
            _hex['}'] = TokenKind.RightCurlyBracket;
        }

        public Tokenizer(ReadOnlySpan<char> text)
        {
            if (text.Length == 0)
                _tokenKind = TokenKind.EndOfText;
            else
            {
                _text = text;
                _length = _text.Length;
                _tokenKind = TokenKind.StartOfText;
            }
        }

        public TokenKind Token => _tokenKind;
        public int LineNumber => _lineNumber;
        public int ColumnNumber => 1 + _tokenIndex - _lineIndex;
        public string TokenString => new(_text.Slice(_tokenIndex, _index - _tokenIndex));

        public TokenKind Next()
        {
            while(_index < _length)
            {
                char c = _text[_index++];
                TokenKind token = _map[c];

                switch (token)
                {
                    case TokenKind.IllegalCharacter:
                        throw new ApplicationException("IllegalCharacter");
                    case TokenKind.Skip:
                        break;
                    case TokenKind.NewLine:
                        _lineNumber++;
                        _lineIndex = _index;
                        break;
                    case TokenKind.CarriageReturn:
                        // Skip over any following newline character
                        if ((_index < _length) && (_text[_index] == '\n'))
                            _index++;

                        _lineNumber++;
                        _lineIndex = _index;
                        break;
                    case TokenKind.Hash:
                        // Ignore everything until we reach the end of the line
                        while ((_index < _length) && (_text[_index] != '\r') && (_text[_index] != '\n'))
                            _index++;
                        break;
                    case TokenKind.Exclamation:
                    case TokenKind.Dollar:
                    case TokenKind.Ampersand:
                    case TokenKind.LeftParenthesis:
                    case TokenKind.RightParenthesis:
                    case TokenKind.Colon:
                    case TokenKind.Equals:
                    case TokenKind.At:
                    case TokenKind.LeftSquareBracket:
                    case TokenKind.RightSquareBracket:
                    case TokenKind.LeftCurlyBracket:
                    case TokenKind.RightCurlyBracket:
                    case TokenKind.Vertical:
                        _tokenIndex = _index - 1;
                        return _tokenKind = token;
                    case TokenKind.Dot:
                        // Should be 3 dots in a row to be the spread operator
                        _tokenIndex = _index - 1;
                        if (((_index + 1) < _length) && (_text[_index] == '.') && (_text[_index + 1] == '.'))
                        {
                            _index += 2;
                            return _tokenKind = TokenKind.Spread;
                        }
                        throw new ApplicationException($"Less than 3 dots found when only spread allowed");
                    case TokenKind.Underscore:
                    case TokenKind.Letter:
                        _tokenIndex = _index - 1;
                        while (_index < _length)
                        {
                            switch (_map[_text[_index]])
                            {
                                case TokenKind.Underscore:
                                case TokenKind.Letter:
                                case TokenKind.Digit:
                                    _index++;
                                    continue;
                            }

                            break;
                        }
                        return _tokenKind = TokenKind.Name;
                    case TokenKind.Digit:
                    case TokenKind.Minus:
                        _tokenIndex = _index - 1;
                        if (c == '-')
                        {
                            if (_index == _length)
                                throw new ApplicationException($"Unexpected end of file.");

                            c = _text[_index++];
                            if (_map[c] != TokenKind.Digit)
                                throw new ApplicationException($"Minus must be followed by a digit.");
                        }

                        if (c != '0')
                        {
                            while (_index < _length && _map[_text[_index]] == TokenKind.Digit)
                                _index++;
                        }

                        if (_index == _length)
                            return _tokenKind = TokenKind.IntValue;

                        c = _text[_index];
                        if (c == '.')
                        {
                            _index++;
                            if (_index == _length)
                                throw new ApplicationException($"Unexpected end of file.");

                            c = _text[_index++];
                            if (_map[c] != TokenKind.Digit)
                                throw new ApplicationException($"Decimal point must be followed by a digit.");

                            while (_index < _length && _map[_text[_index]] == TokenKind.Digit)
                                _index++;

                            if (_index == _length)
                                return _tokenKind = TokenKind.FloatValue;

                            c = _text[_index];
                            if ((c == 'e') || (c == 'E'))
                            {
                                _index++;
                                if (_index == _length)
                                    throw new ApplicationException($"Unexpected end of file.");

                                c = _text[_index++];
                                if ((_map[c] == TokenKind.Minus) || (_map[c] == TokenKind.Plus))
                                {
                                    if (_index == _length)
                                        throw new ApplicationException($"Unexpected end of file.");

                                    c = _text[_index++];
                                }

                                if (_map[c] != TokenKind.Digit)
                                    throw new ApplicationException($"Exponent must be followed by a digit.");

                                while (_index < _length && _map[_text[_index]] == TokenKind.Digit)
                                    _index++;

                                if (_index == _length)
                                    return _tokenKind = TokenKind.FloatValue;

                                switch (_map[_text[_index]])
                                {
                                    case TokenKind.Dot:
                                        throw new ApplicationException($"Float value cannot be followed by a decimal point.");
                                    case TokenKind.Underscore:
                                        throw new ApplicationException($"Float value cannot be followed by an underscore.");
                                    case TokenKind.Letter:
                                        throw new ApplicationException($"Float value cannot be followed by a letter.");
                                }
                            }

                            return _tokenKind = TokenKind.FloatValue;
                        }
                        else if ((c == 'e') || (c == 'E'))
                        {
                            _index++;
                            if (_index == _length)
                                throw new ApplicationException($"Unexpected end of file.");

                            c = _text[_index++];
                            if ((_map[c] == TokenKind.Minus) || (_map[c] == TokenKind.Plus))
                            {
                                if (_index == _length)
                                    throw new ApplicationException($"Unexpected end of file.");

                                c = _text[_index++];
                            }

                            if (_map[c] != TokenKind.Digit)
                                throw new ApplicationException($"Exponent must be followed by a digit.");

                            while (_index < _length && _map[_text[_index]] == TokenKind.Digit)
                                _index++;

                            if (_index == _length)
                                return _tokenKind = TokenKind.FloatValue;

                            switch (_map[_text[_index]])
                            {
                                case TokenKind.Dot:
                                    throw new ApplicationException($"Float value cannot be followed by a decimal point.");
                                case TokenKind.Underscore:
                                    throw new ApplicationException($"Float value cannot be followed by an underscore.");
                                case TokenKind.Letter:
                                    throw new ApplicationException($"Float value cannot be followed by a letter.");
                            }

                            return _tokenKind = TokenKind.FloatValue;
                        }
                        else if (c == '_')
                        {
                            throw new ApplicationException($"Integer value cannot be followed by an underscore.");
                        }
                        else if (_map[c] == TokenKind.Letter)
                        {
                            throw new ApplicationException($"Integer value cannot be followed by a letter.");
                        }

                        return _tokenKind = TokenKind.IntValue;
                    case TokenKind.DoubleQuote:
                        _tokenIndex = _index;
                        if (((_index + 1) < _length) && (_text[_index] == '"') && (_text[_index + 1] == '"'))
                        {
                            _index += 2;
                            while (_index < _length)
                            {
                                if (_text[_index++] == '"')
                                {
                                    if (((_index + 1) < _length) && (_text[_index] == '"') && (_text[_index + 1] == '"'))
                                    {
                                        _index += 2;
                                        return TokenKind.StringValue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            while (_index < _length)
                            {
                                c = _text[_index];
                                if (c == '"')
                                {
                                    _index++;
                                    return TokenKind.StringValue;
                                }
                                else if (c == '\\')
                                {
                                    _index++;
                                    if (_index == _length)
                                        throw new ApplicationException($"Unexpected end of file.");

                                    c = _text[_index];
                                    switch (c)
                                    {
                                        case '\"':
                                        case '\\':
                                        case '/':
                                        case 'b':
                                        case 'f':
                                        case 'n':
                                        case 'r':
                                        case 't':
                                            break;
                                        case 'u':
                                            _index++;
                                            if (_index == _length)
                                                throw new ApplicationException($"Unexpected end of file.");

                                            c = _text[_index];
                                            if (c == '{')
                                            {
                                                int digits = 0;
                                                while (true)
                                                {
                                                    _index++;
                                                    if (_index == _length)
                                                        throw new ApplicationException($"Unexpected end of file.");

                                                    c = _text[_index];
                                                    TokenKind hexToken = _hex[c];
                                                    if (hexToken == TokenKind.Hexadecimal)
                                                        digits++;
                                                    else
                                                    {
                                                        if (hexToken == TokenKind.RightCurlyBracket)
                                                        {
                                                            if (digits == 0)
                                                                throw new ApplicationException($"Escaped character must have at least 1 hexadecimal digit.");

                                                            break;
                                                        }
                                                        else
                                                            throw new ApplicationException($"Escape code specify value using hexadecimal character.");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if ((_index + 3) >= _length)
                                                    throw new ApplicationException($"Unexpected end of file.");

                                                TokenKind hexToken1 = _hex[c];
                                                TokenKind hexToken2 = _hex[_text[_index++]];
                                                TokenKind hexToken3 = _hex[_text[_index++]];
                                                TokenKind hexToken4 = _hex[_text[_index++]];
                                                if ((hexToken1 != TokenKind.Hexadecimal) || (hexToken2 != TokenKind.Hexadecimal) || (hexToken3 != TokenKind.Hexadecimal) || (hexToken4 != TokenKind.Hexadecimal))
                                                    throw new ApplicationException($"Escape code specify value using hexadecimal character.");
                                            }
                                            break;
                                        default:
                                            throw new ApplicationException($"Escaped character is not one of '\"\t\\\t/\tb\tf\tn\tr\tt\r\n'.");
                                    }
                                }

                                _index++;
                            }
                        }

                        throw new ApplicationException($"Unexpected end of file.");
                    default:
                        throw new ApplicationException($"Unrecognized '{c}'");
                }
            }

            _tokenIndex = _index;
            return _tokenKind = TokenKind.EndOfText;
        }
    }

    public enum TokenKind : uint
    {
        IllegalCharacter,
        Skip,
        NewLine,
        CarriageReturn,
        Hash,
        Exclamation,
        Dollar,
        Ampersand,
        LeftParenthesis,
        RightParenthesis,
        Colon,
        Equals,
        At,
        LeftSquareBracket,
        RightSquareBracket,
        LeftCurlyBracket,
        RightCurlyBracket,
        Vertical,
        Dot,
        Underscore,
        Letter,
        Digit,
        Minus,
        DoubleQuote,
        Plus,
        Hexadecimal,
        StartOfText,
        EndOfText,
        Spread,
        Name,
        IntValue,
        FloatValue,
        StringValue
    }
}

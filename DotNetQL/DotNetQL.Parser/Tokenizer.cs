namespace DotNetQL.Parser
{
    public ref struct Tokenizer
    {
        private static Token[] _map = new Token[65536];
        private static Token[] _hex = new Token[65536];

        private int _length = 0;
        private int _index = 0;
        private int _lineIndex = 0;
        private int _lineNumber = 1;
        private int _tokenIndex = 0;
        private Token _token = Token.StartOfText;
        private ReadOnlySpan<char> _text;

        static Tokenizer()
        {
            // Skip the Byte Order Mark (BOM) and simple whitespace
            _map[0xFEFF] = Token.Skip;
            _map[' '] = Token.Skip;
            _map['\t'] = Token.Skip;
            _map[','] = Token.Skip;

            // Line terminators
            _map['\n'] = Token.NewLine;
            _map['\r'] = Token.CarriageReturn;

            // Single line comment
            _map['#'] = Token.Hash;

            // Punctuators
            _map['!'] = Token.Exclamation;
            _map['$'] = Token.Dollar;
            _map['&'] = Token.Ampersand;
            _map['('] = Token.LeftParenthesis;
            _map[')'] = Token.RightParenthesis;
            _map[':'] = Token.Colon;
            _map['='] = Token.Equals;
            _map['@'] = Token.At;
            _map['['] = Token.LeftSquareBracket;
            _map[']'] = Token.RightSquareBracket;
            _map['{'] = Token.LeftCurlyBracket;
            _map['}'] = Token.RightCurlyBracket;
            _map['|'] = Token.Vertical;
            _map['.'] = Token.Dot;

            // Letters and numbers
            _map['_'] = Token.Underscore;
            for (char c = 'A'; c <= 'Z'; c++)
                _map[c] = Token.Letter;
            for (char c = 'a'; c <= 'z'; c++)
                _map[c] = Token.Letter;
            for (char c = '0'; c <= '9'; c++)
                _map[c] = Token.Digit;

            // Signs
            _map['-'] = Token.Minus;
            _map['+'] = Token.Plus;

            // String related
            _map['"'] = Token.DoubleQuote;

            // Hexadecimal values
            for (char c = 'A'; c <= 'Z'; c++)
                _hex[c] = Token.Hexadecimal;
            for (char c = 'a'; c <= 'z'; c++)
                _hex[c] = Token.Hexadecimal;
            for (char c = '0'; c <= '9'; c++)
                _hex[c] = Token.Hexadecimal;
            _hex['}'] = Token.RightCurlyBracket;
        }

        public Tokenizer(ReadOnlySpan<char> text)
        {
            if (text.Length == 0)
                _token = Token.EndOfText;
            else
            {
                _text = text;
                _length = _text.Length;
                _token = Token.StartOfText;
                _token = Next();
            }
        }

        public Token Token => _token;
        public int LineNumber => _lineNumber;
        public int ColumnNumber => 1 + _tokenIndex - _lineIndex;
        public string TokenString => new(_text.Slice(_tokenIndex, _index - _tokenIndex));

        public Token Next()
        {
            while(_index < _length)
            {
                char c = _text[_index++];
                Token token = _map[c];

                switch (token)
                {
                    case Token.IllegalCharacter:
                        throw new ApplicationException("IllegalCharacter");
                    case Token.Skip:
                        break;
                    case Token.NewLine:
                        _lineNumber++;
                        _lineIndex = _index;
                        break;
                    case Token.CarriageReturn:
                        // Skip over any following newline character
                        if ((_index < _length) && (_text[_index] == '\n'))
                            _index++;

                        _lineNumber++;
                        _lineIndex = _index;
                        break;
                    case Token.Hash:
                        // Ignore everything until we reach the end of the line
                        while ((_index < _length) && (_text[_index] != '\r') && (_text[_index] != '\n'))
                            _index++;
                        break;
                    case Token.Exclamation:
                    case Token.Dollar:
                    case Token.Ampersand:
                    case Token.LeftParenthesis:
                    case Token.RightParenthesis:
                    case Token.Colon:
                    case Token.Equals:
                    case Token.At:
                    case Token.LeftSquareBracket:
                    case Token.RightSquareBracket:
                    case Token.LeftCurlyBracket:
                    case Token.RightCurlyBracket:
                    case Token.Vertical:
                        _tokenIndex = _index - 1;
                        return _token = token;
                    case Token.Dot:
                        // Should be 3 dots in a row to be the spread operator
                        _tokenIndex = _index - 1;
                        if (((_index + 1) < _length) && (_text[_index] == '.') && (_text[_index + 1] == '.'))
                        {
                            _index += 2;
                            return _token = Token.Spread;
                        }
                        throw new ApplicationException($"Less than 3 dots found when only spread allowed");
                    case Token.Underscore:
                    case Token.Letter:
                        _tokenIndex = _index - 1;
                        while (_index < _length)
                        {
                            switch (_map[_text[_index]])
                            {
                                case Token.Underscore:
                                case Token.Letter:
                                case Token.Digit:
                                    _index++;
                                    continue;
                            }

                            break;
                        }
                        return _token = Token.Name;
                    case Token.Digit:
                    case Token.Minus:
                        _tokenIndex = _index - 1;
                        if (c == '-')
                        {
                            if (_index == _length)
                                throw new ApplicationException($"Unexpected end of file.");

                            c = _text[_index++];
                            if (_map[c] != Token.Digit)
                                throw new ApplicationException($"Minus must be followed by a digit.");
                        }

                        if (c != '0')
                        {
                            while (_index < _length && _map[_text[_index]] == Token.Digit)
                                _index++;
                        }

                        if (_index == _length)
                            return _token = Token.IntValue;

                        c = _text[_index];
                        if (c == '.')
                        {
                            _index++;
                            if (_index == _length)
                                throw new ApplicationException($"Unexpected end of file.");

                            c = _text[_index++];
                            if (_map[c] != Token.Digit)
                                throw new ApplicationException($"Decimal point must be followed by a digit.");

                            while (_index < _length && _map[_text[_index]] == Token.Digit)
                                _index++;

                            if (_index == _length)
                                return _token = Token.FloatValue;

                            c = _text[_index];
                            if ((c == 'e') || (c == 'E'))
                            {
                                _index++;
                                if (_index == _length)
                                    throw new ApplicationException($"Unexpected end of file.");

                                c = _text[_index++];
                                if ((_map[c] == Token.Minus) || (_map[c] == Token.Plus))
                                {
                                    if (_index == _length)
                                        throw new ApplicationException($"Unexpected end of file.");

                                    c = _text[_index++];
                                }

                                if (_map[c] != Token.Digit)
                                    throw new ApplicationException($"Exponent must be followed by a digit.");

                                while (_index < _length && _map[_text[_index]] == Token.Digit)
                                    _index++;

                                if (_index == _length)
                                    return _token = Token.FloatValue;

                                switch (_map[_text[_index]])
                                {
                                    case Token.Dot:
                                        throw new ApplicationException($"Float value cannot be followed by a decimal point.");
                                    case Token.Underscore:
                                        throw new ApplicationException($"Float value cannot be followed by an underscore.");
                                    case Token.Letter:
                                        throw new ApplicationException($"Float value cannot be followed by a letter.");
                                }
                            }

                            return _token = Token.FloatValue;
                        }
                        else if ((c == 'e') || (c == 'E'))
                        {
                            _index++;
                            if (_index == _length)
                                throw new ApplicationException($"Unexpected end of file.");

                            c = _text[_index++];
                            if ((_map[c] == Token.Minus) || (_map[c] == Token.Plus))
                            {
                                if (_index == _length)
                                    throw new ApplicationException($"Unexpected end of file.");

                                c = _text[_index++];
                            }

                            if (_map[c] != Token.Digit)
                                throw new ApplicationException($"Exponent must be followed by a digit.");

                            while (_index < _length && _map[_text[_index]] == Token.Digit)
                                _index++;

                            if (_index == _length)
                                return _token = Token.FloatValue;

                            switch (_map[_text[_index]])
                            {
                                case Token.Dot:
                                    throw new ApplicationException($"Float value cannot be followed by a decimal point.");
                                case Token.Underscore:
                                    throw new ApplicationException($"Float value cannot be followed by an underscore.");
                                case Token.Letter:
                                    throw new ApplicationException($"Float value cannot be followed by a letter.");
                            }

                            return _token = Token.FloatValue;
                        }
                        else if (c == '_')
                        {
                            throw new ApplicationException($"Integer value cannot be followed by an underscore.");
                        }
                        else if (_map[c] == Token.Letter)
                        {
                            throw new ApplicationException($"Integer value cannot be followed by a letter.");
                        }

                        return _token = Token.IntValue;
                    case Token.DoubleQuote:
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
                                        return Token.StringValue;
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
                                    return Token.StringValue;
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
                                                    Token hexToken = _hex[c];
                                                    if (hexToken == Token.Hexadecimal)
                                                        digits++;
                                                    else
                                                    {
                                                        if (hexToken == Token.RightCurlyBracket)
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

                                                Token hexToken1 = _hex[c];
                                                Token hexToken2 = _hex[_text[_index++]];
                                                Token hexToken3 = _hex[_text[_index++]];
                                                Token hexToken4 = _hex[_text[_index++]];
                                                if ((hexToken1 != Token.Hexadecimal) || (hexToken2 != Token.Hexadecimal) || (hexToken3 != Token.Hexadecimal) || (hexToken4 != Token.Hexadecimal))
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
            return _token = Token.EndOfText;
        }
    }

    public enum Token : byte
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
        Plus,
        DoubleQuote,
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

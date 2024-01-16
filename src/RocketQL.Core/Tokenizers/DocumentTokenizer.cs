namespace RocketQL.Core.Tokenizers;

public ref struct DocumentTokenizer
{
    private static readonly FullTokenKind[] s_mapKind = new FullTokenKind[65536];
    private static readonly FullTokenKind[] s_hexKind = new FullTokenKind[65536];
    private static readonly byte[] s_hexValues = new byte[65536];
    private static readonly EscapeKind[] s_escKind = new EscapeKind[65536];
    private static readonly char[] s_escChar = new char[(int)EscapeKind.u];
    private static readonly ThreadLocal<StringBuilder> s_cachedBuilder = new(() => new(4096));

    private readonly string _source;
    private readonly ReadOnlySpan<char> _text;
    private readonly int _length = 0;
    private readonly StringBuilder _sb;
    private FullTokenKind _tokenKind = FullTokenKind.StartOfText;
    private char _c = ' ';
    private int _index = 0;
    private int _lineIndex = 0;
    private int _lineNumber = 1;
    private int _tokenIndex = 0;

    static DocumentTokenizer()
    {
        // All _mapKind and _hexKind entries are implicitly FullTokenKind.IllegalCharacter because
        // it has the enumeration value of 0 and the arrays are initialized to 0 by default

        // Skip the Byte Order Mark (BOM) and simple whitespace
        s_mapKind[0xFEFF] = FullTokenKind.Skip;
        s_mapKind[' '] = FullTokenKind.Skip;
        s_mapKind['\t'] = FullTokenKind.Skip;
        s_mapKind[','] = FullTokenKind.Skip;

        // Individual characters, mostly punctuators
        s_mapKind['\n'] = FullTokenKind.NewLine;
        s_mapKind['\r'] = FullTokenKind.CarriageReturn;
        s_mapKind['#'] = FullTokenKind.Hash;
        s_mapKind['!'] = FullTokenKind.Exclamation;
        s_mapKind['$'] = FullTokenKind.Dollar;
        s_mapKind['&'] = FullTokenKind.Ampersand;
        s_mapKind['('] = FullTokenKind.LeftParenthesis;
        s_mapKind[')'] = FullTokenKind.RightParenthesis;
        s_mapKind[':'] = FullTokenKind.Colon;
        s_mapKind['='] = FullTokenKind.Equals;
        s_mapKind['@'] = FullTokenKind.At;
        s_mapKind['['] = FullTokenKind.LeftSquareBracket;
        s_mapKind[']'] = FullTokenKind.RightSquareBracket;
        s_mapKind['{'] = FullTokenKind.LeftCurlyBracket;
        s_mapKind['}'] = FullTokenKind.RightCurlyBracket;
        s_mapKind['|'] = FullTokenKind.Vertical;
        s_mapKind['.'] = FullTokenKind.Dot;
        s_mapKind['-'] = FullTokenKind.Minus;
        s_mapKind['+'] = FullTokenKind.Plus;
        s_mapKind['"'] = FullTokenKind.DoubleQuote;
        s_mapKind['_'] = FullTokenKind.Underscore;

        // Letters and numbers
        for (var c = 'A'; c <= 'Z'; c++)
            s_mapKind[c] = FullTokenKind.Letter;
        for (var c = 'a'; c <= 'z'; c++)
            s_mapKind[c] = FullTokenKind.Letter;
        for (var c = '0'; c <= '9'; c++)
            s_mapKind[c] = FullTokenKind.Digit;

        // Hexadecimal characters
        for (var c = 'A'; c <= 'F'; c++)
            s_hexKind[c] = FullTokenKind.Hexadecimal;
        for (var c = 'a'; c <= 'f'; c++)
            s_hexKind[c] = FullTokenKind.Hexadecimal;
        for (var c = '0'; c <= '9'; c++)
            s_hexKind[c] = FullTokenKind.Hexadecimal;
        s_hexKind['}'] = FullTokenKind.RightCurlyBracket;

        // Hexadecimal values
        for (var c = 'A'; c <= 'F'; c++)
            s_hexValues[c] = (byte)(c + 10 - 'A');
        for (var c = 'a'; c <= 'f'; c++)
            s_hexValues[c] = (byte)(c + 10 - 'a');
        for (var c = '0'; c <= '9'; c++)
            s_hexValues[c] = (byte)(c - '0');

        // Escape characters inside a simple string
        s_escKind['\"'] = EscapeKind.DoubleQuote;
        s_escKind['\\'] = EscapeKind.Backslash;
        s_escKind['/'] = EscapeKind.Slash;
        s_escKind['b'] = EscapeKind.Backspace;
        s_escKind['f'] = EscapeKind.Feed;
        s_escKind['n'] = EscapeKind.NewLine;
        s_escKind['r'] = EscapeKind.CarriageReturn;
        s_escKind['t'] = EscapeKind.Tab;
        s_escKind['u'] = EscapeKind.u;

        // Reverse lookup
        s_escChar[(int)EscapeKind.DoubleQuote] = '\"';
        s_escChar[(int)EscapeKind.Backslash] = '\\';
        s_escChar[(int)EscapeKind.Slash] = '/';
        s_escChar[(int)EscapeKind.Backspace] = '\b';
        s_escChar[(int)EscapeKind.Feed] = '\f';
        s_escChar[(int)EscapeKind.NewLine] = '\n';
        s_escChar[(int)EscapeKind.CarriageReturn] = '\r';
        s_escChar[(int)EscapeKind.Tab] = '\t';
    }

    public DocumentTokenizer(
        ReadOnlySpan<char> text,
        [CallerFilePath] string filePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0)
        : this(text, CallerExtensions.CallerToSource(filePath, memberName, lineNumber))
    {
    }

    public DocumentTokenizer(ReadOnlySpan<char> text, string source)
    {
        _source = source;
        _sb = s_cachedBuilder.Value!;

        if (text.Length == 0)
            _tokenKind = FullTokenKind.EndOfText;
        else
        {
            _text = text;
            _length = _text.Length;
            _tokenKind = FullTokenKind.StartOfText;
        }
    }

    public readonly DocumentTokenKind TokenKind => (DocumentTokenKind)_tokenKind;
    public readonly string TokenValue => new(_text[_tokenIndex.._index]);
    public readonly string TokenString => _sb.ToString();
    public readonly int LineNumber => _lineNumber;
    public readonly int ColumnNumber => 1 + _tokenIndex - _lineIndex;
    public readonly Location Location => new(_index, LineNumber, ColumnNumber, _source);

    public bool Next()
    {
        while (_index < _length)
        {
            _c = _text[_index++];
            var token = s_mapKind[_c];

            switch (token)
            {
                case FullTokenKind.IllegalCharacter:
                    _tokenIndex = _index - 1;
                    throw SyntaxException.IllegalCharacterCode(Location, _c);
                case FullTokenKind.Skip:
                    break;
                case FullTokenKind.NewLine:
                    ScanNewLine();
                    break;
                case FullTokenKind.CarriageReturn:
                    ScanCarriageReturn();
                    break;
                case FullTokenKind.Hash:
                    ScanComment();
                    break;
                case FullTokenKind.Exclamation:
                case FullTokenKind.Dollar:
                case FullTokenKind.Ampersand:
                case FullTokenKind.LeftParenthesis:
                case FullTokenKind.RightParenthesis:
                case FullTokenKind.Colon:
                case FullTokenKind.Equals:
                case FullTokenKind.At:
                case FullTokenKind.LeftSquareBracket:
                case FullTokenKind.RightSquareBracket:
                case FullTokenKind.LeftCurlyBracket:
                case FullTokenKind.RightCurlyBracket:
                case FullTokenKind.Vertical:
                    ScanPunctuator(token);
                    return true;
                case FullTokenKind.Dot:
                    ScanSpread();
                    return true;
                case FullTokenKind.Underscore:
                case FullTokenKind.Letter:
                    ScanName();
                    return true;
                case FullTokenKind.Digit:
                case FullTokenKind.Minus:
                    ScanNumber();
                    return true;
                case FullTokenKind.DoubleQuote:
                    ScanString();
                    return true;
                default:
                    _tokenIndex = _index - 1;
                    throw SyntaxException.UnrecognizedCharacterCode(Location, _c);
            }
        }

        _tokenIndex = _index;
        _tokenKind = FullTokenKind.EndOfText;
        return false;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanNewLine()
    {
        _lineNumber++;
        _lineIndex = _index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanCarriageReturn()
    {
        // Skip over any optional following newline
        if ((_index < _length) && (_text[_index] == '\n'))
            _index++;

        _lineNumber++;
        _lineIndex = _index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanComment()
    {
        // Single line comment, ignore everything until we reach the end of the line
        while ((_index < _length) && (_text[_index] != '\r') && (_text[_index] != '\n'))
            _index++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanPunctuator(FullTokenKind token)
    {
        _tokenIndex = _index - 1;
        _tokenKind = token;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanSpread()
    {
        _tokenIndex = _index - 1;

        if ((_index + 1) >= _length)
        {
            _index = _length;
            throw SyntaxException.UnexpectedEndOfFile(Location);
        }

        if ((_text[_index] == '.') && (_text[_index + 1] == '.'))
        {
            _index += 2;
            _tokenKind = FullTokenKind.Spread;
        }
        else
        {
            if (_text[_index] == '.')
                _index++;

            throw SyntaxException.SpreadNeedsThreeDots(Location);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanName()
    {
        _tokenIndex = _index - 1;
        while (_index < _length)
        {
            switch (s_mapKind[_text[_index]])
            {
                case FullTokenKind.Underscore:
                case FullTokenKind.Letter:
                case FullTokenKind.Digit:
                    _index++;
                    continue;
            }

            break;
        }

        _tokenKind = FullTokenKind.Name;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanNumber()
    {
        _tokenIndex = _index - 1;
        if (_c == '-')
        {
            if (_index == _length)
                throw SyntaxException.UnexpectedEndOfFile(Location);

            _c = _text[_index];
            if (s_mapKind[_c] != FullTokenKind.Digit)
                throw SyntaxException.MinusMustBeFollowedByDigit(Location);

            _index++;
        }

        if (_c != '0')
        {
            // Skip over all the whole number digits
            while ((_index < _length) && (s_mapKind[_text[_index]] == FullTokenKind.Digit))
                _index++;
        }

        if (_index == _length)
        {
            _tokenKind = FullTokenKind.IntValue;
            return;
        }

        _c = _text[_index];
        if (_c == '.')
        {
            if (++_index == _length)
                throw SyntaxException.UnexpectedEndOfFile(Location);

            _c = _text[_index];
            if (s_mapKind[_c] != FullTokenKind.Digit)
                throw SyntaxException.PointMustBeFollowedByDigit(Location);

            _index++;
            while ((_index < _length) && (s_mapKind[_text[_index]] == FullTokenKind.Digit))
                _index++;

            if (_index < _length)
            {
                _c = _text[_index];
                if ((_c == 'e') || (_c == 'E'))
                    ScanFloatExponent();
            }

            _tokenKind = FullTokenKind.FloatValue;
            return;
        }
        else if ((_c == 'e') || (_c == 'E'))
            ScanFloatExponent();
        else
        {
            switch (s_mapKind[_text[_index]])
            {
                case FullTokenKind.Underscore:
                case FullTokenKind.Letter:
                    throw SyntaxException.IntCannotBeFollowed(Location, s_mapKind[_c].ToString());
            }

            _tokenKind = FullTokenKind.IntValue;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanFloatExponent()
    {
        if (++_index == _length)
            throw SyntaxException.UnexpectedEndOfFile(Location);

        _c = _text[_index++];
        if ((s_mapKind[_c] == FullTokenKind.Minus) || (s_mapKind[_c] == FullTokenKind.Plus))
        {
            if (_index == _length)
                throw SyntaxException.UnexpectedEndOfFile(Location);

            _c = _text[_index++];
        }

        if (s_mapKind[_c] != FullTokenKind.Digit)
        {
            _index--;
            throw SyntaxException.ExponentMustHaveDigit(Location);
        }

        while (_index < _length && s_mapKind[_text[_index]] == FullTokenKind.Digit)
            _index++;

        if (_index == _length)
        {
            _tokenKind = FullTokenKind.FloatValue;
            return;
        }

        switch (s_mapKind[_text[_index]])
        {
            case FullTokenKind.Dot:
            case FullTokenKind.Underscore:
            case FullTokenKind.Letter:
                throw SyntaxException.FloatCannotBeFollowed(Location, s_mapKind[_text[_index]].ToString());
        }

        _tokenKind = FullTokenKind.FloatValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanString()
    {
        if (((_index + 1) < _length) && (_text[_index] == '"') && (_text[_index + 1] == '"'))
            ScanBlockString();
        else
            ScanSimpleString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanBlockString()
    {
        _index += 2;
        _tokenIndex = _index;

        // Common indent found across all non-whitespace and non-first line
        var indent = int.MaxValue;

        // Start of the current line being scanned
        var currentLine = _tokenIndex;

        // Is the current line only whitespace characters
        var onlyWhitespace = true;

        // Points to the first and last real lines, a real line has at least 1 non-whitespace character
        var firstRealLine = currentLine;
        var lastRealLine = currentLine;

        // Points to the last non-whitespace character scanned
        var lastRealChar = currentLine;

        // We always start by scanning the first line
        var isFirstLine = true;

        // Are we still looking for the first real line, the first line that is not just whitespace
        var findingFirstReal = true;

        while (_index < _length)
        {
            switch (s_mapKind[_text[_index++]])
            {
                case FullTokenKind.Skip:
                    break;
                case FullTokenKind.NewLine:
                    currentLine = _index;

                    if (findingFirstReal)
                    {
                        if (onlyWhitespace)
                            firstRealLine = currentLine;
                        else
                            findingFirstReal = false;
                    }

                    if (lastRealChar > lastRealLine)
                        lastRealLine = _index - 1;

                    isFirstLine = false;
                    onlyWhitespace = true;
                    break;
                case FullTokenKind.CarriageReturn:
                    // Skip over any optional following newline
                    var lastLine = _index;
                    if ((_index < _length) && (_text[_index] == '\n'))
                        _index++;

                    currentLine = _index;

                    if (findingFirstReal)
                    {
                        if (onlyWhitespace)
                            firstRealLine = currentLine;
                        else
                            findingFirstReal = false;
                    }

                    if (lastRealChar > lastRealLine)
                        lastRealLine = lastLine - 1;

                    isFirstLine = false;
                    onlyWhitespace = true;
                    break;
                case FullTokenKind.DoubleQuote:
                    if (((_index + 1) < _length) && (_text[_index] == '"') && (_text[_index + 1] == '"'))
                    {
                        GenerateStringForBlockString(firstRealLine, lastRealChar, lastRealLine, indent);
                        return;
                    }
                    break;
                default:
                    lastRealChar = _index;
                    if (onlyWhitespace)
                    {
                        if (!isFirstLine)
                            indent = Math.Min(indent, _index - currentLine - 1);

                        onlyWhitespace = false;
                    }
                    break;
            }
        }

        throw SyntaxException.UnexpectedEndOfFile(Location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateStringForBlockString(int firstRealLine, int lastRealChar, int lastRealLine, int indent)
    {
        _sb.Clear();

        // If the three quotes are preceded by a backslash then we want to ignore the backslash
        var endIndex = _index - 1;
        if (_text[endIndex - 1] == '\\')
            endIndex--;

        // If the last real character is within the last line, we need to process the entire last line
        if (lastRealChar > lastRealLine)
            lastRealLine = endIndex;

        // If one of more of the ending lines are empty, we do not need to process them
        if (lastRealLine < endIndex)
            endIndex = lastRealLine;

        var currentLine = firstRealLine;
        var onlyWhitespace = true;

        for (var i = firstRealLine; i < endIndex; i++)
        {
            switch (s_mapKind[_text[i]])
            {
                case FullTokenKind.Skip:
                    break;
                case FullTokenKind.NewLine:
                    AppendLineToBlockString(currentLine, firstRealLine, i, onlyWhitespace, indent);

                    firstRealLine = i + 1;
                    currentLine = firstRealLine;
                    onlyWhitespace = true;
                    break;
                case FullTokenKind.CarriageReturn:
                    AppendLineToBlockString(currentLine, firstRealLine, i, onlyWhitespace, indent);

                    // Skip over any optional following newline
                    if ((i < endIndex) && (_text[i + 1] == '\n'))
                        i++;

                    firstRealLine = i + 1;
                    currentLine = firstRealLine;
                    onlyWhitespace = true;
                    break;
                default:
                    if (onlyWhitespace)
                    {
                        firstRealLine = i;
                        onlyWhitespace = false;
                    }
                    break;
            }
        }

        // Process any remaining line
        if (firstRealLine < endIndex)
            AppendLineToBlockString(currentLine, firstRealLine, endIndex, onlyWhitespace, indent);

        _index += 2;
        _tokenKind = FullTokenKind.StringValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void AppendLineToBlockString(int currentLine, int firstRealLine, int endIndex, bool onlyWhitespace, int indent)
    {
        if (_sb.Length > 0)
            _sb.Append('\n');

        // The first line never has indent applied
        if (currentLine == _tokenIndex)
            _sb.Append(_text[_tokenIndex..endIndex]);
        else if (onlyWhitespace)
        {
            var len = endIndex - currentLine - indent;
            if (len > 0)
                _sb.Append(' ', len);
        }
        else
        {
            var len = firstRealLine - currentLine - indent;
            if (len > 0)
                _sb.Append(' ', len);

            _sb.Append(_text[firstRealLine..endIndex]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScanSimpleString()
    {
        _sb.Clear();

        _tokenIndex = _index;
        var copyIndex = _tokenIndex;

        while (_index < _length)
        {
            _c = _text[_index];
            if (_c == '"')
            {
                if (copyIndex < _index)
                    _sb.Append(_text[copyIndex.._index]);

                _index++;
                _tokenKind = FullTokenKind.StringValue;
                return;
            }
            else if (_c == '\\')
            {
                if (++_index == _length)
                    throw SyntaxException.UnexpectedEndOfFile(Location);

                _c = _text[_index];
                var k = s_escKind[_c];
                switch (k)
                {
                    case EscapeKind.DoubleQuote:
                    case EscapeKind.Backslash:
                    case EscapeKind.Slash:
                    case EscapeKind.Backspace:
                    case EscapeKind.Feed:
                    case EscapeKind.NewLine:
                    case EscapeKind.CarriageReturn:
                    case EscapeKind.Tab:
                        // Copy waiting characters, except the previous '\'
                        if (copyIndex < (_index - 1))
                            _sb.Append(_text[copyIndex..(_index - 1)]);

                        _sb.Append(s_escChar[(int)k]);
                        copyIndex = _index + 1;
                        break;
                    case EscapeKind.u:
                        _index++;
                        if (_index == _length)
                            throw SyntaxException.UnexpectedEndOfFile(Location);

                        _c = _text[_index];
                        if (_c == '{')
                        {
                            // Copy waiting characters, except the previous '\u'
                            if (copyIndex < (_index - 2))
                                _sb.Append(_text[copyIndex..(_index - 2)]);

                            var digits = 0;
                            while (true)
                            {
                                if (++_index == _length)
                                    throw SyntaxException.UnexpectedEndOfFile(Location);

                                _c = _text[_index];
                                var hexToken = s_hexKind[_c];
                                if (hexToken == FullTokenKind.Hexadecimal)
                                    digits++;
                                else
                                {
                                    if (hexToken == FullTokenKind.RightCurlyBracket)
                                    {
                                        if (digits == 0)
                                            throw SyntaxException.EscapeAtLeast1Hex(Location);

                                        try
                                        {
                                            // This format is rare, so just use the builtin conversion because speed does not matter!
                                            var val = uint.Parse(_text.Slice(_index - digits, digits), System.Globalization.NumberStyles.HexNumber);
                                            _sb.Append((char)val);
                                        }
                                        catch
                                        {
                                            throw SyntaxException.EscapeCannotBeConverted(Location, new string(_text.Slice(_index - digits, digits)));
                                        }

                                        copyIndex = _index + 1;
                                        break;
                                    }
                                    else
                                        throw SyntaxException.EscapeOnlyUsingHex(Location);
                                }
                            }
                        }
                        else
                        {
                            if ((_index + 3) >= _length)
                            {
                                _index = _length;
                                throw SyntaxException.UnexpectedEndOfFile(Location);
                            }

                            var hexToken1 = s_hexKind[_text[_index++]];
                            var hexToken2 = s_hexKind[_text[_index++]];
                            var hexToken3 = s_hexKind[_text[_index++]];
                            var hexToken4 = s_hexKind[_text[_index]];
                            if ((hexToken1 != FullTokenKind.Hexadecimal) || (hexToken2 != FullTokenKind.Hexadecimal) ||
                                (hexToken3 != FullTokenKind.Hexadecimal) || (hexToken4 != FullTokenKind.Hexadecimal))
                            {
                                _index++;
                                throw SyntaxException.EscapeOnlyUsingHex(Location);
                            }

                            // Copy waiting characters, except the previous '\uXXXX'
                            if (copyIndex < (_index - 5))
                                _sb.Append(_text[copyIndex..(_index - 5)]);

                            _sb.Append((char)(s_hexValues[_text[_index]] |
                                             (s_hexValues[_text[_index - 1]] << 4) |
                                             (s_hexValues[_text[_index - 2]] << 8) |
                                             (s_hexValues[_text[_index - 3]] << 12)));

                            copyIndex = _index + 1;
                        }
                        break;
                    default:
                        throw SyntaxException.EscapeMustBeOneOf(Location);
                }
            }

            _index++;
        }

        throw SyntaxException.UnexpectedEndOfFile(Location);
    }

    private enum FullTokenKind : byte
    {
        IllegalCharacter = 0,
        Skip = 1,
        NewLine = 2,
        CarriageReturn = 3,
        Hash = 4,
        Exclamation = 5,
        Dollar = 6,
        Ampersand = 7,
        LeftParenthesis = 8,
        RightParenthesis = 9,
        Colon = 10,
        Equals = 11,
        At = 12,
        LeftSquareBracket = 13,
        RightSquareBracket = 14,
        LeftCurlyBracket = 15,
        RightCurlyBracket = 16,
        Vertical = 17,
        Dot = 18,
        Underscore = 19,
        Letter = 20,
        Digit = 21,
        Minus = 22,
        DoubleQuote = 23,
        Plus = 24,
        Hexadecimal = 25,
        StartOfText = 26,
        EndOfText = 27,
        Spread = 28,
        Name = 29,
        IntValue = 30,
        FloatValue = 31,
        StringValue = 32
    }

    private enum EscapeKind : byte
    {
        Other = 0,
        DoubleQuote = 1,
        Backslash = 2,
        Slash = 3,
        Backspace = 4,
        Feed = 5,
        NewLine = 6,
        CarriageReturn = 7,
        Tab = 8,
        u = 9
    }
}

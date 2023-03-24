using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetQL.Parser
{
    public ref struct Tokenizer
    {
        private static Token[] _map = new Token[65536];

        private int _length = 0;
        private int _index = 0;
        private int _lineIndex = 0;
        private int _lineNumber = 1;
        private int _tokenIndex = 0;
        private Token _token = Token.StartOfText;
        private ReadOnlySpan<char> _text;

        static Tokenizer()
        {
            // Skip the Byte Order Mark (BOM)
            _map[0xFEFF] = Token.Skip;

            // Skip simple whitespace
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

            // Names
            _map['_'] = Token.Underscore;
            for (char c = 'A'; c <= 'Z'; c++)
                _map[c] = Token.Letter;
            for (char c = 'a'; c <= 'z'; c++)
                _map[c] = Token.Letter;

            // Names and numbers
            for (char c = '0'; c <= '9'; c++)
                _map[c] = Token.Digit;

            // Numbers
            _map['-'] = Token.Negative;
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
        public int ColumnNumber => _tokenIndex - _lineIndex;

        public Token Next()
        {
            while(_index < _length)
            {
                char c = _text[_index++];
                Token token = _map[c];

                switch (token)
                {
                    case Token.IllegalCharacter:
                        // TEMP
                        break;
                        //throw new ApplicationException("IllegalCharacter");
                    case Token.Skip:
                        break;
                    case Token.NewLine:
                        _lineNumber++;
                        _lineIndex = _index;
                        break;
                    case Token.CarriageReturn:
                        if ((_index < _length) && (_text[_index] == '\n'))
                            _index++;

                        _lineNumber++;
                        _lineIndex = _index;
                        break;
                    case Token.Hash:
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
                        _tokenIndex = _index;
                        return _token = token;
                    case Token.Dot:
                        _tokenIndex = _index;
                        if (((_index + 1) < _length) && (_text[_index] == '.') && (_text[_index + 1] == '.'))
                        {
                            _index += 2;
                            return _token = Token.Spread;
                        }
                        // TEMP
                        break;
                    //throw new ApplicationException($"Less than 3 dots found when only spread allowed");
                    case Token.Underscore:
                    case Token.Letter:
                        _tokenIndex = _index;
                        while (_index < _length)
                        {
                            switch (_map[_text[_index]])
                            {
                                case Token.Underscore:
                                case Token.Letter:
                                case Token.Digit:
                                    _index++;
                                    continue;
                                default:
                                    return _token = Token.Name;
                            }
                        }
                        return _token = Token.Name;
                    case Token.Digit:
                    case Token.Negative:
                        _tokenIndex = _index;
                        break;
                    default:
                        // TEMP
                        break;
                        //throw new ApplicationException($"Unrecognized '{c}'");
                }
            }

            _tokenIndex = _index + 1;
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
        Negative,

        StartOfText,
        EndOfText,
        Spread,
        Name
    }
}

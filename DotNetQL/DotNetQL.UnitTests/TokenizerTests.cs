using DotNetQL.Parser;

namespace DotNetQL.UnitTests
{
    public class TokenizerTests
    {
        [Theory]
        [InlineData("\uFEFF")]
        [InlineData("﻿\uFEFF\uFEFF\uFEFF")]
        public void ByteOrderMark(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.EndOfText, t.Token);
            Assert.Equal(1, t.LineNumber);
            Assert.Equal(text.Length + 1, t.ColumnNumber);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\t\t\t")]
        [InlineData("\t  \t\t  ")]
        public void Whitespace(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.EndOfText, t.Token);
            Assert.Equal(1, t.LineNumber);
            Assert.Equal(text.Length + 1, t.ColumnNumber);
        }

        [Theory]
        [InlineData("\n", 2)]
        [InlineData("\n\n\n", 4)]
        [InlineData("\r", 2)]
        [InlineData("\r\r\r", 4)]
        [InlineData("\r\n", 2)]
        [InlineData("\r\n\r\n\r\n", 4)]
        public void LineTerminators(string text, int lineNumber)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.EndOfText, t.Token);
            Assert.Equal(lineNumber, t.LineNumber);
        }

        [Theory]
        [InlineData("#")]
        [InlineData("# ")]
        [InlineData("#\r")]
        [InlineData("# \n")]
        [InlineData("#你好Ȳ\r\n")]
        [InlineData("###")]
        [InlineData("#\n#\n#")]
        [InlineData("#\n#\n\r# ")]
        public void Comment(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.EndOfText, t.Token);
        }

        [Theory]
        [InlineData(",")]
        [InlineData(",,,")]
        public void Comma(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.EndOfText, t.Token);
            Assert.Equal(1, t.LineNumber);
            Assert.Equal(text.Length + 1, t.ColumnNumber);
        }

        [Theory]
        [InlineData("﻿\uFEFF \t \r\n,")]
        [InlineData("﻿\uFEFF \t \r\n,\uFEFF﻿ \t \r\n,")]
        [InlineData("﻿ ,,, \t\r\t\t\n ,\t,\t,\r\n\t,,,")]
        [InlineData("﻿ ,,, \t\r\t\t#你好Ȳ \n ,\t,\t,\r\n\t,,,")]
        public void Ignored(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("!", Token.Exclamation)]
        [InlineData("$", Token.Dollar)]
        [InlineData("&", Token.Ampersand)]
        [InlineData("(", Token.LeftParenthesis)]
        [InlineData(")", Token.RightParenthesis)]
        [InlineData(":", Token.Colon)]
        [InlineData("=", Token.Equals)]
        [InlineData("@", Token.At)]
        [InlineData("[", Token.LeftSquareBracket)]
        [InlineData("]", Token.RightSquareBracket)]
        [InlineData("{", Token.LeftCurlyBracket)]
        [InlineData("}", Token.RightCurlyBracket)]
        [InlineData("|", Token.Vertical)]
        [InlineData("...", Token.Spread)]
        public void Punctuators(string text, Token token)
        {
            var t = new Tokenizer(text);
            Assert.Equal(token, t.Token);
            Assert.Equal(text, t.TokenString);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
            Assert.Equal(1, t.LineNumber);
            Assert.Equal(text.Length + 1, t.ColumnNumber);
        }

        [Theory]
        [InlineData("!$&", Token.Exclamation, Token.Dollar, Token.Ampersand)]
        [InlineData("! $ &", Token.Exclamation, Token.Dollar, Token.Ampersand)]
        [InlineData("\uFEFF\t!,...   &\r\n", Token.Exclamation, Token.Spread, Token.Ampersand)]
        [InlineData("...... \t,\t...", Token.Spread, Token.Spread, Token.Spread)]
        public void PunctuatorsAndWhitespace(string text, Token token1, Token token2, Token token3)
        {
            var t = new Tokenizer(text);
            Assert.Equal(token1, t.Token);
            t.Next();
            Assert.Equal(token2, t.Token);
            t.Next();
            Assert.Equal(token3, t.Token);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("A")]
        [InlineData("AA")]
        [InlineData("AAAA")]
        [InlineData("a")]
        [InlineData("aaaa")]
        [InlineData("aaaaaaaa")]
        [InlineData("aAzZ")]
        [InlineData("_")]
        [InlineData("_____")]
        [InlineData("_1A2b_3_d_eezZ")]
        public void Name(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.Name, t.Token);
            Assert.Equal(text, t.TokenString);
            Assert.Equal(1, t.ColumnNumber);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
            Assert.Equal(text.Length + 1, t.ColumnNumber);
        }

        [Theory]
        [InlineData("World#Hello")]
        [InlineData("# Hello 你好Ȳ\nWorld\t# fish")]
        [InlineData("#Fish#你好Ȳ\rWorld")]
        [InlineData("#Fish#你好Ȳ\rWorld\n")]
        public void NameAndWhitespace(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.Name, t.Token);
            Assert.Equal("World", t.TokenString);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("0", 0, Token.EndOfText)]
        [InlineData("9", 9, Token.EndOfText)]
        [InlineData("12345", 12345, Token.EndOfText)]
        [InlineData("9876543", 9876543, Token.EndOfText)]
        [InlineData("-0", -0, Token.EndOfText)]
        [InlineData("-9", -9, Token.EndOfText)]
        [InlineData("-12345", -12345, Token.EndOfText)]
        [InlineData("-9876543", -9876543, Token.EndOfText)]
        [InlineData("0#", 0, Token.EndOfText)]
        [InlineData("9\t", 9, Token.EndOfText)]
        [InlineData("12345\r\n", 12345, Token.EndOfText)]
        [InlineData("9876543,", 9876543, Token.EndOfText)]
        [InlineData("-0{", -0, Token.LeftCurlyBracket)]
        [InlineData("-9876543\uFEFF", -9876543, Token.EndOfText)]
        public void IntValue(string text, int val, Token nextToken)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.IntValue, t.Token);
            Assert.Equal(val, int.Parse(t.TokenString));
            t.Next();
            Assert.Equal(nextToken, t.Token);
        }

        [Theory]
        [InlineData("0.0", 0.0, Token.EndOfText)]
        [InlineData("9.1", 9.1, Token.EndOfText)]
        [InlineData("12345.0123", 12345.0123, Token.EndOfText)]
        [InlineData("9876543.987", 9876543.987, Token.EndOfText)]
        [InlineData("-0.0", -0.0, Token.EndOfText)]
        [InlineData("-9.1", -9.1, Token.EndOfText)]
        [InlineData("-12345.0123", -12345.0123, Token.EndOfText)]
        [InlineData("-9876543.987", -9876543.987, Token.EndOfText)]
        [InlineData("1e0", 1e0, Token.EndOfText)]
        [InlineData("2e123", 2e123, Token.EndOfText)]
        [InlineData("1e+2", 1e+2, Token.EndOfText)]
        [InlineData("2e+33", 2e+33, Token.EndOfText)]
        [InlineData("1e-2", 1e-2, Token.EndOfText)]
        [InlineData("2e-33", 2e-33, Token.EndOfText)]
        [InlineData("1.1e0", 1.1e0, Token.EndOfText)]
        [InlineData("2.2e123", 2.2e123, Token.EndOfText)]
        [InlineData("1.1e+2", 1.1e+2, Token.EndOfText)]
        [InlineData("2.2e+33", 2.2e+33, Token.EndOfText)]
        [InlineData("1.1e-2", 1.1e-2, Token.EndOfText)]
        [InlineData("2.2e-33", 2.2e-33, Token.EndOfText)]
        [InlineData("0.0 ", 0.0, Token.EndOfText)]
        [InlineData("9.1#", 9.1, Token.EndOfText)]
        [InlineData("12345.0123\r\n", 12345.0123, Token.EndOfText)]
        [InlineData("9876543.987{", 9876543.987, Token.LeftCurlyBracket)]
        [InlineData("-0.0﻿\uFEFF", -0.0, Token.EndOfText)]
        public void FloatValue(string text, double val, Token nextToken)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.FloatValue, t.Token);
            Assert.Equal(val, double.Parse(t.TokenString));
            t.Next();
            Assert.Equal(nextToken, t.Token);
        }

        [Theory]
        [InlineData("  \t42#fish\n\t 3.14#fish", 42, 3.14)]
        [InlineData("7   \r\n3.14e-2    \n\r", 7, 3.14e-2)]
        public void NumbersAndWhitespace(string text, int val1, double val2)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.IntValue, t.Token);
            Assert.Equal(val1, int.Parse(t.TokenString));
            t.Next();
            Assert.Equal(Token.FloatValue, t.Token);
            Assert.Equal(val2, double.Parse(t.TokenString));
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("\"\"")]
        [InlineData("\"你好Ȳ\"")]
        [InlineData("\"abc\"")]
        [InlineData("\"\\\"\"")]
        [InlineData("\"\\\\\"")]
        [InlineData("\"\\/\"")]
        [InlineData("\"\\b\"")]
        [InlineData("\"\\f\"")]
        [InlineData("\"\\n\"")]
        [InlineData("\"\\r\"")]
        [InlineData("\"\\u0123\"")]
        [InlineData("\"\\u4567\"")]
        [InlineData("\"\\u7890\"")]
        [InlineData("\"\\uABCD\"")]
        [InlineData("\"\\uCDEF\"")]
        [InlineData("\"\\u{0}\"")]
        [InlineData("\"\\u{99}\"")]
        [InlineData("\"\\u{AAA}\"")]
        [InlineData("\"\\u{FFFF}\"")]
        [InlineData("\"\\u{123456789ABCDEF}\"")]
        public void String(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.StringValue, t.Token);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("\"\"!", Token.Exclamation)]
        [InlineData("\"\" $", Token.Dollar)]
        [InlineData("\"abc\"42", Token.IntValue)]
        [InlineData("\"abc\" ,3.14", Token.FloatValue)]
        [InlineData("\"abc\" \"def\"", Token.StringValue)]
        [InlineData("\"abc\"\"def\"", Token.StringValue)]
        public void StringAndSecondToken(string text, Token token)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.StringValue, t.Token);
            t.Next();
            Assert.Equal(token, t.Token);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("\"\"\"\"\"\"")]
        [InlineData("\"\"\"你好Ȳ\"\"\"")]
        [InlineData("\"\"\"abc\"\"\"")]
        [InlineData("\"\"\"a\"b\"c\"\"\"")]
        [InlineData("\"\"\"a\"\"b\"\"c\"\"\"")]
        [InlineData("\"\"\"abc\n\"\"\"")]
        [InlineData("\"\"\"abc\r\ndef\"\"\"")]
        [InlineData("\"\"\"abc#fish\r\n\"\"\"")]
        [InlineData("\"\"\"$ab!c#fish\r\n3.14 _fish\"\"\"")]
        public void BlockString(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.StringValue, t.Token);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("\"\"\"\"\"\"!", Token.Exclamation)]
        [InlineData("\"\"\"abc\"\"\" $", Token.Dollar)]
        [InlineData("\"\"\"a\"b\"c\"\"\"42", Token.IntValue)]
        [InlineData("\"\"\"a\"\"b\"\"c\"\"\" ,3.14", Token.FloatValue)]
        [InlineData("\"\"\"abc\n\"\"\" \"def\"", Token.StringValue)]
        [InlineData("\"\"\"abc\r\ndef\"\"\"\"def\"", Token.StringValue)]
        [InlineData("\"\"\"abc\"\"\" \"\"\"abc\"\"\"", Token.StringValue)]
        [InlineData("\"\"\"abc\"\"\"\"\"\"abc\"\"\"", Token.StringValue)]
        public void BlockStringAndSecondToken(string text, Token token)
        {
            var t = new Tokenizer(text);
            Assert.Equal(Token.StringValue, t.Token);
            t.Next();
            Assert.Equal(token, t.Token);
            t.Next();
            Assert.Equal(Token.EndOfText, t.Token);
        }
    }
}
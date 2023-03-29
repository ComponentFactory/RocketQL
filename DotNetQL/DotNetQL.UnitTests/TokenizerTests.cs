using DotNetQL.Parser;

namespace DotNetQL.UnitTests
{
    public class TokenizerTests
    {
        [Fact]
        public void NullText()
        {
            var t = new Tokenizer(null);
            Assert.Equal(TokenKind.EndOfText, t.Token);
            Assert.Equal(1, t.LineNumber);
            Assert.Equal(1, t.ColumnNumber);

        }
        [Theory]
        [InlineData("\uFEFF")]
        [InlineData("﻿\uFEFF\uFEFF\uFEFF")]
        public void ByteOrderMark(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
            Assert.Equal(1, t.LineNumber);
            Assert.Equal(text.Length + 1, t.ColumnNumber);
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\t\t\t")]
        [InlineData("\t  \t\t  ")]
        public void Whitespace(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
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
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
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
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
        }

        [Theory]
        [InlineData(",")]
        [InlineData(",,,")]
        public void Comma(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
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
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("!", TokenKind.Exclamation)]
        [InlineData("$", TokenKind.Dollar)]
        [InlineData("&", TokenKind.Ampersand)]
        [InlineData("(", TokenKind.LeftParenthesis)]
        [InlineData(")", TokenKind.RightParenthesis)]
        [InlineData(":", TokenKind.Colon)]
        [InlineData("=", TokenKind.Equals)]
        [InlineData("@", TokenKind.At)]
        [InlineData("[", TokenKind.LeftSquareBracket)]
        [InlineData("]", TokenKind.RightSquareBracket)]
        [InlineData("{", TokenKind.LeftCurlyBracket)]
        [InlineData("}", TokenKind.RightCurlyBracket)]
        [InlineData("|", TokenKind.Vertical)]
        [InlineData("...", TokenKind.Spread)]
        public void Punctuators(string text, TokenKind token)
        {
            var t = new Tokenizer(text);
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(token, t.Token);
            Assert.Equal(text, t.TokenValue);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
            Assert.Equal(1, t.LineNumber);
            Assert.Equal(text.Length + 1, t.ColumnNumber);
        }

        [Theory]
        [InlineData("!$&", TokenKind.Exclamation, TokenKind.Dollar, TokenKind.Ampersand)]
        [InlineData("! $ &", TokenKind.Exclamation, TokenKind.Dollar, TokenKind.Ampersand)]
        [InlineData("\uFEFF\t!,...   &\r\n", TokenKind.Exclamation, TokenKind.Spread, TokenKind.Ampersand)]
        [InlineData("...... \t,\t...", TokenKind.Spread, TokenKind.Spread, TokenKind.Spread)]
        public void PunctuatorsAndWhitespace(string text, TokenKind token1, TokenKind token2, TokenKind token3)
        {
            var t = new Tokenizer(text);
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(token1, t.Token);
            t.Next();
            Assert.Equal(token2, t.Token);
            t.Next();
            Assert.Equal(token3, t.Token);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
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
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.Name, t.Token);
            Assert.Equal(text, t.TokenValue);
            Assert.Equal(1, t.ColumnNumber);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
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
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.Name, t.Token);
            Assert.Equal("World", t.TokenValue);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("0", 0, TokenKind.EndOfText)]
        [InlineData("9", 9, TokenKind.EndOfText)]
        [InlineData("12345", 12345, TokenKind.EndOfText)]
        [InlineData("9876543", 9876543, TokenKind.EndOfText)]
        [InlineData("-0", -0, TokenKind.EndOfText)]
        [InlineData("-9", -9, TokenKind.EndOfText)]
        [InlineData("-12345", -12345, TokenKind.EndOfText)]
        [InlineData("-9876543", -9876543, TokenKind.EndOfText)]
        [InlineData("0 ", 0, TokenKind.EndOfText)]
        [InlineData("0#", 0, TokenKind.EndOfText)]
        [InlineData("9\t", 9, TokenKind.EndOfText)]
        [InlineData("12345\r\n", 12345, TokenKind.EndOfText)]
        [InlineData("9876543,", 9876543, TokenKind.EndOfText)]
        [InlineData("-0{", -0, TokenKind.LeftCurlyBracket)]
        [InlineData("-0 {", -0, TokenKind.LeftCurlyBracket)]
        [InlineData("-9876543\uFEFF", -9876543, TokenKind.EndOfText)]
        public void IntValue(string text, int val, TokenKind nextToken)
        {
            var t = new Tokenizer(text);
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.IntValue, t.Token);
            Assert.Equal(val, int.Parse(t.TokenValue));
            t.Next();
            Assert.Equal(nextToken, t.Token);
        }

        [Theory]
        [InlineData("0.0", 0.0, TokenKind.EndOfText)]
        [InlineData("9.1", 9.1, TokenKind.EndOfText)]
        [InlineData("12345.0123", 12345.0123, TokenKind.EndOfText)]
        [InlineData("9876543.987", 9876543.987, TokenKind.EndOfText)]
        [InlineData("-0.0", -0.0, TokenKind.EndOfText)]
        [InlineData("-9.1", -9.1, TokenKind.EndOfText)]
        [InlineData("-12345.0123", -12345.0123, TokenKind.EndOfText)]
        [InlineData("-9876543.987", -9876543.987, TokenKind.EndOfText)]
        [InlineData("1e0", 1e0, TokenKind.EndOfText)]
        [InlineData("2e123", 2e123, TokenKind.EndOfText)]
        [InlineData("1e+2", 1e+2, TokenKind.EndOfText)]
        [InlineData("2e+33", 2e+33, TokenKind.EndOfText)]
        [InlineData("1e-2", 1e-2, TokenKind.EndOfText)]
        [InlineData("2e-33", 2e-33, TokenKind.EndOfText)]
        [InlineData("1.1e0", 1.1e0, TokenKind.EndOfText)]
        [InlineData("2.2e123", 2.2e123, TokenKind.EndOfText)]
        [InlineData("1.1e+2", 1.1e+2, TokenKind.EndOfText)]
        [InlineData("2.2e+33", 2.2e+33, TokenKind.EndOfText)]
        [InlineData("1.1e-2", 1.1e-2, TokenKind.EndOfText)]
        [InlineData("2.2e-33", 2.2e-33, TokenKind.EndOfText)]
        [InlineData("0.0 ", 0.0, TokenKind.EndOfText)]
        [InlineData("9.1#", 9.1, TokenKind.EndOfText)]
        [InlineData("12345.0123\r\n", 12345.0123, TokenKind.EndOfText)]
        [InlineData("9876543.987{", 9876543.987, TokenKind.LeftCurlyBracket)]
        [InlineData("-0.0﻿\uFEFF", -0.0, TokenKind.EndOfText)]
        public void FloatValue(string text, double val, TokenKind nextToken)
        {
            var t = new Tokenizer(text);
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.FloatValue, t.Token);
            Assert.Equal(val, double.Parse(t.TokenValue));
            t.Next();
            Assert.Equal(nextToken, t.Token);
        }

        [Theory]
        [InlineData("  \t42#fish\n\t 3.14#fish", 42, 3.14)]
        [InlineData("7   \r\n3.14e-2    \n\r", 7, 3.14e-2)]
        public void NumbersAndWhitespace(string text, int val1, double val2)
        {
            var t = new Tokenizer(text);
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.IntValue, t.Token);
            Assert.Equal(val1, int.Parse(t.TokenValue));
            t.Next();
            Assert.Equal(TokenKind.FloatValue, t.Token);
            Assert.Equal(val2, double.Parse(t.TokenValue));
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
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
        public void SimpleStringOnly(string text)
        {
            var t = new Tokenizer(text);
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.StringValue, t.Token);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("\"\"!", TokenKind.Exclamation)]
        [InlineData("\"\" $", TokenKind.Dollar)]
        [InlineData("\"abc\"42", TokenKind.IntValue)]
        [InlineData("\"abc\" ,3.14", TokenKind.FloatValue)]
        [InlineData("\"abc\" \"def\"", TokenKind.StringValue)]
        [InlineData("\"abc\"\"def\"", TokenKind.StringValue)]
        public void SimpleStringAndSecondToken(string text, TokenKind token)
        {
            var t = new Tokenizer(text);
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.StringValue, t.Token);
            t.Next();
            Assert.Equal(token, t.Token);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("\"\"\"\"\"\"", "")]
        [InlineData("\"\"\"a\"\"\"", "a")]
        [InlineData("\"\"\"\na\"\"\"", "a")]
        [InlineData("\"\"\"   \na\"\"\"", "a")]
        [InlineData("\"\"\"   \n  \n\na\"\"\"", "a")]
        [InlineData("\"\"\"abc\"\"\"", "abc")]
        [InlineData("\"\"\"\nabc\"\"\"", "abc")]
        [InlineData("\"\"\"   \nabc\"\"\"", "abc")]
        [InlineData("\"\"\"   \n  \n\nabc\"\"\"", "abc")]
        [InlineData("\"\"\"a\n\"\"\"", "a")]
        [InlineData("\"\"\"a\n  \"\"\"", "a")]
        [InlineData("\"\"\"a   \n\"\"\"", "a   ")]
        [InlineData("\"\"\"a   \n\n\n\"\"\"", "a   ")]
        [InlineData("\"\"\"a   \n  \n \n\"\"\"", "a   ")]
        [InlineData("\"\"\"abc\n\"\"\"", "abc")]
        [InlineData("\"\"\"abc\n  \"\"\"", "abc")]
        [InlineData("\"\"\"abc   \n\"\"\"", "abc   ")]
        [InlineData("\"\"\"abc   \n\n\n\"\"\"", "abc   ")]
        [InlineData("\"\"\"abc   \n  \n \n\"\"\"", "abc   ")]
        [InlineData("\"\"\"a\nbc\"\"\"", "a\nbc")]
        [InlineData("\"\"\"ab\nc\"\"\"", "ab\nc")]
        [InlineData("\"\"\"ab\ncd\nef\"\"\"", "ab\ncd\nef")]
        [InlineData("\"\"\"ab \ncd  \nef   \"\"\"", "ab \ncd  \nef   ")]
        [InlineData("\"\"\"   a\"\"\"", "   a")]
        [InlineData("\"\"\"  a\n  b\n  c\"\"\"", "  a\nb\nc")]
        [InlineData("\"\"\"  a\n    b\n   c\"\"\"", "  a\n b\nc")]
        [InlineData("\"\"\"  a\n      \n  c\"\"\"", "  a\n    \nc")]
        [InlineData("\"\"\" a\n  b\n  c\"\"\"", " a\nb\nc")]
        [InlineData("\"\"\"    a\n  b\n    c\"\"\"", "    a\nb\n  c")]
        public void BlockStringOnly(string text, string contents)
        {
            var t = new Tokenizer(text);
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.StringValue, t.Token);
            Assert.Equal(contents, t.TokenString);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
        }

        [Theory]
        [InlineData("\"\"\"\"\"\"!", TokenKind.Exclamation)]
        [InlineData("\"\"\"abc\"\"\" $", TokenKind.Dollar)]
        [InlineData("\"\"\"a\"b\"c\"\"\"42", TokenKind.IntValue)]
        [InlineData("\"\"\"a\"\"b\"\"c\"\"\" ,3.14", TokenKind.FloatValue)]
        [InlineData("\"\"\"abc\n\"\"\" \"def\"", TokenKind.StringValue)]
        [InlineData("\"\"\"abc\r\ndef\"\"\"\"def\"", TokenKind.StringValue)]
        [InlineData("\"\"\"  a\n    b\n   c\"\"\" ,3.14", TokenKind.FloatValue)]
        public void BlockStringAndSecondToken(string text, TokenKind token)
        {
            var t = new Tokenizer(text);
            Assert.Equal(TokenKind.StartOfText, t.Token);
            t.Next();
            Assert.Equal(TokenKind.StringValue, t.Token);
            t.Next();
            Assert.Equal(token, t.Token);
            t.Next();
            Assert.Equal(TokenKind.EndOfText, t.Token);
        }
    }
}
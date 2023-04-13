namespace RocketQL.Core;

public enum GraphQLTokenKind : byte
{
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
    StartOfText = 26,
    EndOfText = 27,
    Spread = 28,
    Name = 29,
    IntValue = 30,
    FloatValue = 31,
    StringValue = 32
}

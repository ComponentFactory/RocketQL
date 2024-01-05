namespace RocketQL.Core.Exceptions;

public class SyntaxException(Location location, string message) : RocketException(location, message)
{
    public static SyntaxException UnexpectedEndOfFile(Location location) => new(location, "Unexpected end of file encountered.");
    public static SyntaxException UnrecognizedCharacterCode(Location location, char c) => new(location, $"Unrecognized character code '{(int)c}' found.");
    public static SyntaxException UnrecognizedToken(Location location, string tokenKind) => new(location, $"Unrecognized token '{tokenKind}'.");
    public static SyntaxException UnrecognizedKeyword(Location location, string keyword) => new(location, $"Unrecognized keyword '{keyword}'.");
    public static SyntaxException UnrecognizedOperationType(Location location, string keyword) => new(location, $"Unrecognized operator type '{keyword}', must be one of 'query', 'mutation', 'subscription'.");
    public static SyntaxException IllegalCharacterCode(Location location, char c) => new(location, $"Illegal character code '{(int)c}' for this location.");
    public static SyntaxException SpreadNeedsThreeDots(Location location) => new(location, "Spread operator requires 3 dots in sequence.");
    public static SyntaxException MinusMustBeFollowedByDigit(Location location) => new(location, "Minus sign must be followed by a digit.");
    public static SyntaxException PointMustBeFollowedByDigit(Location location) => new(location, "Decimal point must be followed by a digit.");
    public static SyntaxException ExponentMustHaveDigit(Location location) => new(location, "Exponent must have at least one digit.");
    public static SyntaxException FloatCannotBeFollowed(Location location, string param) => new(location, $"Floating point value cannot be followed by a {param.ToLower()}.");
    public static SyntaxException IntCannotBeFollowed(Location location, string param) => new(location, $"Integer value cannot be followed by a {param.ToLower()}.");
    public static SyntaxException EscapeAtLeast1Hex(Location location) => new(location, "Escaped character must have at least 1 hexadecimal value.");
    public static SyntaxException EscapeOnlyUsingHex(Location location) => new(location, "Escaped character must be specificed only using hexadecimal values.");
    public static SyntaxException EscapeCannotBeConverted(Location location, string param) => new(location, $"Cannot escape characters using hexidecimal value '{param}'.");
    public static SyntaxException EscapeMustBeOneOf(Location location) => new(location, "Escaped character is not one of \\\" \\\\ \\/ \\b \\f \\n \\r \\t.");
    public static SyntaxException ExpectedTokenNotFound(Location location, string expected, string found) => new(location, $"Expected token '{expected}' but found '{found}' instead.");
    public static SyntaxException ExpectedKeywordNotFound(Location location, string expected, string found) => new(location, $"Expected keyword '{expected}' but found '{found}' instead.");
    public static SyntaxException ExpectedDirectiveLocationNotFound(Location location, string found) => new(location, $"Expected directive location but found '{found}' instead.");
    public static SyntaxException TypeMustBeNameOrList(Location location, string found) => new(location, $"Type must be a name or '[' indicating a list but found token '{found}' instead.");
    public static SyntaxException TokenNotAllowedHere(Location location, string found) => new(location, $"Token '{found}' not allowed in this position.");
    public static SyntaxException SelectionSetInvalidToken(Location location, string found) => new(location, $"Found token '{found}' instead of either a name or spread operator inside the selection set.");
    public static SyntaxException FragmentNameCannotBeOn(Location location) => new(location, "Fragment name cannot be the keyword 'on'.");
    public static SyntaxException ExtendSchemaMissingAtLeastOne(Location location) => new(location, "Extend scheme must specify at least one directive or operation.");
    public static SyntaxException ExtendObjectTypeMissingAtLeastOne(Location location) => new(location, "Extend object must specify at least one of interface, directive or field.");
    public static SyntaxException ExtendInterfaceTypeMissingAtLeastOne(Location location) => new(location, "Extend interface must specify at least one of interface, directive or field.");
    public static SyntaxException ExtendUnionTypeMissingAtLeastOne(Location location) => new(location, "Extend union must specify at least one of directive or member type.");
    public static SyntaxException ExtendEnumTypeMissingAtLeastOne(Location location) => new(location, "Extend enum must specify at least one of directive or enum value.");
    public static SyntaxException ExtendInputObjectTypeMissingAtLeastOne(Location location) => new(location, "Extend input object must specify at least one of directive or field.");
    public static SyntaxException QueryNotAllowedInSchema(Location location) => new(location, "Schema document cannot contain query.");
    public static SyntaxException UnnamedQueryNotAllowedInSchema(Location location) => new(location,$"Schema document cannot contain unnamed query operation.");
    public static SyntaxException DefinintionNotAllowedInOperation(Location location, string definition) => new(location, $"Operation document cannot contain definition '{definition}'.");
    public static SyntaxException ExtendDefinintionNotAllowedInOperation(Location location, string definition) => new(location, $"Operation document cannot contain extend definition '{definition}'.");
}

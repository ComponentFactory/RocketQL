namespace RocketQL.Core.UnitTests.GraphQLTokenizer;

public class Files
{
    [Theory]
    [InlineData("github.graphql")]
    [InlineData("introspection.graphql")]
    public void FileSchema(string filename)
    {
        var schema = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var t = new Core.GraphQLTokenizer(schema.AsSpan());
        var s = string.Empty;
        while (t.Next())
        {
            switch (t.TokenKind)
            {
                case GraphQLTokenKind.Name:
                case GraphQLTokenKind.IntValue:
                case GraphQLTokenKind.FloatValue:
                case GraphQLTokenKind.Spread:
                    s = t.TokenValue;
                    break;
                case GraphQLTokenKind.StringValue:
                    s = t.TokenString;
                    break;
                default:
                    break;
            }
        }
    }
}


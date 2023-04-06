namespace RocketQL.Core.UnitTests.Tokenizer;

public class Schema
{
    [Theory]
    [InlineData("github.graphql")]
    public void GithubSchema(string filename)
    {
        var schema = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var t = new Core.Tokenizer(schema.AsSpan());
        var s = string.Empty;
        while (t.Next())
        {
            switch (t.TokenKind)
            {
                case TokenKind.Name:
                case TokenKind.IntValue:
                case TokenKind.FloatValue:
                case TokenKind.Spread:
                    s = t.TokenValue;
                    break;
                case TokenKind.StringValue:
                    s = t.TokenString;
                    break;
                default:
                    break;
            }
        }
    }
}


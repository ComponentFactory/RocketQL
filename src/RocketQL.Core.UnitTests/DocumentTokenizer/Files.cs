namespace RocketQL.Core.UnitTests.DocumentTokenizerTests;

public class Files
{
    [Theory]
    [InlineData("github.graphql")]
    [InlineData("introspection.graphql")]
    public void FileSchema(string filename)
    {
        var schema = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var t = new DocumentTokenizer("test", schema.AsSpan());
        var s = string.Empty;
        while (t.Next())
        {
            switch (t.TokenKind)
            {
                case DocumentTokenKind.Name:
                case DocumentTokenKind.IntValue:
                case DocumentTokenKind.FloatValue:
                case DocumentTokenKind.Spread:
                    s = t.TokenValue;
                    break;
                case DocumentTokenKind.StringValue:
                    s = t.TokenString;
                    break;
                default:
                    break;
            }
        }
    }
}


namespace RocketQL.Core.UnitTests.DocumentTokenizerTests;

public class Files : UnitTestBase
{
    [Theory]
    [InlineData("github-schema.graphql")]
    [InlineData("introspection-query.graphql")]
    [InlineData("kitchensink-schema.graphql")]
    public void FileSchema(string filename)
    {
        var schema = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var t = new DocumentTokenizer(schema.AsSpan(), "test");
        var s = "";
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


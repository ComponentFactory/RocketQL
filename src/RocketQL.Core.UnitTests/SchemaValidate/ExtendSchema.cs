namespace RocketQL.Core.UnitTests.SchemaValidation;

public class ExtendSchema : UnitTestBase
{
    [Theory]
    [InlineData("""
                extend schema { query: fizz }
                type Query { alpha: Int }
                """,
                "Extend schema cannot be applied because no schema has been defined.",
                "extend schema")]
    [InlineData("""
                extend schema { query: fizz }
                schema { query: fizz }
                type fizz { alpha: Int }
                """,
                "Extend schema cannot be applied because no schema has been defined.",
                "extend schema")]
    [InlineData("""
                directive @bar on SCHEMA
                type fizz { buzz: Int }
                schema @bar { query: fizz } 
                extend schema @bar
                """,
                "Directive '@bar' is not repeatable but has been applied multiple times.",
                "schema, directive @bar")]
    [InlineData("""
                directive @bar on SCHEMA
                type fizz { buzz: Int }
                schema { query: fizz } 
                extend schema { query: fizz } 
                """,
                "Extend schema cannot add query operation because it is already defined.",
                "extend schema, query fizz")]
    public void ValidationSingleExceptions(string schemaText, string message, string commaPath)
    {
        SchemaValidationSingleException(schemaText, message, commaPath);
    }

    [Fact]
    public void AddDirectiveToSchema()
    {
        var schema = SchemaFromString(
            """
            directive @bar on SCHEMA
            type Query { fizz: Int }
            type fizz { buzz: Int }
            schema { query: fizz } 
            extend schema @bar               
            """);

        var foo = schema.Root;
        Assert.NotNull(foo);
        var directive = foo.Directives[0];
        Assert.NotNull(directive);
        Assert.Equal("@bar", directive.Name);
    }

    [Fact]
    public void AddOperation()
    {
        var schema = SchemaFromString(
            """
            type Query { fizz: Int }
            type fizz { buzz: Int }
            type foo { bar: Int }
            schema { query: fizz } 
            extend schema { mutation: foo }        
            """);

        var foo = schema.Root;
        Assert.NotNull(foo);
        Assert.NotNull(foo.Query);
        Assert.NotNull(foo.Mutation);
    }
}


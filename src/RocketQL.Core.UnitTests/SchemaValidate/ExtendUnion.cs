namespace RocketQL.Core.UnitTests.SchemaValidation;

public class ExtendUnion : UnitTestBase
{
    [Theory]
    [InlineData("""
                type Query { alpha: Int }
                extend union foo = bar
                """,
                "Union 'foo' cannot be extended because it is not defined.",
                "extend union foo")]
    [InlineData("""
                type Query { alpha: Int }
                extend union foo = bar
                union foo
                """,
                "Union 'foo' cannot be extended because it is not defined.",
                "extend union foo")]
    [InlineData("""
                type Query { alpha: Int }
                directive @bar on UNION
                union foo @bar
                extend union foo @bar
                """,
                "Directive '@bar' is not repeatable but has been applied multiple times.",
                "union foo, directive @bar")]
    [InlineData("""
                type Query { alpha: Int }
                type bar { buzz: Int }
                union foo = bar
                extend union foo = bar
                """,
                "Extend union 'foo' specifies a member type 'bar' already defined.",
                "extend union foo, member type bar")]
    public void ValidationSingleExceptions(string schemaText, string message, string commaPath)
    {
        SchemaValidationSingleException(schemaText, message, commaPath);
    }

    [Fact]
    public void AddDirectiveToUnion()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   directive @bar on UNION
                   union foo
                   extend union foo @bar                
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as UnionTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        var directive = foo.Directives[0];
        Assert.NotNull(directive);
        Assert.Equal("@bar", directive.Name);
    }

    [Fact]
    public void AddType()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   type bar { buzz: Int }
                   union foo
                   extend union foo = bar          
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as UnionTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        var bar = foo.MemberTypes["bar"];
        Assert.NotNull(bar);
        Assert.Equal("bar", bar.Name);
    }
}


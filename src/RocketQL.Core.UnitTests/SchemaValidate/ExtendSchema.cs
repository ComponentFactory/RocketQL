﻿namespace RocketQL.Core.UnitTests.SchemaValidation;

public class ExtendSchema : UnitTestBase
{
    [Theory]
    [InlineData("""
                extend schema { query: fizz }
                type Query { alpha: Int }
                """,                                            "Extend schema cannot be applied because no schema has been defined.")]
    [InlineData("""
                extend schema { query: fizz }
                schema { query: fizz }
                type fizz { alpha: Int }
                """,                                            "Extend schema cannot be applied because no schema has been defined.")]
    [InlineData("""
                directive @bar on SCHEMA
                type fizz { buzz: Int }
                schema @bar { query: fizz } 
                extend schema @bar
                """,                                            "Directive '@bar' is not repeatable but has been applied multiple times on schema.")]
    [InlineData("""
                directive @bar on SCHEMA
                type fizz { buzz: Int }
                schema { query: fizz } 
                extend schema { query: fizz } 
                """,                                            "Extend schema cannot add operation QUERY because it is already defined.")]
    public void ValidationSingleExceptions(string schemaText, string message)
    {
        SchemaValidationSingleException(schemaText, message);
    }

    [Fact]
    public void AddDirectiveToSchema()
    {
        var schema = new Schema();
        schema.Add("""
                   directive @bar on SCHEMA
                   type Query { fizz: Int }
                   type fizz { buzz: Int }
                   schema { query: fizz } 
                   extend schema @bar               
                   """);
        schema.Validate();

        var foo = schema.Root;
        Assert.NotNull(foo);
        var directive = foo.Directives[0];
        Assert.NotNull(directive);
        Assert.Equal("@bar", directive.Name);
    }

    [Fact]
    public void AddOperation()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   type fizz { buzz: Int }
                   type foo { bar: Int }
                   schema { query: fizz } 
                   extend schema { mutation: foo }        
                   """);
        schema.Validate();

        var foo = schema.Root;
        Assert.NotNull(foo);
        Assert.NotNull(foo.Query);
        Assert.NotNull(foo.Mutation);
    }
}

namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Schemas : UnitTestBase
{
    [Theory]
    // Mandatory query operation
    [InlineData("""
                type foo { fizz: Int }
                schema { mutation: foo }
                """,
                "Schema definition missing mandatory query operation.",
                "schema")]
    // Cannot define same operation more than once
    [InlineData("""
                type foo { fizz: Int }
                schema 
                { 
                    query: foo
                    query: foo
                }            
                """,
                "Schema defines the query operation more than once.",
                "schema, query foo")]
    // Check operation types
    [InlineData("""
                schema { query: foo }
                """,
                "Schema query operation type 'foo' not defined.",
                "schema, query foo")]
    [InlineData("""
                scalar foo
                schema { query: foo }
                """,
                "Schema query operation 'foo' has type scalar instead of object type.",
                "schema, query foo")]
    [InlineData("""
                type bar { fizz: Int }
                scalar foo
                schema 
                { 
                    query: bar 
                    mutation: foo 
                }
                """,
                "Schema mutation operation 'foo' has type scalar instead of object type.",
                "schema, mutation foo")]
    [InlineData("""
                type bar { fizz: Int }
                scalar foo
                schema 
                { 
                    query: bar 
                    subscription: foo 
                }
                """,
                "Schema subscription operation 'foo' has type scalar instead of object type.",
                "schema, subscription foo")]
    [InlineData("""
                type foo { fizz: Int }
                schema 
                { 
                    query: foo 
                    mutation: foo 
                }
                """,
                "Schema operations query and mutation cannot have the same 'foo' type.",
                "schema")]
    [InlineData("""
                type foo { fizz: Int }
                schema 
                { 
                    query: foo 
                    subscription: foo 
                }
                """,
                "Schema operations query and subscription cannot have the same 'foo' type.",
                "schema")]
    [InlineData("""
                type bar { fizz: Int }
                type foo { fizz: Int }
                schema 
                { 
                    query: bar 
                    mutation: foo 
                    subscription: foo 
                }
                """,
                "Schema operations mutation and subscription cannot have the same 'foo' type.",
                "schema")]
    [InlineData("",
                "Cannot auto generate schema because 'Query' type missing.",
                "")]
    [InlineData("""
                scalar foo
                """,
                "Cannot auto generate schema because 'Query' type missing.",
                "")]
    [InlineData("""
                input Query { fizz: Int }
                """,
                "Cannot auto generate schema because 'Query' is type input object instead of object type.",
                "")]
    [InlineData("""
                type Query { fizz: Int }     
                type Other { fizz: Query }     
                """,
                "Cannot auto generate schema because 'Query' type is referenced from other types instead of being a top level type.",
                "")]
    [InlineData("""
                type Query { fizz: Int }     
                input Mutation { fizz: Int }     
                """,
                "Cannot auto generate schema because 'Mutation' is type input object instead of object type.",
                "")]
    [InlineData("""
                type Query { fizz: Int }     
                type Mutation { fizz: Int }     
                type Other { fizz: Mutation }     
                """,
                "Cannot auto generate schema because 'Mutation' type is referenced from other types instead of being a top level type.",
                "")]
    [InlineData("""
                type Query { fizz: Int }     
                input Subscription { fizz: Int }     
                """,
                "Cannot auto generate schema because 'Subscription' is type input object instead of object type.",
                "")]
    [InlineData("""
                type Query { fizz: Int }     
                type Subscription { fizz: Int }     
                type Other { fizz: Subscription }     
                """,
                "Cannot auto generate schema because 'Subscription' type is referenced from other types instead of being a top level type.",
                "")]
    public void ValidationSingleExceptions(string schemaText, string message, string commaPath)
    {
        SchemaValidationSingleException(schemaText, message, commaPath);
    }

    [Theory]

    [InlineData("""
                type Query { first: Int } 
                schema { }
                """,
                "Schema definition must have at least one operation type.",
                "schema",
                "Schema definition missing mandatory query operation.",
                "schema")]
    [InlineData("""
                type Query { first: Int } 
                schema { } 
                schema { }
                """,
                "Schema definition already encountered.",
                "schema",
                "Schema definition must have at least one operation type.",
                "schema",
                "Schema definition missing mandatory query operation.",
                "schema")]
    [InlineData("""
                input Subscription { fizz: Int }
                """,
                "Cannot auto generate schema because 'Query' type missing.",
                "",
                "Cannot auto generate schema because 'Subscription' is type input object instead of object type.",
                "")]
    [InlineData("""
                input Mutation { fizz: Int }
                """,
                "Cannot auto generate schema because 'Query' type missing.",
                "",
                "Cannot auto generate schema because 'Mutation' is type input object instead of object type.",
                "")]
    [InlineData("""
                type foo { fizz: Int }
                schema 
                { 
                    query: foo
                    mutation: foo
                    mutation: foo
                }            
                """,
                "Schema defines the mutation operation more than once.",
                "schema, mutation foo",
                "Schema operations query and mutation cannot have the same 'foo' type.",
                "schema")]
    [InlineData("""
                type foo { fizz: Int }
                schema 
                { 
                    query: foo
                    subscription: foo
                    subscription: foo
                }            
                """,
                "Schema defines the subscription operation more than once.",
                "schema, subscription foo",
                "Schema operations query and subscription cannot have the same 'foo' type.",
                "schema")]
    [InlineData("schema @example { }",
                "Undefined directive '@example' defined on schema.",
                "schema, directive @example",
                "Schema definition must have at least one operation type.",
                "schema",
                "Schema definition missing mandatory query operation.",
                "schema")]
    public void ValidationMultipleExceptions(string schemaText, params string[] messages)
    {
        SchemaValidationMultipleExceptions(schemaText, messages);
    }

    [Theory]
    [InlineData("""
                type Query { fizz: Query }
                """)]
    [InlineData("""
                type Query { fizz: Int }
                """)]
    [InlineData("""
                type Query { fizz: Int }
                type Mutation { fizz: Int }
                """)]
    [InlineData("""
                type Query { fizz: Int }
                type Subscription { fizz: Int }
                """)]
    [InlineData("""
                type Query { fizz: Int }
                type Mutation { fizz: Int }
                type Subscription { fizz: Int }
                """)]
    [InlineData("""
                type q { fizz: Int }
                schema 
                {
                    query: q
                }
                """)]
    [InlineData("""
                type q { fizz: Int }
                type m { fizz: Int }
                schema 
                {
                    query: q
                    mutation: m
                }
                """)]
    [InlineData("""
                type q { fizz: Int }
                type s { fizz: Int }
                schema 
                {
                    query: q
                    subscription: s
                }
                """)]
    [InlineData("""
                type q { fizz: Int }
                type m { fizz: Int }
                type s { fizz: Int }
                schema 
                {
                    query: q
                    mutation: m
                    subscription: s
                }
                """)]
    public void ValidSchemas(string schemaText)
    {
        var schema = new Schema();
        schema.Add(schemaText);
        schema.Validate();
    }
}


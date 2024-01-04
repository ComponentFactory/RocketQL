namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Schemas : UnitTestBase
{
    [Theory]
    // At least one operation
    [InlineData("schema { }",                                               "Schema definition must have at least one operation type.")]
    // Can only define schema once
    [InlineData("schema { } schema { }",                                    "Schema is already defined.")]
    // Mandatory query operation
    [InlineData("""
                type foo { fizz: Int }
                schema { mutation: foo }
                """,                                                        "Schema definition missing mandatory query operation.")]
    // Cannot define same operation more than once
    [InlineData("""
                schema 
                { 
                    query: foo
                    query: foo
                }            
                """,                                                        "Schema defines the query operation more than once.")]
    [InlineData("""
                schema 
                { 
                    query: foo
                    mutation: foo
                    mutation: foo
                }            
                """,                                                        "Schema defines the mutation operation more than once.")]
    [InlineData("""
                schema 
                { 
                    query: foo
                    subscription: foo
                    subscription: foo
                }            
                """,                                                        "Schema defines the subscription operation more than once.")]
    // Check operatin types
    [InlineData("""
                schema { query: foo }
                """,                                                        "Type 'foo' not defined for the schema operation query.")]
    [InlineData("""
                scalar foo
                schema { query: foo }
                """,                                                        "Schema operation query 'foo' has type scalar instead of object type.")]
    [InlineData("""
                type bar { fizz: Int }
                scalar foo
                schema 
                { 
                    query: bar 
                    mutation: foo 
                }
                """,                                                        "Schema operation mutation 'foo' has type scalar instead of object type.")]
    [InlineData("""
                type bar { fizz: Int }
                scalar foo
                schema 
                { 
                    query: bar 
                    subscription: foo 
                }
                """,                                                        "Schema operation subscription 'foo' has type scalar instead of object type.")]    
    [InlineData("""
                type foo { fizz: Int }
                schema 
                { 
                    query: foo 
                    mutation: foo 
                }
                """,                                                        "Schema operations query and mutation cannot have the same 'foo' type.")]
    [InlineData("""
                type foo { fizz: Int }
                schema 
                { 
                    query: foo 
                    subscription: foo 
                }
                """,                                                        "Schema operations query and subscription cannot have the same 'foo' type.")]
    [InlineData("""
                type bar { fizz: Int }
                type foo { fizz: Int }
                schema 
                { 
                    query: bar 
                    mutation: foo 
                    subscription: foo 
                }
                """,                                                        "Schema operations mutation and subscription cannot have the same 'foo' type.")]
    [InlineData("",                                                         "Cannot auto generate schema because 'Query' type missing.")]
    [InlineData("scalar foo",                                               "Cannot auto generate schema because 'Query' type missing.")]
    [InlineData("input Subscription { fizz: Int }",                         "Cannot auto generate schema because 'Query' type missing.")]
    [InlineData("input Mutation { fizz: Int }",                             "Cannot auto generate schema because 'Query' type missing.")]
    [InlineData("input Query { fizz: Int }",                                "Cannot auto generate schema because 'Query' is type input object instead of object type.")]
    [InlineData("""
                type Query { fizz: Int }     
                type Other { fizz: Query }     
                """,                                                        "Cannot auto generate schema because 'Query' type is referenced from other types instead of being a top level type.")]
    [InlineData("""
                type Query { fizz: Int }     
                input Mutation { fizz: Int }     
                """,                                                        "Cannot auto generate schema because 'Mutation' is type input object instead of object type.")]
    [InlineData("""
                type Query { fizz: Int }     
                type Mutation { fizz: Int }     
                type Other { fizz: Mutation }     
                """,                                                        "Cannot auto generate schema because 'Mutation' type is referenced from other types instead of being a top level type.")]
    [InlineData("""
                type Query { fizz: Int }     
                input Subscription { fizz: Int }     
                """,                                                        "Cannot auto generate schema because 'Subscription' is type input object instead of object type.")]
    [InlineData("""
                type Query { fizz: Int }     
                type Subscription { fizz: Int }     
                type Other { fizz: Subscription }     
                """,                                                        "Cannot auto generate schema because 'Subscription' type is referenced from other types instead of being a top level type.")]
    // Directive errors
    [InlineData("schema @example { }",                                      "Undefined directive 'example' defined on schema.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
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


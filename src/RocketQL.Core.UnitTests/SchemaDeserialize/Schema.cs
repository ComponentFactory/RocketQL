﻿namespace RocketQL.Core.UnitTests.SchemaDeserialize;

public class Schema : UnitTestBase
{
    [Theory]
    [InlineData("schema { }")]
    public void Minimum(string schema)
    {
        Serialization.SchemaDeserialize(schema);
    }

    [Theory]
    [InlineData("schema { query: FizzBuzz }", OperationType.QUERY)]
    [InlineData("schema { mutation: FizzBuzz }", OperationType.MUTATION)]
    [InlineData("schema { subscription: FizzBuzz }", OperationType.SUBSCRIPTION)]
    public void OperationTypes(string schema, OperationType operationType)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxSchemaDefinitionNode>(definition);
        var def = ((SyntaxSchemaDefinitionNode)definition);
        Assert.Equal("", def.Description);
        def.Directives.NotNull().Count(0);
        var operation = def.OperationTypes.NotNull().One();
        Assert.Equal(operationType, operation.Operation);
        Assert.Equal("FizzBuzz", operation.NamedType);
    }

    [Theory]
    [InlineData("\"bar\" schema { query: FizzBuzz }")]
    [InlineData("\"\"\"bar\"\"\" schema { query: FizzBuzz }")]
    public void Description(string schema)
    {
        var documentNode = Serialization.SchemaDeserialize(schema);

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxSchemaDefinitionNode>(definition);
        var def = ((SyntaxSchemaDefinitionNode)definition);
        Assert.Equal("bar", def.Description);
        def.Directives.NotNull().Count(0);
        var operation = def.OperationTypes.NotNull().One();
        Assert.Equal(OperationType.QUERY, operation.Operation);
        Assert.Equal("FizzBuzz", operation.NamedType);
    }

    [Fact]
    public void Directive()
    {
        var documentNode = Serialization.SchemaDeserialize("schema @bar { query: FizzBuzz }");

        var definition = documentNode.NotNull().One();
        Assert.IsType<SyntaxSchemaDefinitionNode>(definition);
        var def = ((SyntaxSchemaDefinitionNode)definition);
        Assert.Equal("", def.Description);
        var directive = def.Directives.NotNull().One();
        Assert.Equal("@bar", directive.Name);
        var operation = def.OperationTypes.NotNull().One();
        Assert.Equal(OperationType.QUERY, operation.Operation);
        Assert.Equal("FizzBuzz", operation.NamedType);
    }

    [Theory]
    [InlineData("schema")]
    [InlineData("schema {")]
    [InlineData("schema { query")]
    [InlineData("schema { query:")]
    [InlineData("schema { query: FizzBuzz")]
    [InlineData("schema @bar")]
    public void UnexpectedEndOfFile(string text)
    {
        try
        {
            var documentNode = Serialization.SchemaDeserialize(text);
        }
        catch (SyntaxException ex)
        {
            Assert.Equal($"Unexpected end of file encountered.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}

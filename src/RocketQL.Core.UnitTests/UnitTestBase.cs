using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace RocketQL.Core.UnitTests;

public class UnitTestBase
{
    protected static void CheckTypeName(SyntaxTypeNode node, string typeName, bool nonNullWrapper)
    {
        if (nonNullWrapper)
        {
            Assert.IsType<SyntaxTypeNonNullNode>(node);
            node = ((SyntaxTypeNonNullNode)node).Type;
            Assert.NotNull(node);
        }

        Assert.IsType<SyntaxTypeNameNode>(node);
        var nameNode = (SyntaxTypeNameNode)node;
        Assert.Equal(typeName, nameNode.Name);
    }

    protected static SyntaxTypeNode CheckTypeList(SyntaxTypeNode node, bool nonNullWrapper)
    {
        if (nonNullWrapper)
        {
            Assert.IsType<SyntaxTypeNonNullNode>(node);
            node = ((SyntaxTypeNonNullNode)node).Type;
            Assert.NotNull(node);
        }

        Assert.IsType<SyntaxTypeListNode>(node);
        node = ((SyntaxTypeListNode)node).Type;
        Assert.NotNull(node);
        return node;
    }

    protected static Schema SchemaFromString(string schema)
    {
        var builder = new SchemaBuilder();
        builder.AddFromString(schema);
        return builder.Build();
    }

    protected static Request RequestFromString(ISchema schema, string request)
    {
        var builder = new RequestBuilder();
        builder.AddFromString(request);
        return builder.Build(schema);
    }

    protected static Request RequestFromString(string schema, string request)
    {
        var builder = new RequestBuilder();
        builder.AddFromString(request);
        return builder.Build(SchemaFromString(schema));
    }

    protected static void SchemaValidationNoException(string schemaTest)
    {
        var schema = new SchemaBuilder();
        schema.AddFromString(schemaTest);
        schema.Build();
    }

    protected static void SchemaValidationSingleException(string schemaTest, string message, string commaPath)
    {
        try
        {
            var schema = new SchemaBuilder();
            schema.AddFromString(schemaTest);
            schema.Build();

            Assert.Fail("Exception expected");
        }
        catch (Exception ex)
        {
            var validation = Assert.IsType<ValidationException>(ex);
            Assert.NotNull(validation.Source);
            Assert.Equal(message, validation.Message);
            Assert.Equal(commaPath, validation.CommaPath);
        }
    }

    protected static void SchemaValidationSingleException(string schemaTest1, string schemaTest2, string message, string commaPath)
    {
        try
        {
            var schema = new SchemaBuilder();
            schema.AddFromString(schemaTest1);
            schema.AddFromString(schemaTest2);
            schema.Build();

            Assert.Fail("Exception expected");
        }
        catch (Exception ex)
        {
            var validation = Assert.IsType<ValidationException>(ex);
            Assert.Equal(message, ex.Message);
            Assert.NotNull(validation.Source);
            Assert.Equal(message, validation.Message);
            Assert.Equal(commaPath, validation.CommaPath);
        }
    }

    protected static void SchemaValidationMultipleExceptions(string schemaTest, params string[] messages)
    {
        try
        {
            var schema = new SchemaBuilder();
            schema.AddFromString(schemaTest);
            schema.Build();

            Assert.Fail("Exception expected");
        }
        catch (Exception ex)
        {
            var expected = messages.Length / 2;
            var aggregate = Assert.IsType<RocketExceptions>(ex);
            Assert.Equal(expected, aggregate.InnerExceptions.Count);

            for (var i = 0; i < expected; i++)
            {
                var validatonException = aggregate.InnerExceptions[i] as ValidationException;
                Assert.NotNull(validatonException);
                Assert.Equal(messages[i * 2], validatonException.Message);
                Assert.Equal(messages[1 + (i * 2)], validatonException.CommaPath);
            }
        }
    }

    protected static void RequestValidationSingleException(string schemaTest, string requestTest, string message, string commaPath)
    {
        try
        {
            var schema = new SchemaBuilder();
            schema.AddFromString(schemaTest);
            var request = new RequestBuilder();
            request.AddFromString(requestTest);
            request.Build(schema.Build());

            Assert.Fail("Exception expected");
        }
        catch (Exception ex)
        {
            Assert.Equal(message, ex.Message);
            var validation = Assert.IsType<ValidationException>(ex);
            Assert.NotNull(validation.Source);
            Assert.Equal(commaPath, validation.CommaPath); ;
        }
    }
}


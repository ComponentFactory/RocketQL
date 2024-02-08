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

    protected static ISchema SchemaFromString(string schemaText)
    {
        var builder = new SchemaBuilder();
        builder.AddFromString(schemaText);
        return builder.Build();
    }

    protected static IRequest RequestFromString(ISchema schema, string request)
    {
        var builder = new RequestBuilder();
        builder.AddFromString(request);
        return builder.Build(schema);
    }

    protected static IRequest RequestFromString(string schemaText, string request)
    {
        var builder = new RequestBuilder();
        builder.AddFromString(request);
        return builder.Build(SchemaFromString(schemaText));
    }

    protected static void SchemaValidationNoException(string schemaText)
    {
        SchemaFromString(schemaText);
    }

    protected static void RequestValidationNoException(string schemaText, string requestText)
    {
        RequestFromString(SchemaFromString(schemaText), requestText);
    }

    protected static void SchemaValidationSingleException(string schemaText, string message, string commaPath)
    {
        try
        {
            var schema = new SchemaBuilder();
            schema.AddFromString(schemaText);
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

    protected static void SchemaValidationSingleException(string schemaText1, string schemaText2, string message, string commaPath)
    {
        try
        {
            var schema = new SchemaBuilder();
            schema.AddFromString(schemaText1);
            schema.AddFromString(schemaText2);
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

    protected static void SchemaValidationMultipleExceptions(string schemaText, params string[] messages)
    {
        try
        {
            var schema = new SchemaBuilder();
            schema.AddFromString(schemaText);
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

    protected static void RequestValidationSingleException(string schemaText, string requestText, string message, string commaPath)
    {
        try
        {
            var schema = new SchemaBuilder();
            schema.AddFromString(schemaText);
            var request = new RequestBuilder();
            request.AddFromString(requestText);
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


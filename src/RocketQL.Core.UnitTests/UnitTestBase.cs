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

    protected static void SchemaValidationNoException(string schemaTest)
    {
        var schema = new Schema();
        schema.Add(schemaTest);
        schema.Validate();
    }

    protected static void SchemaValidationSingleException(string schemaTest, string message)
    {
        try
        {
            var schema = new Schema();
            schema.Add(schemaTest);
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (Exception ex)
        {
            var validation = Assert.IsType<ValidationException>(ex);
            Assert.NotNull(validation.Source);
            Assert.Equal(message, validation.Message);
        }
    }


    protected static void SchemaValidationSinglePathException(string schemaTest, string message, string commaPath)
    {
        try
        {
            var schema = new Schema();
            schema.Add(schemaTest);
            schema.Validate();

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

    protected static void SchemaValidationSinglePathException(string schemaTest1, string schemaTest2, string message, string commaPath)
    {
        try
        {
            var schema = new Schema();
            schema.Add(schemaTest1);
            schema.Add(schemaTest2);
            schema.Validate();

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
            var schema = new Schema();
            schema.Add(schemaTest);
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (Exception ex)
        {
            var expected = messages.Length;
            var aggregate = Assert.IsType<RocketExceptions>(ex);
            Assert.Equal(expected, aggregate.InnerExceptions.Count);

            for (var i = 0; i < expected; i++)
            {
                var validatonException = aggregate.InnerExceptions[i] as ValidationException;
                Assert.NotNull(validatonException);
                Assert.Equal(messages[i], validatonException.Message);
            }
        }
    }

    protected static void SchemaValidationMultiplePathExceptions(string schemaTest, params string[] messages)
    {
        try
        {
            var schema = new Schema();
            schema.Add(schemaTest);
            schema.Validate();

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

    protected static void RequestSchemaValidationSingleException(string schemaTest, string requestTest, string message)
    {
        try
        {
            var schema = new Schema();
            schema.Add(schemaTest);
            schema.Validate();
            var request = new Request();
            request.Add(requestTest);
            request.ValidateSchema(schema);

            Assert.Fail("Exception expected");
        }
        catch (Exception ex)
        {
            Assert.Equal(message, ex.Message);
            var validation = Assert.IsType<ValidationException>(ex);
            Assert.NotNull(validation.Source);
        }
    }


}


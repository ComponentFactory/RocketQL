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
        SyntaxTypeNameNode nameNode = (SyntaxTypeNameNode)node;
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

    protected static void SchemaValidationException(string schemaTest, string message)
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
            Assert.Equal(message, ex.Message);
            var validation = Assert.IsType<ValidationException>(ex);
            Assert.Single(validation.Locations);
        }
    }

    protected static void SchemaValidationException(string schemaTest1, string schemaTest2, string message)
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
            Assert.Equal(message, ex.Message);
            var validation = Assert.IsType<ValidationException>(ex);
            Assert.Single(validation.Locations);
        }
    }
}


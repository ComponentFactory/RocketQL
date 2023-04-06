namespace RocketQL.Core.UnitTests.Parser;

public class InputValueDefinitionList
{
    [Theory]
    [InlineData("directive @foo (fizz: buzz) on ENUM", false)]
    [InlineData("directive @foo (fizz: buzz!) on ENUM", true)]
    public void SingleNameType(string schema, bool nonNull)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();
        
        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.Null(node.Description);
        Assert.Equal("fizz", node.Name);
        Assert.IsType<TypeNameNode>(node.Type);
        TypeNameNode nameNode = (TypeNameNode)node.Type;
        Assert.Equal("buzz", nameNode.Name);
        Assert.Equal(nonNull, nameNode.NonNull);
    }

    [Theory]
    [InlineData("directive @foo (fizz: [buzz]) on ENUM", false, false)]
    [InlineData("directive @foo (fizz: [buzz]!) on ENUM", true, false)]
    [InlineData("directive @foo (fizz: [buzz!]) on ENUM", false, true)]
    [InlineData("directive @foo (fizz: [buzz!]!) on ENUM", true, true)]
    public void SingleListType(string schema, bool listNonNull, bool typeNonNull)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.Null(node.Description);
        Assert.Equal("fizz", node.Name);
        Assert.IsType<TypeListNode>(node.Type);
        TypeListNode listNode = (TypeListNode)node.Type;
        Assert.Equal(listNonNull, listNode.NonNull);
        Assert.IsType<TypeNameNode>(listNode.Type);
        TypeNameNode nameNode = (TypeNameNode)listNode.Type;
        Assert.Equal("buzz", nameNode.Name);
        Assert.Equal(typeNonNull, nameNode.NonNull);
    }

    [Theory]
    [InlineData("directive @foo (fizz: [[buzz]]) on ENUM", false, false, false)]
    [InlineData("directive @foo (fizz: [[buzz]]!) on ENUM", true, false, false)]
    [InlineData("directive @foo (fizz: [[buzz]!]) on ENUM", false, true, false)]
    [InlineData("directive @foo (fizz: [[buzz]!]!) on ENUM", true, true, false)]
    [InlineData("directive @foo (fizz: [[buzz!]]) on ENUM", false, false, true)]
    [InlineData("directive @foo (fizz: [[buzz!]]!) on ENUM", true, false, true)]
    [InlineData("directive @foo (fizz: [[buzz!]!]) on ENUM", false, true, true)]
    [InlineData("directive @foo (fizz: [[buzz!]!]!) on ENUM", true, true, true)]
    public void SingleListListType(string schema, bool outerNonNull, bool innerNonNull, bool typeNonNull)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();

        Assert.Single(documentNode.DirectiveDefinitions[0].Arguments);
        var node = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.Null(node.Description);
        Assert.Equal("fizz", node.Name);
        Assert.IsType<TypeListNode>(node.Type);
        TypeListNode listNodeOuter = (TypeListNode)node.Type;
        Assert.Equal(outerNonNull, listNodeOuter.NonNull);
        Assert.IsType<TypeListNode>(listNodeOuter.Type);
        TypeListNode listNodeInner = (TypeListNode)listNodeOuter.Type;
        Assert.Equal(innerNonNull, listNodeInner.NonNull);
        Assert.IsType<TypeNameNode>(listNodeInner.Type);
        TypeNameNode nameNode = (TypeNameNode)listNodeInner.Type;
        Assert.Equal("buzz", nameNode.Name);
        Assert.Equal(typeNonNull, nameNode.NonNull);
    }

    [Theory]
    [InlineData("directive @foo (fizz:buzz hello:world) on ENUM")]
    [InlineData("directive @foo (fizz: buzz hello: world) on ENUM")]
    [InlineData("directive @foo (fizz:buzz,hello: world) on ENUM")]
    [InlineData("directive @foo (fizz: buzz, hello:world) on ENUM")]
    public void DoubleNameType(string schema)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();

        Assert.Equal(2, documentNode.DirectiveDefinitions[0].Arguments.Count);
        var node1 = documentNode.DirectiveDefinitions[0].Arguments[0];
        Assert.Null(node1.Description);
        Assert.Equal("fizz", node1.Name);
        Assert.IsType<TypeNameNode>(node1.Type);
        TypeNameNode nameNode1 = (TypeNameNode)node1.Type;
        Assert.Equal("buzz", nameNode1.Name);
        Assert.False(nameNode1.NonNull);
        var node2 = documentNode.DirectiveDefinitions[0].Arguments[1];
        Assert.Null(node2.Description);
        Assert.Equal("hello", node2.Name);
        Assert.IsType<TypeNameNode>(node2.Type);
        TypeNameNode nameNode2 = (TypeNameNode)node2.Type;
        Assert.Equal("world", nameNode2.Name);
        Assert.False(nameNode2.NonNull);
    }
}


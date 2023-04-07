using System.Reflection.Metadata;

namespace RocketQL.Core.UnitTests.Parser;

public class Operation
{
    [Theory]
    [InlineData("query { foo }", OperationType.QUERY)]
    [InlineData("mutation { foo }", OperationType.MUTATION)]
    [InlineData("subscription { foo }", OperationType.SUBSCRIPTION)]
    public void OperationTypes(string schema, OperationType operationType)
    {
        var t = new Core.Parser(schema);
        var documentNode = t.Parse();

        var operation = documentNode.NotNull().Operations.NotNull().One();
        Assert.Equal(operationType, operation.Operation);
        Assert.Equal(string.Empty, operation.Name);
        operation.VariableDefinitions.NotNull().Count(0);
        operation.Directives.NotNull().Count(0);
        var field = (FieldSelectionNode)operation.SelectionSet.NotNull().One();
        Assert.Equal(string.Empty, field.Alias);
        Assert.Equal("foo", field.Name);
        field.Arguments.NotNull().Count(0);
        field.Directives.NotNull().Count(0);
        field.SelectionSet.NotNull().Count(0);        
    }

    [Fact]
    public void OperationName()
    {
        var t = new Core.Parser("query name { foo }");
        var documentNode = t.Parse();

        var operation = documentNode.NotNull().Operations.NotNull().One();
        Assert.Equal(OperationType.QUERY, operation.Operation);
        Assert.Equal("name", operation.Name);
        operation.VariableDefinitions.NotNull().Count(0);
        operation.Directives.NotNull().Count(0);
        var field = (FieldSelectionNode)operation.SelectionSet.NotNull().One();
        Assert.Equal(string.Empty, field.Alias);
        Assert.Equal("foo", field.Name);
        field.Arguments.NotNull().Count(0);
        field.Directives.NotNull().Count(0);
        field.SelectionSet.NotNull().Count(0);
    }

    [Fact]
    public void FieldAlias()
    {
        var t = new Core.Parser("query name { foo : bar }");
        var documentNode = t.Parse();

        var operation = documentNode.NotNull().Operations.NotNull().One();
        Assert.Equal(OperationType.QUERY, operation.Operation);
        Assert.Equal("name", operation.Name);
        operation.VariableDefinitions.NotNull().Count(0);
        operation.Directives.NotNull().Count(0);
        var field = (FieldSelectionNode)operation.SelectionSet.NotNull().One();
        Assert.Equal("foo", field.Alias);
        Assert.Equal("bar", field.Name);
        field.Arguments.NotNull().Count(0);
        field.Directives.NotNull().Count(0);
        field.SelectionSet.NotNull().Count(0);
    }

    [Fact]
    public void FieldArgument()
    {
        var t = new Core.Parser("query name { foo(bar: 3) }");
        var documentNode = t.Parse();

        var operation = documentNode.NotNull().Operations.NotNull().One();
        Assert.Equal(OperationType.QUERY, operation.Operation);
        Assert.Equal("name", operation.Name);
        operation.VariableDefinitions.NotNull().Count(0);
        operation.Directives.NotNull().Count(0);
        var field = (FieldSelectionNode)operation.SelectionSet.NotNull().One();
        Assert.Equal(string.Empty, field.Alias);
        Assert.Equal("foo", field.Name);
        var argument = field.Arguments.NotNull().One();
        Assert.Equal("bar", argument.Name);
        Assert.Equal("3", argument.Value.IsType<IntValueNode>().Value);
        field.Directives.NotNull().Count(0);
        field.SelectionSet.NotNull().Count(0);
    }

    [Fact]
    public void FieldArgumentUsingVariable()
    {
        var t = new Core.Parser("query name { foo(bar: $var) }");
        var documentNode = t.Parse();

        var operation = documentNode.NotNull().Operations.NotNull().One();
        Assert.Equal(OperationType.QUERY, operation.Operation);
        Assert.Equal("name", operation.Name);
        operation.VariableDefinitions.NotNull().Count(0);
        operation.Directives.NotNull().Count(0);
        var field = (FieldSelectionNode)operation.SelectionSet.NotNull().One();
        Assert.Equal(string.Empty, field.Alias);
        Assert.Equal("foo", field.Name);
        var argument = field.Arguments.NotNull().One();
        Assert.Equal("bar", argument.Name);
        Assert.Equal("var", argument.Value.IsType<VariableValueNode>().Value);
        field.Directives.NotNull().Count(0);
        field.SelectionSet.NotNull().Count(0);
    }

    [Fact]
    public void FieldDirective()
    {
        var t = new Core.Parser("query name { foo @bar }");
        var documentNode = t.Parse();

        var operation = documentNode.NotNull().Operations.NotNull().One();
        Assert.Equal(OperationType.QUERY, operation.Operation);
        Assert.Equal("name", operation.Name);
        operation.VariableDefinitions.NotNull().Count(0);
        operation.Directives.NotNull().Count(0);
        var field = (FieldSelectionNode)operation.SelectionSet.NotNull().One();
        Assert.Equal(string.Empty, field.Alias);
        Assert.Equal("foo", field.Name);
        field.Arguments.NotNull().Count(0);
        var directive = field.Directives.NotNull().One();
        Assert.Equal("bar", directive.Name);
        field.SelectionSet.NotNull().Count(0);
    }

    [Fact]
    public void FieldSelectionSet()
    {
        var t = new Core.Parser("query name { foo { bar } }");
        var documentNode = t.Parse();

        var operation = documentNode.NotNull().Operations.NotNull().One();
        Assert.Equal(OperationType.QUERY, operation.Operation);
        Assert.Equal("name", operation.Name);
        operation.VariableDefinitions.NotNull().Count(0);
        operation.Directives.NotNull().Count(0);
        var field1 = (FieldSelectionNode)operation.SelectionSet.NotNull().One();
        Assert.Equal(string.Empty, field1.Alias);
        Assert.Equal("foo", field1.Name);
        field1.Arguments.NotNull().Count(0);
        field1.Directives.NotNull().Count(0);
        var field2 = (FieldSelectionNode)field1.SelectionSet.NotNull().One();
        Assert.Equal(string.Empty, field2.Alias);
        Assert.Equal("bar", field2.Name);
        field2.Arguments.NotNull().Count(0);
        field2.Directives.NotNull().Count(0);
    }

    [Fact]
    public void FieldAliasArgumentDirectiveSelectionSet()
    {
        var t = new Core.Parser("query name { foo : bar(fizz: 3) @bar { foo2 : bar2(fizz2: 3) @bar2} }");
        var documentNode = t.Parse();

        var operation = documentNode.NotNull().Operations.NotNull().One();
        Assert.Equal(OperationType.QUERY, operation.Operation);
        Assert.Equal("name", operation.Name);
        operation.VariableDefinitions.NotNull().Count(0);
        operation.Directives.NotNull().Count(0);
        var field1 = (FieldSelectionNode)operation.SelectionSet.NotNull().One();
        Assert.Equal("foo", field1.Alias);
        Assert.Equal("bar", field1.Name);
        var argument1 = field1.Arguments.NotNull().One();
        Assert.Equal("fizz", argument1.Name);
        Assert.Equal("3", argument1.Value.IsType<IntValueNode>().Value);
        var directive1 = field1.Directives.NotNull().One();
        Assert.Equal("bar", directive1.Name);
        var field2 = (FieldSelectionNode)field1.SelectionSet.NotNull().One();
        Assert.Equal("foo2", field2.Alias);
        Assert.Equal("bar2", field2.Name);
        var argument2 = field2.Arguments.NotNull().One();
        Assert.Equal("fizz2", argument2.Name);
        Assert.Equal("3", argument2.Value.IsType<IntValueNode>().Value);
        var directive2 = field2.Directives.NotNull().One();
        Assert.Equal("bar2", directive2.Name);
    }
}




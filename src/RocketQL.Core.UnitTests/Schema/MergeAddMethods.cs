namespace RocketQL.Core.UnitTests.SchemaTests;

public class MergeAddMethods
{
    private static string _scalarSchema =
        """
            "description"
            scalar Foo
        """;

    [Fact]
    public void MergeString()
    {
        var schema = new Schema();
        schema.Merge(_scalarSchema);
        schema.Validate();

        Assert.Single(schema.Scalars);
        var foo = schema.Scalars["Foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("Foo", foo.Name);
        Assert.Contains(nameof(MergeString), foo.Location.Source);
    }

    [Fact]
    public void MergeStringSource()
    {
        var schema = new Schema();
        schema.Merge(_scalarSchema, "source");
        schema.Validate();

        Assert.Single(schema.Scalars);
        var foo = schema.Scalars["Foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("Foo", foo.Name);
        Assert.Equal("source", foo.Location.Source);
    }

    [Fact]
    public void MergeSyntaxSchemaNode()
    {
        var schema = new Schema();

        var syntaxSchemaNode =  Serialization.SchemaDeserialize(_scalarSchema);
        schema.Merge(syntaxSchemaNode);
        schema.Validate();

        Assert.Single(schema.Scalars);
        var foo = schema.Scalars["Foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("Foo", foo.Name);
        Assert.Contains(nameof(MergeSyntaxSchemaNode), foo.Location.Source);
    }

    [Fact]
    public void MergeSyntaxSchemaNodeSource()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_scalarSchema, "source");
        schema.Merge(syntaxSchemaNode);
        schema.Validate();

        Assert.Single(schema.Scalars);
        var foo = schema.Scalars["Foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("Foo", foo.Name);
        Assert.Equal("source", foo.Location.Source);
    }

    [Fact]
    public void MergeSyntaxSchemaNodeList()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_scalarSchema);
        schema.Merge(new SyntaxSchemaNode[] { syntaxSchemaNode } );
        schema.Validate();

        Assert.Single(schema.Scalars);
        var foo = schema.Scalars["Foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("Foo", foo.Name);
        Assert.Contains(nameof(MergeSyntaxSchemaNodeList), foo.Location.Source);
    }

    [Fact]
    public void MergeSyntaxSchemaNodeListSource()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_scalarSchema, "source");
        schema.Merge(new SyntaxSchemaNode[] { syntaxSchemaNode });
        schema.Validate();

        Assert.Single(schema.Scalars);
        var foo = schema.Scalars["Foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("Foo", foo.Name);
        Assert.Equal("source", foo.Location.Source);
    }

    [Fact]
    public void AddScalarTypes()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_scalarSchema);
        schema.AddScalarTypes(syntaxSchemaNode.ScalarTypes);
        schema.Validate();

        Assert.Single(schema.Scalars);
        var foo = schema.Scalars["Foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("Foo", foo.Name);
        Assert.Contains(nameof(AddScalarTypes), foo.Location.Source);
    }

    [Fact]
    public void AddScalarType()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_scalarSchema);
        schema.AddScalarType(syntaxSchemaNode.ScalarTypes[0]);
        schema.Validate();

        Assert.Single(schema.Scalars);
        var foo = schema.Scalars["Foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("Foo", foo.Name);
        Assert.Contains(nameof(AddScalarType), foo.Location.Source);
    }
}


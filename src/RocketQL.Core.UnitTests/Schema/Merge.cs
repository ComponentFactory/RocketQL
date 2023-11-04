using Newtonsoft.Json.Bson;

namespace RocketQL.Core.UnitTests.SchemaTests;

public class Merge
{
    private static string _schema =
        """
            "description"
            scalar aaa

            "description"
            directive @bbb on ENUM

            "description"
            enum ccc {
                FIRST
                SECOND
            }
        """;

    [Fact]
    public void MergeString()
    {
        var schema = new Schema();
        schema.Merge(_schema);
        schema.Validate();
        CheckSchema(schema, nameof(MergeString));
    }

    [Fact]
    public void MergeStringSource()
    {
        var schema = new Schema();
        schema.Merge(_schema, "source");
        schema.Validate();
        CheckSchema(schema, "source");
    }

    [Fact]
    public void MergeSyntaxSchemaNode()
    {
        var schema = new Schema();
        var syntaxSchemaNode =  Serialization.SchemaDeserialize(_schema);
        schema.Merge(syntaxSchemaNode);
        schema.Validate();
        CheckSchema(schema, nameof(MergeSyntaxSchemaNode));
    }

    [Fact]
    public void MergeSyntaxSchemaNodeSource()
    {
        var schema = new Schema();
        var syntaxSchemaNode = Serialization.SchemaDeserialize(_schema, "source");
        schema.Merge(syntaxSchemaNode);
        schema.Validate();
        CheckSchema(schema, "source");
    }

    [Fact]
    public void MergeSyntaxSchemaNodeList()
    {
        var schema = new Schema();
        var syntaxSchemaNode = Serialization.SchemaDeserialize(_schema);
        schema.Merge(new SyntaxSchemaNode[] { syntaxSchemaNode } );
        schema.Validate();
        CheckSchema(schema, nameof(MergeSyntaxSchemaNodeList));
    }

    [Fact]
    public void MergeSyntaxSchemaNodeListSource()
    {
        var schema = new Schema();
        var syntaxSchemaNode = Serialization.SchemaDeserialize(_schema, "source");
        schema.Merge(new SyntaxSchemaNode[] { syntaxSchemaNode });
        schema.Validate();
        CheckSchema(schema, "source");
    }

    private void CheckSchema(Schema schema, string source)
    {
        Assert.Single(schema.Scalars);
        Assert.Single(schema.Directives);
        Assert.Single(schema.Enums);
        var aaa = schema.Scalars["aaa"];
        Assert.NotNull(aaa);
        Assert.Equal("description", aaa.Description);
        Assert.Equal("aaa", aaa.Name);
        Assert.Contains(source, aaa.Location.Source);
        var bbb = schema.Directives["bbb"];
        Assert.NotNull(bbb);
        Assert.Equal("description", bbb.Description);
        Assert.Equal("bbb", bbb.Name);
        Assert.Contains(source, bbb.Location.Source);
        var ccc = schema.Enums["ccc"];
        Assert.NotNull(ccc);
        Assert.Equal("description", ccc.Description);
        Assert.Equal("ccc", ccc.Name);
        Assert.Contains(source, ccc.Location.Source);
    }
}


using Newtonsoft.Json.Bson;

namespace RocketQL.Core.UnitTests.SchemaTests;

public class Merge
{
    private static string _schema =
        """
            "test scalar"
            scalar aaa
        
            "test directive"
            directive @bbb on ENUM
        
            "test enum"
            enum ccc {
                FIRST
                SECOND
            }

            "test interface"
            interface ddd { 
                fizz : Integer
                bar : String
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
        var aaa = schema.Scalars["aaa"];
        Assert.NotNull(aaa);
        Assert.Equal("test scalar", aaa.Description);
        Assert.Equal("aaa", aaa.Name);
        Assert.Contains(source, aaa.Location.Source);

        Assert.Single(schema.Directives);
        var bbb = schema.Directives["bbb"];
        Assert.NotNull(bbb);
        Assert.Equal("test directive", bbb.Description);
        Assert.Equal("bbb", bbb.Name);
        Assert.Contains(source, bbb.Location.Source);

        Assert.Single(schema.Enums);
        var ccc = schema.Enums["ccc"];
        Assert.NotNull(ccc);
        Assert.Equal("test enum", ccc.Description);
        Assert.Equal("ccc", ccc.Name);
        Assert.Contains(source, ccc.Location.Source);

        Assert.Single(schema.Interfaces);
        var ddd = schema.Interfaces["ddd"];
        Assert.NotNull(ddd);
        Assert.Equal("test interface", ddd.Description);
        Assert.Equal("ddd", ddd.Name);
        Assert.Contains(source, ddd.Location.Source);

    }
}


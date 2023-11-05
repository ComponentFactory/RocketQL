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
                buzz : String
            }       
            
            "test type"
            type eee implements ddd { 
                fizz : Integer
                buzz : String
            }   

            "test union"
            union fff = fizz | buzz

            "test input object"
            input ggg { 
                fizz : Integer
                buzz : String
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
        Assert.Single(schema.ScalarTypes);
        var aaa = schema.ScalarTypes["aaa"];
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

        Assert.Single(schema.EnumTypes);
        var ccc = schema.EnumTypes["ccc"];
        Assert.NotNull(ccc);
        Assert.Equal("test enum", ccc.Description);
        Assert.Equal("ccc", ccc.Name);
        Assert.Contains(source, ccc.Location.Source);

        Assert.Single(schema.InterfaceTypes);
        var ddd = schema.InterfaceTypes["ddd"];
        Assert.NotNull(ddd);
        Assert.Equal("test interface", ddd.Description);
        Assert.Equal("ddd", ddd.Name);
        Assert.Contains(source, ddd.Location.Source);

        Assert.Single(schema.ObjectTypes);
        var eee = schema.ObjectTypes["eee"];
        Assert.NotNull(eee);
        Assert.Equal("test type", eee.Description);
        Assert.Equal("eee", eee.Name);
        Assert.Contains(source, eee.Location.Source);

        Assert.Single(schema.UnionTypes);
        var fff = schema.UnionTypes["fff"];
        Assert.NotNull(fff);
        Assert.Equal("test union", fff.Description);
        Assert.Equal("fff", fff.Name);
        Assert.Contains(source, fff.Location.Source);

        Assert.Single(schema.InputObjectTypes);
        var ggg = schema.InputObjectTypes["ggg"];
        Assert.NotNull(ggg);
        Assert.Equal("test input object", ggg.Description);
        Assert.Equal("ggg", ggg.Name);
        Assert.Contains(source, ggg.Location.Source);
    }
}


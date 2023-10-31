namespace RocketQL.Core.UnitTests.MergeSchema;

public class ScalarDefinition
{
    [Fact]
    public void MergeOne()
    {
        var schema = new Schema();
        schema.Merge("scalar Foo");

        //Assert.Single(schema.SchemaNode.Directives);
        //var d1 = schema.SchemaNode.Directives["foo"];
        //Assert.NotNull(d1);
        //Assert.Equal(DirectiveLocations.ENUM, d1.DirectiveLocations);
        //Assert.Equal("test1", d1.Location.Source);
    }

    [Fact]
    public void MergeTwo()
    {
        var schema = new Schema();
        schema.Merge("scalar Foo");
        schema.Merge("scalar Bar");

        //Assert.Equal(2, schema.SchemaNode.Directives.Count);
        //var d1 = schema.SchemaNode.Directives["foo"];
        //Assert.NotNull(d1);
        //Assert.Equal(DirectiveLocations.ENUM, d1.DirectiveLocations);
        //Assert.Equal("test1", d1.Location.Source);
        //var d2 = schema.SchemaNode.Directives["bar"];
        //Assert.NotNull(d2);
        //Assert.Equal(DirectiveLocations.INTERFACE, d2.DirectiveLocations);
        //Assert.Equal("test2", d2.Location.Source);
    }

    //[Fact]
    //public void MergeThree()
    //{
    //    var schema = new Schema();

    //    schema.Merge(Serialization.SchemaDeserialize("test1", "directive @foo on ENUM"));
    //    schema.Merge(Serialization.SchemaDeserialize("test2", "directive @bar on INTERFACE"));
    //    schema.Merge(Serialization.SchemaDeserialize("test3", "directive @fizz on SCALAR"));

    //    Assert.Equal(3, schema.SchemaNode.Directives.Count);
    //    var d1 = schema.SchemaNode.Directives["foo"];
    //    Assert.NotNull(d1);
    //    Assert.Equal(DirectiveLocations.ENUM, d1.DirectiveLocations);
    //    Assert.Equal("test1", d1.Location.Source);
    //    var d2 = schema.SchemaNode.Directives["bar"];
    //    Assert.NotNull(d2);
    //    Assert.Equal(DirectiveLocations.INTERFACE, d2.DirectiveLocations);
    //    Assert.Equal("test2", d2.Location.Source);
    //    var d3 = schema.SchemaNode.Directives["fizz"];
    //    Assert.NotNull(d3);
    //    Assert.Equal(DirectiveLocations.SCALAR, d3.DirectiveLocations);
    //    Assert.Equal("test3", d3.Location.Source);
    //}

    //[Fact]
    //public void PreventDuplicates()
    //{
    //    try
    //    {
    //        var schema = new Schema();

    //        schema.Merge(Serialization.SchemaDeserialize("test1", "directive @foo on ENUM"));
    //        schema.Merge(Serialization.SchemaDeserialize("test2", "directive @foo on ENUM"));
    //    }
    //    catch (ValidationException ex)
    //    {
    //        Assert.Equal($"Directive 'foo' is already defined.", ex.Message);
    //    }
    //    catch
    //    {
    //        Assert.Fail("Wrong exception");
    //    }
    //}
}


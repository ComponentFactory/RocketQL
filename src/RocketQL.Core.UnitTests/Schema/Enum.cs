namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Enum
{
    [Fact]
    public void AddEnum()
    {
        var schema = new Schema();
        schema.Add("enum foo { FIRST SECOND }");
        schema.Validate();

        Assert.Single(schema.Types);
        var foo = schema.Types["foo"] as EnumTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.EnumValues.Count);
        Assert.NotNull(foo.EnumValues["FIRST"]);
        Assert.NotNull(foo.EnumValues["SECOND"]);
        Assert.Contains(nameof(AddEnum), foo.Location.Source);
    }

    [Fact]
    public void EnumNameAlreadyDefined()
    {
        try
        {
            var schema = new Schema();
            schema.Add("enum foo { FIRST }");
            schema.Add("enum foo { FIRST }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Enum name 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void EnumNameDoubleUnderscore()
    {
        try
        {
            var schema = new Schema();
            schema.Add("enum __foo { FIRST }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Enum name '__foo' not allowed to start with two underscores.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}


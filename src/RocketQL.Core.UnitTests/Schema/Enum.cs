namespace RocketQL.Core.UnitTests.SchemaTests;

public class Enum
{
    private static readonly string _foo =
        """
            "description"
            enum foo {
                FIRST
                SECOND
            }
        """;

    [Fact]
    public void AddEnum()
    {
        var schema = new Schema();
        schema.Add(_foo);
        schema.Validate();

        Assert.Single(schema.Types);
        var foo = schema.Types["foo"] as EnumTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.EnumValues.Count);
        Assert.NotNull(foo.EnumValues["FIRST"]);
        Assert.NotNull(foo.EnumValues["SECOND"]);
        Assert.Contains(nameof(AddEnum), foo.Location.Source);
    }

    [Fact]
    public void NameAlreadyDefined()
    {
        try
        {
            var schema = new Schema();
            schema.Add(_foo);
            schema.Add(_foo);
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Enum type name 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}


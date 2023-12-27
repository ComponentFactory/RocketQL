namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Union : UnitTestBase
{
    [Fact]
    public void NameAlreadyDefined()
    {
        SchemaValidationException("type fizz { buzz: Int } union foo = fizz", 
                                  "union foo = fizz", 
                                  "Union 'foo' is already defined.");
    }

    [Theory]
    // Double underscores
    [InlineData("type fizz { buzz: Int } union __foo = fizz",       "Union '__foo' not allowed to start with two underscores.")]
    // Undefined member type
    [InlineData("union foo = fizz",                                 "Undefined member type 'fizz' defined on union 'foo'.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }

    [Fact]
    public void AddUnionType()
    {
        var schema = new Schema();
        schema.Add("""
                   type fizz { buzz: Int }
                   union foo = fizz
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as UnionTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Single(foo.MemberTypes);
        Assert.NotNull(foo.MemberTypes["fizz"]);
        Assert.Contains(nameof(AddUnionType), foo.Location.Source);
    }
}


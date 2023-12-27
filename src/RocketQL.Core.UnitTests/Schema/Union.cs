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
    // Member type can only be an object type
    [InlineData("scalar fizz union foo = fizz",                     "Cannot reference member type 'fizz' defined on union 'foo' because it is a scalar.")]
    [InlineData("interface fizz { buzz: Int } union foo = fizz",    "Cannot reference member type 'fizz' defined on union 'foo' because it is an interface.")]
    [InlineData("enum fizz { BUZZ } union foo = fizz",              "Cannot reference member type 'fizz' defined on union 'foo' because it is an enum.")]
    [InlineData("input fizz { buzz: Int } union foo = fizz",        "Cannot reference member type 'fizz' defined on union 'foo' because it is an input object.")]
    [InlineData("""
                type fizz { buzz: Int }
                union foo = fizz
                union buzz = foo
                """,                                                "Cannot reference member type 'foo' defined on union 'buzz' because it is a union.")]
    public void ValidationExceptions(string schemaText, string message)
    {
        SchemaValidationException(schemaText, message);
    }

    [Fact]
    public void AddUnionType()
    {
        var schema = new Schema();
        schema.Add("""
                   type fizz { b1: Int }
                   type buzz { b2: Int }
                   union foo = | fizz | buzz
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as UnionTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.MemberTypes.Count);
        Assert.NotNull(foo.MemberTypes["fizz"]);
        Assert.NotNull(foo.MemberTypes["buzz"]);
        Assert.Contains(nameof(AddUnionType), foo.Location.Source);
    }
}


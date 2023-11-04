﻿namespace RocketQL.Core.UnitTests.SchemaTests;

public class Scalar
{
    private static readonly string _foo =
        """
            "description"
            scalar foo
        """;


    [Fact]
    public void AddScalarTypes()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddScalarTypes(syntaxSchemaNode.ScalarTypes);
        schema.Validate();

        Assert.Single(schema.Scalars);
        var foo = schema.Scalars["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Contains(nameof(AddScalarTypes), foo.Location.Source);
    }

    [Fact]
    public void AddScalarType()
    {
        var schema = new Schema();

        var syntaxSchemaNode = Serialization.SchemaDeserialize(_foo);
        schema.AddScalarType(syntaxSchemaNode.ScalarTypes[0]);
        schema.Validate();

        Assert.Single(schema.Scalars);
        var foo = schema.Scalars["foo"];
        Assert.NotNull(foo);
        Assert.Equal("description", foo.Description);
        Assert.Equal("foo", foo.Name);
        Assert.Contains(nameof(AddScalarType), foo.Location.Source);
    }

    [Fact]
    public void NameAlreadyDefined()
    {
        try
        {
            var schema = new Schema();
            schema.Merge(_foo);
            schema.Merge(_foo);
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Scalar 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}


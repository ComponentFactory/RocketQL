namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Scalar
{
    [Fact]
    public void AddScalarType()
    {
        var schema = new Schema();
        schema.Add("scalar foo");
        schema.Validate();

        Assert.Single(schema.Types);
        var foo = schema.Types["foo"] as ScalarTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Contains(nameof(AddScalarType), foo.Location.Source);
    }

    [Fact]
    public void ScalarNameAlreadyDefined()
    {
        try
        {
            var schema = new Schema();
            schema.Add("scalar foo");
            schema.Add("scalar foo");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Scalar name 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void ScalarNameDoubleUnderscore()
    {
        try
        {
            var schema = new Schema();
            schema.Add("scalar __foo");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Scalar name '__foo' not allowed to start with two underscores.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void UndefinedDirective()
    {
        try
        {
            var schema = new Schema();
            schema.Add("scalar foo @example");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Undefined directive 'example' defined on scalar 'foo'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}


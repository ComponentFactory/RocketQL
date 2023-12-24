namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Input
{

    [Fact]
    public void AddInputObjectType()
    {
        var schema = new Schema();
        schema.Add("input foo { fizz: Int buzz : String }");
        schema.Validate();

        var foo = schema.Types["foo"] as InputObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.InputFields.Count);
        Assert.NotNull(foo.InputFields["fizz"]);
        Assert.NotNull(foo.InputFields["buzz"]);
        Assert.Contains(nameof(AddInputObjectType), foo.Location.Source);
    }

    [Fact]
    public void InputObjectNameAlreadyDefined()
    {
        try
        {
            var schema = new Schema();
            schema.Add("input foo { fizz: Int buzz : String }");
            schema.Add("input foo { fizz: Int buzz : String }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Input object name 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void InputObjectNameDoubleUnderscore()
    {
        try
        {
            var schema = new Schema();
            schema.Add("input __foo { fizz : Int buzz : String } ");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Input object name '__foo' not allowed to start with two underscores.", ex.Message);
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
            schema.Add("input foo @example { fizz : Integer }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Undefined directive 'example' defined on input object 'foo'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}


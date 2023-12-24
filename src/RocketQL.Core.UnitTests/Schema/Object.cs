namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Object
{
    [Fact]
    public void AddObjectType()
    {
        var schema = new Schema();
        schema.Add("type foo { fizz : Integer buzz : String } ");
        schema.Validate();

        Assert.Single(schema.Types);
        var foo = schema.Types["foo"] as ObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Equal(2, foo.Fields.Count);
        Assert.NotNull(foo.Fields["fizz"]);
        Assert.NotNull(foo.Fields["buzz"]);
        Assert.Contains(nameof(AddObjectType), foo.Location.Source);
    }

    [Fact]
    public void ObjectNameAlreadyDefined()
    {
        try
        {
            var schema = new Schema();
            schema.Add("type foo { fizz : Integer buzz : String } ");
            schema.Add("type foo { fizz : Integer buzz : String } ");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Object name 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void ObjectNameDoubleUnderscore()
    {
        try
        {
            var schema = new Schema();
            schema.Add("type __foo { fizz : Integer buzz : String } ");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Object name '__foo' not allowed to start with two underscores.", ex.Message);
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
            schema.Add("type foo @example { fizz : Integer }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Undefined directive 'example' defined on object 'foo'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void UndefinedInterface()
    {
        try
        {
            var schema = new Schema();
            schema.Add("type foo implements example { fizz : Integer }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Undefined interface 'example' defined on object 'foo'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void InterfaceIsNotAnInterfaceType()
    {
        try
        {
            var schema = new Schema();
            schema.Add("scalar example type foo implements example { fizz : Integer }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Cannot implement interface 'example' defined on object 'foo' because it is a 'scalar'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}


namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Interface
{
    [Fact]
    public void AddInterfaceType()
    {
        var schema = new Schema();
        schema.Add("interface foo { fizz : Int }");
        schema.Validate();

        var foo = schema.Types["foo"] as InterfaceTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Single(foo.Fields);
        Assert.NotNull(foo.Fields["fizz"]);
        Assert.Contains(nameof(AddInterfaceType), foo.Location.Source);
    }

    [Fact]
    public void InterfaceNameAlreadyDefined()
    {
        try
        {
            var schema = new Schema();
            schema.Add("interface foo { fizz : Integer }");
            schema.Add("interface foo { fizz : Integer }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal($"Interface name 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void InterfaceNameDoubleUnderscore()
    {
        try
        {
            var schema = new Schema();
            schema.Add("interface __foo { fizz : Int }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Interface name '__foo' not allowed to start with two underscores.", ex.Message);
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
            schema.Add("interface foo @example { fizz : Integer }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Undefined directive 'example' defined on interface 'foo'.", ex.Message);
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
            schema.Add("interface foo implements example { fizz : Integer }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Undefined interface 'example' defined on interface 'foo'.", ex.Message);
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
            schema.Add("scalar example interface foo implements example { fizz : Integer }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Cannot implement interface 'example' defined on interface 'foo' because it is a 'scalar'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}


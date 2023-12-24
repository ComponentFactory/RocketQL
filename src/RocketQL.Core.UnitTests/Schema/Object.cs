namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Object
{
    [Fact]
    public void AddObjectType()
    {
        var schema = new Schema();
        schema.Add("type foo { fizz : Int buzz : String } ");
        schema.Validate();

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
            schema.Add("type __foo { fizz : Int buzz : String } ");
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
            schema.Add("type foo implements Int { fizz : Int }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Cannot implement interface 'Int' defined on object 'foo' because it is a 'scalar'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void FieldTypeUndefined()
    {
        try
        {
            var schema = new Schema();
            schema.Add("type foo { fizz : Buzz }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Undefined type 'Buzz' for field 'fizz' of object 'foo'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}


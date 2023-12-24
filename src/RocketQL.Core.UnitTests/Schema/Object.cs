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
            schema.Add("type foo { fizz : Int buzz : String } ");
            schema.Add("type foo { fizz : Int buzz : String } ");
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
            schema.Add("type foo @example { fizz : Int }");
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
    public void UndefinedDirectiveOnField()
    {
        try
        {
            var schema = new Schema();
            schema.Add("type foo { fizz : Int @example }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Undefined directive 'example' defined on field 'fizz' of object 'foo'.", ex.Message);
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
            schema.Add("type foo implements example { fizz : Int }");
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

    [Fact]
    public void ArgumentEntryDoubleUnderscore()
    {
        try
        {
            var schema = new Schema();
            schema.Add("type foo { fizz(__arg1: String): String }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Directive 'foo' has argument name '__arg1' not allowed to start with two underscores.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Theory]
    [InlineData("arg1: String, arg1: String")]
    [InlineData("arg1: String, arg2: String, arg1: String")]
    [InlineData("arg2: String, arg1: String, arg1: String")]
    public void ArgumentEntryDuplicateName(string arguments)
    {
        try
        {
            var schema = new Schema();
            schema.Add($"type foo {{ fizz({arguments}): String }}");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Field 'fizz' has duplicate argument name 'arg1'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void ArgumentUndefinedType()
    {
        try
        {
            var schema = new Schema();
            schema.Add("type foo { fizz(arg1: Buzz) : String }");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Undefined type 'Buzz' for argument 'arg1' of object 'foo'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

}


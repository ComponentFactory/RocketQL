namespace RocketQL.Core.UnitTests.SchemaValidation;

public class Directive
{
    [Fact]
    public void AddDirective()
    {
        var schema = new Schema();
        schema.Add("directive @foo on ENUM");
        schema.Validate();

        Assert.Single(schema.Directives);
        var foo = schema.Directives["foo"];
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Contains(nameof(AddDirective), foo.Location.Source);
    }

    [Fact]
    public void DirectiveNameAlreadyDefined()
    {
        try
        {
            var schema = new Schema();
            schema.Add("directive @foo on ENUM");
            schema.Add("directive @foo on ENUM");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Directive name 'foo' is already defined.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }

    [Fact]
    public void DirectiveNameDoubleUnderscore()
    {
        try
        {
            var schema = new Schema();
            schema.Add("directive @__foo on ENUM");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Directive name '__foo' not allowed to start with two underscores.", ex.Message);
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
            schema.Add("directive @foo(__arg1: String) on ENUM");
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
            schema.Add($"directive @foo({arguments}) on ENUM");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Directive 'foo' has duplicate argument name 'arg1'.", ex.Message);
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
            schema.Add("directive @foo(arg1: Fizz) on ENUM");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Undefined type 'Fizz' for argument 'arg1' of directive 'foo'.", ex.Message);
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
            schema.Add("directive @foo(arg1: String @example) on ENUM");
            schema.Validate();

            Assert.Fail("Exception expected");
        }
        catch (ValidationException ex)
        {
            Assert.Equal("Undefined directive 'example' defined on argument 'arg1' of directive 'foo'.", ex.Message);
        }
        catch
        {
            Assert.Fail("Wrong exception");
        }
    }
}


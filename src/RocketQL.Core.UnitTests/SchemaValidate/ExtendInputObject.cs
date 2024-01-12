namespace RocketQL.Core.UnitTests.SchemaValidation;

public class ExtendInputObject : UnitTestBase
{
    [Theory]
    [InlineData("""
                type Query { alpha: Int }
                extend input foo { buzz: Int }
                """,                                            "Input object 'foo' cannot be extended because it is not defined.")]
    [InlineData("""
                type Query { alpha: Int }
                extend input foo { buzz: Int }
                input foo { first: Int }
                """,                                            "Input object 'foo' cannot be extended because it is not defined.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @bar on INPUT_OBJECT
                input foo @bar { buzz: Int } 
                extend input foo @bar
                """,                                            "Directive '@bar' is not repeatable but has been applied multiple times on input object 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                input foo { buzz: Int } 
                extend input foo { fizz: Int fizz: Int} 
                """,                                            "Extend input object 'foo' has duplicate definition of input field 'fizz'.")]
    [InlineData("""
                type Query { alpha: Int }
                input foo { buzz: Int } 
                extend input foo { buzz: Int } 
                """,                                            "Extend input object 'foo' for existing input field 'buzz' does not make any change.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @bar on INPUT_FIELD_DEFINITION
                input foo { buzz: Int @bar } 
                extend input foo { buzz: Int } 
                """,                                            "Extend input object 'foo' for existing input field 'buzz' does not make any change.")]
    public void ValidationSingleExceptions(string schemaText, string message)
    {
        SchemaValidationSingleException(schemaText, message);
    }

    [Fact]
    public void AddDirectiveToInputObject()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   directive @bar on INPUT_OBJECT
                   input foo { fizz: Int }
                   extend input foo @bar                
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as InputObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Single(foo.Directives);
        var directive = foo.Directives[0];
        Assert.NotNull(directive);
        Assert.Equal("@bar", directive.Name);
    }

    [Fact]
    public void AddDirectiveToInputField()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   directive @bar on INPUT_FIELD_DEFINITION
                   input foo { fizz: Int }
                   extend input foo { fizz: Int @bar }               
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as InputObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Single(foo.InputFields);
        var fizz = foo.InputFields["fizz"];
        Assert.NotNull(fizz);
        Assert.Equal("fizz", fizz.Name);
        Assert.Single(fizz.Directives);
        var directive = fizz.Directives[0];
        Assert.NotNull(directive);
        Assert.Equal("@bar", directive.Name);
    }

    [Fact]
    public void AddInputField()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   directive @bar on INPUT_FIELD_DEFINITION
                   input foo { buzz: Int }
                   extend input foo { fizz: Int @bar }       
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as InputObjectTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        var fizz = foo.InputFields["fizz"];
        Assert.NotNull(fizz);
        Assert.Equal("fizz", fizz.Name);
        var directive = fizz.Directives[0];
        Assert.NotNull(directive);
        Assert.Equal("@bar", directive.Name);
    }

}


namespace RocketQL.Core.UnitTests.SchemaValidation;

public class ExtendInterface : UnitTestBase
{
    [Theory]
    [InlineData("""
                type Query { alpha: Int }
                extend interface foo { buzz: Int }
                """,                                            "Interface 'foo' cannot be extended because it is not defined.")]
    [InlineData("""
                type Query { alpha: Int }
                extend interface foo { buzz: Int }
                interface foo { first: Int }
                """,                                            "Interface 'foo' cannot be extended because it is not defined.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @bar on INTERFACE
                interface foo @bar { buzz: Int } 
                extend interface foo @bar
                """,                                            "Directive '@bar' is not repeatable but has been applied multiple times on interface 'foo'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface bar { buzz: Int }
                interface foo implements bar { buzz: Int } 
                extend interface foo implements bar
                """,                                            "Extend interface 'foo' specifies an interface 'bar' already defined.")]
    [InlineData("""
                type Query { alpha: Int }
                interface foo { buzz: Int } 
                extend interface foo { fizz: Int fizz: Int} 
                """,                                            "Extend interface 'foo' has duplicate definition of field 'fizz'.")]
    [InlineData("""
                type Query { alpha: Int }
                interface foo { buzz: Int } 
                extend interface foo { buzz: Int } 
                """,                                            "Extend interface 'foo' for existing field 'buzz' does not make any change.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @bar on FIELD_DEFINITION
                interface foo { buzz: Int @bar } 
                extend interface foo { buzz: Int } 
                """,                                            "Extend interface 'foo' for existing field 'buzz' does not make any change.")]
    [InlineData("""
                type Query { alpha: Int }
                directive @bar on ARGUMENT_DEFINITION
                interface foo { buzz(arg: String @bar): Int } 
                extend interface foo { buzz(arg: String): Int } 
                """,                                            "Extend interface 'foo' for existing field 'buzz' does not make any change.")]
    public void ValidationSingleExceptions(string schemaText, string message)
    {
        SchemaValidationSingleException(schemaText, message);
    }

    [Fact]
    public void AddDirectiveToInterface()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   directive @bar on INTERFACE
                   interface foo { fizz: Int }
                   extend interface foo @bar                
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as InterfaceTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Single(foo.Directives);
        var directive = foo.Directives[0];
        Assert.NotNull(directive);
        Assert.Equal("@bar", directive.Name);
    }

    [Fact]
    public void AddDirectiveToField()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   directive @bar on FIELD_DEFINITION
                   interface foo { fizz: Int }
                   extend interface foo { fizz: Int @bar }               
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as InterfaceTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Single(foo.Fields);
        var fizz = foo.Fields["fizz"];
        Assert.NotNull(fizz);
        Assert.Equal("fizz", fizz.Name);
        Assert.Single(fizz.Directives);
        var directive = fizz.Directives[0];
        Assert.NotNull(directive);
        Assert.Equal("@bar", directive.Name);
    }

    [Fact]
    public void AddDirectiveToArgument()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   directive @bar on ARGUMENT_DEFINITION
                   interface foo { fizz(arg: String): Int }
                   extend interface foo { fizz(arg: String @bar): Int }             
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as InterfaceTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        Assert.Single(foo.Fields);
        var fizz = foo.Fields["fizz"];
        Assert.NotNull(fizz);
        Assert.Equal("fizz", fizz.Name);
        Assert.Single(fizz.Arguments);
        var agument = fizz.Arguments["arg"];
        Assert.NotNull(agument);
        Assert.Equal("arg", agument.Name);
        Assert.Single(agument.Directives);
        var directive = agument.Directives[0];
        Assert.NotNull(directive);
        Assert.Equal("@bar", directive.Name);
    }

    [Fact]
    public void AddType()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   interface bar { buzz: Int }
                   interface foo { buzz: Int }
                   extend interface foo implements bar        
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as InterfaceTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        var bar = foo.ImplementsInterfaces["bar"];
        Assert.NotNull(bar);
        Assert.Equal("bar", bar.Name);
    }

    [Fact]
    public void AddField()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   directive @bar on FIELD_DEFINITION | ARGUMENT_DEFINITION
                   interface foo { buzz: Int }
                   extend interface foo { fizz(arg: Int @bar): Int @bar }       
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as InterfaceTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        var fizz = foo.Fields["fizz"];
        Assert.NotNull(fizz);
        Assert.Equal("fizz", fizz.Name);
        var directive1 = fizz.Directives[0];
        Assert.NotNull(directive1);
        Assert.Equal("@bar", directive1.Name);
        var argument = fizz.Arguments["arg"];
        Assert.NotNull(argument);
        Assert.Equal("arg", argument.Name);
        var directive2 = argument.Directives[0];
        Assert.NotNull(directive2);
        Assert.Equal("@bar", directive2.Name);
    }

    [Fact]
    public void AddArgument()
    {
        var schema = new Schema();
        schema.Add("""
                   type Query { fizz: Int }
                   directive @bar on FIELD_DEFINITION | ARGUMENT_DEFINITION
                   interface foo { buzz: Int }
                   extend interface foo { buzz(arg: String @bar): Int }       
                   """);
        schema.Validate();

        var foo = schema.Types["foo"] as InterfaceTypeDefinition;
        Assert.NotNull(foo);
        Assert.Equal("foo", foo.Name);
        var buzz = foo.Fields["buzz"];
        Assert.NotNull(buzz);
        Assert.Equal("buzz", buzz.Name);
        var argument = buzz.Arguments["arg"];
        Assert.NotNull(argument);
        Assert.Equal("arg", argument.Name);
        var directive = argument.Directives[0];
        Assert.NotNull(directive);
        Assert.Equal("@bar", directive.Name);
    }
}


namespace RocketQL.Core.UnitTests.SchemaValidation;

public class InputOutputTypes : UnitTestBase
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ObjectTypeDefinitionOutputOnly(bool innerNonNull, bool outerNonNull)
    {
        var node = new ObjectTypeDefinition()
        {
            Description = "",
            Name = "",
            ImplementsInterfaces = [],
            Directives = [],
            Fields = [],
            Location = new Location()
        };

        CheckInAndOutsideList(node, innerNonNull, outerNonNull, false, true);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void InterfaceTypeDefinitionOutputOnly(bool innerNonNull, bool outerNonNull)
    {
        var node = new InterfaceTypeDefinition()
        {
            Description = "",
            Name = "",
            ImplementsInterfaces = [],
            Directives = [],
            Fields = [],
            Location = new Location()
        };

        CheckInAndOutsideList(node, innerNonNull, outerNonNull, false, true);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void UnionTypeDefinitionOutputOnly(bool innerNonNull, bool outerNonNull)
    {
        var node = new UnionTypeDefinition()
        {
            Description = "",
            Name = "",
            Directives = [],
            MemberTypes = [],
            Location = new Location()
        };

        CheckInAndOutsideList(node, innerNonNull, outerNonNull, false, true);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void InputObjectTypeDefinitionInputOnly(bool innerNonNull, bool outerNonNull)
    {
        var node = new InputObjectTypeDefinition()
        {
            Description = "",
            Name = "",
            Directives = [],
            InputFields = [],
            Location = new Location()
        };

        CheckInAndOutsideList(node, innerNonNull, outerNonNull, true, false);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ScalarTypeDefinitionBoth(bool innerNonNull, bool outerNonNull)
    {
        var node = new ScalarTypeDefinition()
        {
            Description = "",
            Name = "",
            Directives = [],
            Location = new Location()
        };

        CheckInAndOutsideList(node, innerNonNull, outerNonNull, true, true);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void EnumTypeDefinitionBoth(bool innerNonNull, bool outerNonNull)
    {
        var node = new EnumTypeDefinition()
        {
            Description = "",
            Name = "",
            Directives = [],
            EnumValues = [],
            Location = new Location()
        };

        CheckInAndOutsideList(node, innerNonNull, outerNonNull, true, true);
    }

    private static void CheckInAndOutsideList(TypeDefinition typeDefinition, bool innerNonNull, bool outerNonNull, bool input, bool output)
    {
        Assert.True(typeDefinition.IsInputType == input);
        Assert.True(typeDefinition.IsOutputType == output);

        var listNode = new TypeList()
        {
            Type = new TypeName()
            {
                Name = "",
                NonNull = innerNonNull,
                Definition = typeDefinition,
                Location = new()
            },
            NonNull = outerNonNull,
            Location = new()
        };

        Assert.True(listNode.IsInputType == input);
        Assert.True(listNode.IsOutputType == output);
    }
}


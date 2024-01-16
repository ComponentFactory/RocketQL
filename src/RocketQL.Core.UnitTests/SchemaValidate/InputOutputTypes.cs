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
        var node = new ObjectTypeDefinition("", "", [], [], [], Location.Empty);
        CheckInAndOutsideList(node, innerNonNull, outerNonNull, false, true);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void InterfaceTypeDefinitionOutputOnly(bool innerNonNull, bool outerNonNull)
    {
        var node = new InterfaceTypeDefinition("", "", [], [], [], Location.Empty);
        CheckInAndOutsideList(node, innerNonNull, outerNonNull, false, true);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void UnionTypeDefinitionOutputOnly(bool innerNonNull, bool outerNonNull)
    {
        var node = new UnionTypeDefinition("", "", [], [], Location.Empty);
        CheckInAndOutsideList(node, innerNonNull, outerNonNull, false, true);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void InputObjectTypeDefinitionInputOnly(bool innerNonNull, bool outerNonNull)
    {
        var node = new InputObjectTypeDefinition("", "", [], [], Location.Empty);
        CheckInAndOutsideList(node, innerNonNull, outerNonNull, true, false);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ScalarTypeDefinitionBoth(bool innerNonNull, bool outerNonNull)
    {
        var node = new ScalarTypeDefinition("", "", [], Location.Empty);
        CheckInAndOutsideList(node, innerNonNull, outerNonNull, true, true);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void EnumTypeDefinitionBoth(bool innerNonNull, bool outerNonNull)
    {
        var node = new EnumTypeDefinition("", "", [], [], Location.Empty);
        CheckInAndOutsideList(node, innerNonNull, outerNonNull, true, true);
    }

    private static void CheckInAndOutsideList(TypeDefinition typeDefinition, bool innerNonNull, bool outerNonNull, bool input, bool output)
    {
        Assert.True(typeDefinition.IsInputType == input);
        Assert.True(typeDefinition.IsOutputType == output);

        TypeNode node = new TypeName("", Location.Empty)
        {
            Definition = typeDefinition
        };

        if (innerNonNull)
            node = new TypeNonNull(node, Location.Empty);

        node = new TypeList(node, Location.Empty);

        if (outerNonNull)
            node = new TypeNonNull(node, Location.Empty);

        Assert.True(node.IsInputType == input);
        Assert.True(node.IsOutputType == output);
    }
}


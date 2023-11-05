using RocketQL.Core.Nodes;
using RocketQL.Core.Serializers;
using System.Xml.Linq;

namespace RocketQL.Core.Base;

public partial class Schema
{
    private static Directives ToDirectives(SyntaxDirectiveNodeList directives)
    {
        var nodes = new Directives();

        foreach (var directive in directives)
        {
            nodes.Add(directive.Name, new()
            {
                Name = directive.Name,
                Definition = null,
                Arguments = ToObjectFieldNodes(directive.Arguments),
                Location = directive.Location
            });
        }

        return nodes;
    }

    private static Interfaces ToInterfaces(SyntaxNameList names)
    {
        var nodes = new Interfaces();

        foreach (var name in names)
        {
            nodes.Add(name, new()
            {
                Name = name,
                Definition = null,
            });
        }

        return nodes;
    }

    private static MemberTypes ToMemberTypes(SyntaxNameList names)
    {
        var nodes = new MemberTypes();

        foreach (var name in names)
        {
            nodes.Add(name, new()
            {
                Name = name,
                Definition = null,
            });
        }

        return nodes;
    }

    private static EnumValueDefinitions ToEnumValues(SyntaxEnumValueDefinitionList enumValues)
    {
        var nodes = new EnumValueDefinitions();

        foreach (var enumValue in enumValues)
        {
            nodes.Add(enumValue.Name, new()
            {
                Description = enumValue.Description,
                Name = enumValue.Name,
                Directives = ToDirectives(enumValue.Directives),
                Location = enumValue.Location
            });
        }

        return nodes;
    }

    private static ObjectFields ToObjectFieldNodes(SyntaxObjectFieldNodeList fields)
    {
        var nodes = new ObjectFields();

        foreach (var field in fields)
            nodes.Add(field.Name, field);

        return nodes;
    }

    private static FieldDefinitions ToFieldDefinitions(SyntaxFieldDefinitionNodeList fields)
    {
        var nodes = new FieldDefinitions();

        foreach (var field in fields)
        {
            nodes.Add(field.Name, new()
            {
                Description = field.Description,
                Name = field.Name,
                Arguments = ToInputValueDefinitions(field.Arguments),
                Type = ToTypeNode(field.Type),
                Definition = null
            });
        }

        return nodes;
    }

    private static InputValueDefinitions ToInputValueDefinitions(SyntaxInputValueDefinitionNodeList inputValues)
    {
        var nodes = new InputValueDefinitions();

        foreach (var inputValue in inputValues)
        {
            nodes.Add(inputValue.Name, new()
            {
                Description = inputValue.Description,
                Name = inputValue.Name,
                Type = ToTypeNode(inputValue.Type),
                DefaultValue = inputValue.DefaultValue,
                Directives = ToDirectives(inputValue.Directives),
                Location = inputValue.Location
            });
        }

        return nodes;
    }

    private static TypeLocation ToTypeNode(SyntaxTypeNode node)
    {
        return node switch
        {
            SyntaxTypeNameNode nameNode => new TypeName() 
            {
                 Name = nameNode.Name,
                 NonNull = nameNode.NonNull,
                 Location = nameNode.Location,
            },
            SyntaxTypeListNode listNode => new TypeList() 
            {
                Type = ToTypeNode(listNode.Type),
                Definition = null,
                NonNull = listNode.NonNull,
                Location = listNode.Location,
            },
            _ => throw ValidationException.UnrecognizedType(node.Location, node.GetType().Name)
        }; ;
    }
}

namespace RocketQL.Core;

public class Schema
{
    public Schema()
    {
        SchemaNode = new(new());
    }

    public Schema(Schema other)
    {
        SchemaNode = other.SchemaNode.Clone();
    }

    public ValidatedSchemaNode SchemaNode { get; init; }

    public void Merge(IEnumerable<SchemaNode> schemas)
    {
        foreach (var schema in schemas)
            Merge(schema);
    }

    public void Merge(SchemaNode schema)
    {
        MergeDirectives(schema.Directives);
    }

    private void MergeDirectives(DirectiveDefinitionNodeList directives)
    {
        foreach(var directive in directives) 
        {
            // Each directive must have a unique name
            if (SchemaNode.Directives.ContainsKey(directive.Name))
                throw ValidationException.DirectiveAlreadyDefined(directive.Location, directive.Name);

            SchemaNode.Directives.Add(directive.Name, directive);
        }
    }
}

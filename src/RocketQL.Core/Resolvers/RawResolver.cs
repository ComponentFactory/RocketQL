namespace RocketQL.Core.Resolvers;

public class RawResolver : IRootSchemaResolver
{
    private readonly SchemaNode _schema;

    public RawResolver(string schema)
    {
        _schema = Document.SchemaDeserialize(schema);
    }
}

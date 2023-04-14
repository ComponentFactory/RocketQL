namespace RocketQL.Core.Executor;

public class RawResolver : IRootSchemaResolver
{
    private readonly TypeSystemDocumentNode _schema;

    public RawResolver(string schema)
    {
        _schema = new TypeSystemParser(schema).Parse();
    }
}

using System.Collections.Generic;
using System.Data;

namespace RocketQL.Core.Base;

public partial class Request : IRequest
{
    private readonly List<ValidationException> _exceptions = [];
    private readonly SyntaxNodeList _nodes = [];

    public ISchema? Schema { get; protected set; }
    public ValueNode? Variables { get; protected set; }
    public bool IsValidatedSchema { get; protected set; } = false;
    public bool IsValidatedVariables { get; protected set; } = false;

    public void Add(SyntaxNode node)
    {
        _nodes.Add(node);
        IsValidatedSchema = false;
    }

    public void Add(IEnumerable<SyntaxNode> nodes)
    {
        _nodes.AddRange(nodes);
        IsValidatedSchema = false;
    }

    public void Add(SyntaxNodeList nodes)
    {
        _nodes.AddRange(nodes);
        IsValidatedSchema = false;
    }

    public void Add(IEnumerable<SyntaxNodeList> schemas)
    {
        foreach (var nodes in schemas)
            _nodes.AddRange(nodes);

        IsValidatedSchema = false;
    }

    public void Add(ReadOnlySpan<char> schema, string source)
    {
        Add(Serialization.RequestDeserialize(schema, source));
    }

    public void Add(ReadOnlySpan<char> schema,
                    [CallerFilePath] string filePath = "",
                    [CallerMemberName] string memberName = "",
                    [CallerLineNumber] int lineNumber = 0)
    {
        Add(Serialization.RequestDeserialize(schema, CallerExtensions.CallerToSource(filePath, memberName, lineNumber)));
    }


    public void ValidateSchema(ISchema schema)
    {
        CleanSchema();

        if (!schema.IsValidated)
            FatalException(ValidationException.SchemaNotValidated());

        try
        {
            Schema = schema;
            Converter.Visit();
            CheckExceptions();
            IsValidatedSchema = true;
        }
        catch
        {
            CleanSchema();
            throw;
        }
    }

    public void ValidateVariables(ValueNode variables)
    {
        CleanVariables();

        if (!IsValidatedSchema)
            FatalException(ValidationException.SchemaNotValidated());

        try
        {
            Variables = variables;
            CheckExceptions();
            IsValidatedVariables = true;
        }
        catch
        {
            CleanVariables();
            throw;
        }
    }

    public void Reset()
    {
        _nodes.Clear();
        CleanSchema();
    }

    private void CleanSchema()
    {
        Schema = null;
        IsValidatedSchema = false;
        CleanVariables();
    }

    private void CleanVariables()
    {
        _exceptions.Clear();
        Variables = null;
        IsValidatedVariables = false;
    }

    private static void FatalException(ValidationException validationException)
    {
        throw validationException;
    }

    private void NonFatalException(ValidationException validationException)
    {
        _exceptions.Add(validationException);
    }

    private void CheckExceptions()
    {
        if (_exceptions.Count == 1)
            throw _exceptions[0];
        else if (_exceptions.Count > 1)
            throw new RocketExceptions(_exceptions);
    }
}

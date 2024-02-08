using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Xml.Linq;
using System;

namespace RocketQL.Core.Base;

public interface ISchemaBuilder
{
    ISchemaBuilder AddSyntaxNode(SyntaxNode node);
    ISchemaBuilder AddSyntaxNodes(IEnumerable<SyntaxNode> nodes);
    ISchemaBuilder AddSyntaxNodes(SyntaxNodeList nodes);
    ISchemaBuilder AddSyntaxNodes(IEnumerable<SyntaxNodeList> schemas);
    ISchemaBuilder AddFromString(ReadOnlySpan<char> schema, string source);
    ISchemaBuilder AddFromString(ReadOnlySpan<char> schema, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);

    ISchema Build();
}

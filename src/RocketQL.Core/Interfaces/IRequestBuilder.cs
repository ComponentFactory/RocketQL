using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Xml.Linq;
using System;

namespace RocketQL.Core.Base;

public interface IRequestBuilder
{
    IRequestBuilder AddSyntaxNode(SyntaxNode node);
    IRequestBuilder AddSyntaxNodes(IEnumerable<SyntaxNode> nodes);
    IRequestBuilder AddSyntaxNodes(SyntaxNodeList nodes);
    IRequestBuilder AddSyntaxNodes(IEnumerable<SyntaxNodeList> schemas);
    IRequestBuilder AddFromString(ReadOnlySpan<char> schema, string source);
    IRequestBuilder AddFromString(ReadOnlySpan<char> schema, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0);

    public IRequest Build(ISchema schema);
}

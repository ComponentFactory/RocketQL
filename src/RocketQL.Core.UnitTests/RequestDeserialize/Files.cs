﻿using RocketQL.Core.Serializers;

namespace RocketQL.Core.UnitTests.RequestDeserialize;

public class Files
{
    [Theory]
    [InlineData("introspection.graphql")]
    public void FileSchema(string filename)
    {
        var schema = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", filename));
        var documentNode = Document.RequestDeserialize(schema);
    }
}




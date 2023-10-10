namespace RocketQL.Core.Common;

public record struct Location(string Source, int Position, int Line, int Column);

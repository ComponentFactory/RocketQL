namespace RocketQL.Core.Common;

public record struct Location(int Position, int Line, int Column, string Source);

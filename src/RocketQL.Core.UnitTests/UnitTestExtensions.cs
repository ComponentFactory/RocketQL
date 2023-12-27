
namespace RocketQL.Core.UnitTests;

public static class UnitTestExtensions
{
    public static void IsNull<T>(this T? reference) where T : class
    {
        if (reference is not null)
            throw new ArgumentNullException("Reference should be null but is not.");
    }

    public static T NotNull<T>([NotNull] this T? reference) where T : class
    {
        if (reference is null)
            throw new ArgumentNullException("Reference should not be null but is.");

        return reference;
    }

    public static T One<T>([NotNull] this List<T> reference) where T : class
    {
        if (reference is null)
            throw NotNullException.ForNullValue();

        if (reference.Count != 1)
            throw new ArgumentNullException($"Collection should have 1 entry but has '{reference.Count}'");

        return reference[0];
    }

    public static T Count<T>([NotNull] this T reference, int count) where T : ICollection
    {
        if (reference is null)
            throw NotNullException.ForNullValue();

        if (reference.Count != count)
            throw new ArgumentNullException($"Collection should have '{count}' entries but has '{reference.Count}'");

        return reference;
    }

    public static T IsType<T>(this ValueNode? reference) where T : ValueNode
    {
        ValueNode notNull = reference.NotNull();
        return (T)notNull;
    }
}



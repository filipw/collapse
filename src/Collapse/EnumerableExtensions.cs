namespace Collapse;

internal static class EnumerableExtensions
{
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> enumerable, int batchSize)
    {
        var length = enumerable.Count();
        var index = 0;
        do
        {
            yield return enumerable.Skip(index).Take(batchSize);
            index += batchSize;
        } while (index < length);
    }
}
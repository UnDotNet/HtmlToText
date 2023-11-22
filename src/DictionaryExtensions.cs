
namespace UnDotNet.HtmlToText;

internal static class DictionaryExtensions
{
    public static Dictionary<TKey, TValue> MergeValues<TKey, TValue>(Dictionary<TKey, TValue>? source, params Dictionary<TKey, TValue>[] newValues) where TKey : notnull
    {
        source ??= new Dictionary<TKey, TValue>();
        foreach (var dictionary in newValues)
        {
            foreach (var value in dictionary)
            {
                if (source.ContainsKey(value.Key)) source.Remove(value.Key);
                source.Add(value.Key, value.Value);
            }
        }

        return source;
    }
}
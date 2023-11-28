using System.Text.RegularExpressions;

namespace UnDotNet.HtmlToText;

public static class StringExtensions
{
    public static string RemoveMultipleSpaces(this string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }
    
    public static string Join(this string[] value, char separator)
    {
#if (NETFRAMEWORK)
        return string.Join(separator.ToString(), value);
#else
        return string.Join(separator, value);
#endif
    }
    
    public static string Join(this string[] value, string separator)
    {
        return string.Join(separator, value);
    }
    
    // https://blog.nimblepros.com/blogs/repeat-string-in-csharp/
    public static string Repeat(this string text, int n)
    {
        var textAsSpan = text.AsSpan();
        var span = new Span<char>(new char[textAsSpan.Length * n]);
        for (var i = 0; i < n; i++)
        {
            textAsSpan.CopyTo(span.Slice(i * textAsSpan.Length, textAsSpan.Length));
        }

        return span.ToString();
    }
}
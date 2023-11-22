
namespace UnDotNet.HtmlToText;

internal static class Utils
{
    public static string NumberToLetterSequence(int num, char baseChar, int baseNum = 26)
    {
        var digits = new List<int>();
        do
        {
            num -= 1;
            digits.Add(num % baseNum);
            num = num / baseNum;
        } while (num > 0);
        int baseCode = baseChar;
        digits.Reverse();
        var result = new List<char>();
        foreach (var n in digits)
        {
            result.Add((char)(baseCode + n));
        }
        return new string(result.ToArray());
    }

    private static readonly string[] I = { "I", "X", "C", "M" };
    private static readonly string[] V = { "V", "L", "D" };

    public static string NumberToRoman(int num)
    {
        return string.Join("", num.ToString().ToCharArray()
            .Select(n => int.Parse(n.ToString()))
            .Reverse()
            .Select((v, i) => ((v % 5 < 4)
                ? (v < 5 ? "" : V[i]) + new string(I[i].ToCharArray()[0], v % 5)
                : I[i] + (v < 5 ? V[i] : I[i + 1])))
            .Reverse());
    }
    
}
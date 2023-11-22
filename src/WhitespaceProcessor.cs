using System.Text.RegularExpressions;

namespace UnDotNet.HtmlToText;

/**
* Helps to handle HTML whitespaces.
*
* @class WhitespaceProcessor
*/
internal class WhitespaceProcessor
{
    private string whitespaceChars;
    private string whitespaceCodes;
    private Regex leadingWhitespaceRe;
    private Regex trailingWhitespaceRe;
    public Regex allWhitespaceOrEmptyRe;
    private Regex newlineOrNonWhitespaceRe;
    private Regex newlineOrNonNewlineStringRe;

    public Action<string, InlineTextBuilder, Func<string, string>?, bool> ShrinkWrapAdd { get; private set; }

    public WhitespaceProcessor(Options options)
    {
        whitespaceChars = (options.preserveNewlines)
            ? options.whitespaceCharacters.Replace("\n", "")
            : options.whitespaceCharacters;
       
        //  \t\r\n\f
        // space
        // \u0020\u0009\u000d\u000a\u000c
        //  \t\r\f
        // \u0020\u0009\u000d\u000c
       
        whitespaceCodes = CharactersToCodes(whitespaceChars);
        leadingWhitespaceRe = new Regex($"^[{whitespaceCodes}]");
        trailingWhitespaceRe = new Regex($"[{whitespaceCodes}]$");
        allWhitespaceOrEmptyRe = new Regex($"^[{whitespaceCodes}]*$");
        newlineOrNonWhitespaceRe = new Regex($"(\n|[^\n{whitespaceCodes}])", RegexOptions.Compiled);
        newlineOrNonNewlineStringRe = new Regex("(\n|[^\n]+)", RegexOptions.Compiled);

   
        if (options.preserveNewlines)
        {
            var wordOrNewlineRe = new Regex($"\n|[^\n{whitespaceCodes}]+", RegexOptions.Compiled);
            ShrinkWrapAdd = (string text, InlineTextBuilder inlineTextBuilder, Func<string, string>? transform, bool noWrap) =>
            {
                transform ??= str => str;
                if (string.IsNullOrEmpty(text)) { return; }
                var previouslyStashedSpace = inlineTextBuilder.StashedSpace;
                var anyMatch = false;
           
                var m = wordOrNewlineRe.Matches(text);
                for (var i = 0; i < m.Count; i++)
                {
                    if (i == 0)
                    {
                        anyMatch = true;
                        if (m[i].Value == "\n")
                        {
                            inlineTextBuilder.StartNewLine();
                        }
                        else if (previouslyStashedSpace || this.testLeadingWhitespace(text))
                        {
                            inlineTextBuilder.PushWord(transform(m[i].Value), noWrap);
                        }
                        else
                        {
                            inlineTextBuilder.ConcatWord(transform(m[i].Value), noWrap);
                        }
                    }
                    else
                    {
                        if (m[i].Value == "\n")
                        {
                            inlineTextBuilder.StartNewLine();
                        }
                        else
                        {
                            inlineTextBuilder.PushWord(transform(m[i].Value), noWrap);
                        }
                    }
                }
                inlineTextBuilder.StashedSpace = (previouslyStashedSpace && !anyMatch) || (this.testTrailingWhitespace(text));
            };
        }
        else
        {
            var wordRe = new Regex($"[^{whitespaceCodes}]+", RegexOptions.Compiled);
            ShrinkWrapAdd = (string text, InlineTextBuilder inlineTextBuilder, Func<string, string>? transform, bool noWrap) =>
            {
                transform ??= str => str;
                if (string.IsNullOrEmpty(text)) { return; }
                var previouslyStashedSpace = inlineTextBuilder.StashedSpace;
                var anyMatch = false;
                var m = wordRe.Matches(text);
                for (var i = 0; i < m.Count; i++)
                {
                    if (i == 0)
                    {
                        anyMatch = true;
                        if (previouslyStashedSpace || this.testLeadingWhitespace(text))
                        {
                            inlineTextBuilder.PushWord(transform(m[i].Value), noWrap);
                        }
                        else
                        {
                            inlineTextBuilder.ConcatWord(transform(m[i].Value), noWrap);
                        }
                    }
                    else
                    {
                        inlineTextBuilder.PushWord(transform(m[i].Value), noWrap);
                    }
                }
                inlineTextBuilder.StashedSpace = (previouslyStashedSpace && !anyMatch) || this.testTrailingWhitespace(text);
            };
        }
    }
   
   
   
    /**
* Add text with only minimal processing.
* Everything between newlines considered a single word.
* No whitespace is trimmed.
* Not affected by preserveNewlines option - `\n` always starts a new line.
*
* `noWrap` argument is `true` by default - this won't start a new line
* even if there is not enough space left in the current line.
*
* @param { string }            text              Input text.
* @param { InlineTextBuilder } inlineTextBuilder A builder to receive processed text.
* @param { boolean }           [noWrap] Don't wrap text even if the line is too long.
*/
    public void addLiteral(string text, InlineTextBuilder inlineTextBuilder, bool noWrap = true)
    {
        if (string.IsNullOrEmpty(text)) { return; }
        var previouslyStashedSpace = inlineTextBuilder.StashedSpace;
        var anyMatch = false;
        var m = newlineOrNonNewlineStringRe.Match(text);
        if (m.Success)
        {
            anyMatch = true;
            if (m.Value == "\n")
            {
                inlineTextBuilder.StartNewLine();
            }
            else if (previouslyStashedSpace)
            {
                inlineTextBuilder.PushWord(m.Value, noWrap);
            }
            else
            {
                inlineTextBuilder.ConcatWord(m.Value, noWrap);
            }
            while ((m = newlineOrNonNewlineStringRe.Match(text, m.Index + m.Length)) != Match.Empty)
            {
                if (m.Value == "\n")
                {
                    inlineTextBuilder.StartNewLine();
                }
                else
                {
                    inlineTextBuilder.PushWord(m.Value, noWrap);
                }
            }
        }
        inlineTextBuilder.StashedSpace = (previouslyStashedSpace && !anyMatch);
    }
   

    /*
     * Test whether the given text starts with HTML whitespace character.
     *
     * @param   { string }  text  The string to test.
     * @returns { boolean }
     */
    public bool testLeadingWhitespace(string text)
    {
        return leadingWhitespaceRe.IsMatch(text);
    }
   
    /*
     * Test whether the given text ends with HTML whitespace character.
     *
     * @param   { string }  text  The string to test.
     * @returns { boolean }
     */
    public bool testTrailingWhitespace(string text)
    {
        return trailingWhitespaceRe.IsMatch(text);
    }
   
   
    /*
     * Test whether the given text contains any non-whitespace characters.
     *
     * @param   { string }  text  The string to test.
     * @returns { boolean }
     */
    public bool testContainsWords(string text)
    {
        //dotnet bug considers "\n" to be empty 
        if (!whitespaceChars.Contains("\n") && text == "\n") return true;
       
        return !allWhitespaceOrEmptyRe.IsMatch(text);
    }
   
    /*
     * Return the number of newlines if there are no words.
     *
     * If any word is found then return zero regardless of the actual number of newlines.
     *
     * @param   { string }  text  Input string.
     * @returns { number }
     */
    public int countNewlinesNoWords(string text)
    {
        var counter = 0;
        var m = newlineOrNonWhitespaceRe.Matches(text);
        for (var i = 0; i < m.Count; i++)
        {
            if (m[i].Value== "\n")
            {
                counter++;
            }
            else
            {
                return 0;
            }
        }
        return counter;
    }

    public static  string CharactersToCodes(string str)
    {
        var result = "";
        foreach (var c in str)
        {
            result += "\\u" + ((int)c).ToString("x4");
        }
        return result;
    }
}
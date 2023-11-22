using System.Text.RegularExpressions;

namespace UnDotNet.HtmlToText;

/**
* Helps to handle HTML whitespaces.
*
* @class WhitespaceProcessor
*/
internal class WhitespaceProcessor
{
    private readonly string _whitespaceChars;
    private readonly Regex _leadingWhitespaceRe;
    private readonly Regex _trailingWhitespaceRe;
    private readonly Regex _allWhitespaceOrEmptyRe;
    private readonly Regex _newlineOrNonWhitespaceRe;
    private readonly Regex _newlineOrNonNewlineStringRe;

    public Action<string, InlineTextBuilder, Func<string, string>?, bool> ShrinkWrapAdd { get; private set; }

    public WhitespaceProcessor(Options options)
    {
        _whitespaceChars = (options.PreserveNewlines)
            ? options.WhitespaceCharacters.Replace("\n", "")
            : options.WhitespaceCharacters;

        //  \t\r\n\f
        // space
        // \u0020\u0009\u000d\u000a\u000c
        //  \t\r\f
        // \u0020\u0009\u000d\u000c
        var whitespaceCodes = CharactersToCodes(_whitespaceChars);
        _leadingWhitespaceRe = new Regex($"^[{whitespaceCodes}]");
        _trailingWhitespaceRe = new Regex($"[{whitespaceCodes}]$");
        _allWhitespaceOrEmptyRe = new Regex($"^[{whitespaceCodes}]*$");
        _newlineOrNonWhitespaceRe = new Regex($"(\n|[^\n{whitespaceCodes}])", RegexOptions.Compiled);
        _newlineOrNonNewlineStringRe = new Regex("(\n|[^\n]+)", RegexOptions.Compiled);

   
        if (options.PreserveNewlines)
        {
            var wordOrNewlineRe = new Regex($"\n|[^\n{whitespaceCodes}]+", RegexOptions.Compiled);
            ShrinkWrapAdd = (text, inlineTextBuilder, transform, noWrap) =>
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
                        else if (previouslyStashedSpace || this.TestLeadingWhitespace(text))
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
                inlineTextBuilder.StashedSpace = (previouslyStashedSpace && !anyMatch) || (this.TestTrailingWhitespace(text));
            };
        }
        else
        {
            var wordRe = new Regex($"[^{whitespaceCodes}]+", RegexOptions.Compiled);
            ShrinkWrapAdd = (text, inlineTextBuilder, transform, noWrap) =>
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
                        if (previouslyStashedSpace || this.TestLeadingWhitespace(text))
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
                inlineTextBuilder.StashedSpace = (previouslyStashedSpace && !anyMatch) || this.TestTrailingWhitespace(text);
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
    public void AddLiteral(string text, InlineTextBuilder inlineTextBuilder, bool noWrap = true)
    {
        if (string.IsNullOrEmpty(text)) { return; }
        var previouslyStashedSpace = inlineTextBuilder.StashedSpace;
        var anyMatch = false;
        var m = _newlineOrNonNewlineStringRe.Match(text);
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
            while ((m = _newlineOrNonNewlineStringRe.Match(text, m.Index + m.Length)) != Match.Empty)
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
    private bool TestLeadingWhitespace(string text)
    {
        return _leadingWhitespaceRe.IsMatch(text);
    }
   
    /*
     * Test whether the given text ends with HTML whitespace character.
     *
     * @param   { string }  text  The string to test.
     * @returns { boolean }
     */
    private bool TestTrailingWhitespace(string text)
    {
        return _trailingWhitespaceRe.IsMatch(text);
    }
   
   
    /*
     * Test whether the given text contains any non-whitespace characters.
     *
     * @param   { string }  text  The string to test.
     * @returns { boolean }
     */
    public bool TestContainsWords(string text)
    {
        //dotnet bug considers "\n" to be empty 
        if (!_whitespaceChars.Contains('\n') && text == "\n") return true;
       
        return !_allWhitespaceOrEmptyRe.IsMatch(text);
    }
   
    /*
     * Return the number of newlines if there are no words.
     *
     * If any word is found then return zero regardless of the actual number of newlines.
     *
     * @param   { string }  text  Input string.
     * @returns { number }
     */
    public int CountNewlinesNoWords(string text)
    {
        var counter = 0;
        var m = _newlineOrNonWhitespaceRe.Matches(text);
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
namespace UnDotNet.HtmlToText;

/**
* Helps to build text from words.
*/
internal class InlineTextBuilder
{
    private readonly List<List<string>> _lines;
    private List<string> _nextLineWords;
    private int _nextLineAvailableChars;
    private readonly string _wrapCharacters;
    private readonly bool _forceWrapOnLimit;
    public bool WordBreakOpportunity { get; set; }

    public int MaxLineLength { get; }

    public bool StashedSpace { get; set; }


    /**
 * Creates an instance of InlineTextBuilder.
 *
 * If `maxLineLength` is not provided then it is either `options.wordwrap` or unlimited.
 *
 * @param { Options } options           HtmlToText options.
 * @param { number }  [ maxLineLength ] This builder will try to wrap text to fit this line length.
 */
    public InlineTextBuilder(Options options, int maxLineLength = 0)
    {
        this._lines = new List<List<string>>();
        this._nextLineWords = new List<string>();
        this.MaxLineLength = maxLineLength != 0 ? maxLineLength : options.Wordwrap is null or 0 ? int.MaxValue : options.Wordwrap.Value;
        this._nextLineAvailableChars = this.MaxLineLength;
        this._wrapCharacters = options.LongWordSplit.WrapCharacters ?? "";
        this._forceWrapOnLimit = options.LongWordSplit.ForceWrapOnLimit ?? false;
        this.StashedSpace = false;
        this.WordBreakOpportunity = false;
    }

    /**
 * Add a new word.
 *
 * @param { string } word A word to add.
 * @param { boolean } [noWrap] Don't wrap text even if the line is too long.
 */
    public void PushWord(string word, bool noWrap = false)
    {
        if (this._nextLineAvailableChars <= 0 && !noWrap)
        {
            this.StartNewLine();
        }
        var isLineStart = this._nextLineWords.Count == 0;
        var cost = word.Length + (isLineStart ? 0 : 1);
        // Fits into available budget
        if ((cost <= this._nextLineAvailableChars) || noWrap)
        {
            this._nextLineWords.Add(word);
            this._nextLineAvailableChars -= cost;
        }
        // Does not fit - try to split the word
        else
        {
                
            // The word is moved to a new line - prefer to wrap between words.
            var words = this.SplitLongWord(word);
            if (!isLineStart)
            {
                this.StartNewLine();
            }
            this._nextLineWords.Add(words[0]);
            this._nextLineAvailableChars -= words[0].Length;
            foreach (var part in words.Skip(1))
            {
                this.StartNewLine();
                this._nextLineWords.Add(part);
                this._nextLineAvailableChars -= part.Length;
            }
        }
    }

    /**
 * Pop a word from the currently built line.
 * This doesn't affect completed lines.
 *
 * @returns { string }
 */
    private string? PopWord()
    {
        if (_nextLineWords.Count == 0) return null;
        var lastWord = _nextLineWords[^1];
        _nextLineWords.RemoveAt(_nextLineWords.Count - 1);
        var isLineStart = this._nextLineWords.Count == 0;
        var cost = lastWord.Length + (isLineStart ? 0 : 1);
        this._nextLineAvailableChars += cost;
        return lastWord;
    }

    /**
 * Concat a word to the last word already in the builder.
 * Adds a new word in case there are no words yet in the last line.
 *
 * @param { string } word A word to be concatenated.
 * @param { boolean } [noWrap] Don't wrap text even if the line is too long.
 */
    public void ConcatWord(string word, bool noWrap = false)
    {
        if (this.WordBreakOpportunity && word.Length > this._nextLineAvailableChars)
        {
            this.PushWord(word, noWrap);
            this.WordBreakOpportunity = false;
        }
        else
        {
            var lastWord = this.PopWord();
            this.PushWord((lastWord != null) ? lastWord + word : word, noWrap);
        }
    }

    /**
 * Add current line (and more empty lines if provided argument > 1) to the list of complete lines and start a new one.
 *
 * @param { number } n Number of line breaks that will be added to the resulting string.
 */
    public void StartNewLine(int n = 1)
    {
        this._lines.Add(this._nextLineWords);
        if (n > 1)
        {
            for (var i = 0; i < n - 1; i++)
            {
                this._lines.Add(new List<string>());
            }
        }
        this._nextLineWords = new List<string>();
        this._nextLineAvailableChars = this.MaxLineLength;
    }

    /**
 * No words in this builder.
 *
 * @returns { boolean }
 */
    public bool IsEmpty()
    {
        return this._lines.Count == 0 && this._nextLineWords.Count == 0;
    }

    public void Clear()
    {
        this._lines.Clear();
        this._nextLineWords.Clear();
        this._nextLineAvailableChars = this.MaxLineLength;
    }

    /**
 * Join all lines of words inside the InlineTextBuilder into a complete string.
 *
 * @returns { string }
 */
    public override string ToString()
    {
        var result = new List<string>();
        foreach (var words in this._lines)
        {
            result.Add(string.Join(" ", words));
        }
        result.Add(string.Join(" ", this._nextLineWords));
        return string.Join("\n", result);
    }

    /**
 * Split a long word up to fit within the word wrap limit.
 * Use either a character to split looking back from the word wrap limit,
 * or truncate to the word wrap limit.
 *
 * @param   { string }   word Input word.
 * @returns { string[] }      Parts of the word.
 */
    private List<string> SplitLongWord(string word)
    {
        var parts = new List<string>();
        var idx = 0;
        while (word.Length > this.MaxLineLength)
        {
            var firstLine = word.Substring(0, this.MaxLineLength);
            var remainingChars = word.Substring(this.MaxLineLength);
                
            var splitIndex = _wrapCharacters.Length > 0 ? firstLine.LastIndexOf(_wrapCharacters[idx]) : -1;
            if (splitIndex > -1)
            {
                word = firstLine.Substring(splitIndex + 1) + remainingChars;
                parts.Add(firstLine.Substring(0, splitIndex + 1));
            }
            else
            {
                idx++;
                if (idx < this._wrapCharacters.Length)
                {
                    word = firstLine + remainingChars;
                }
                else
                {
                    if (this._forceWrapOnLimit)
                    {
                        parts.Add(firstLine);
                        word = remainingChars;
                        if (word.Length > this.MaxLineLength)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        word = firstLine + remainingChars;
                    }
                    break;
                }
            }
        }
        parts.Add(word);
        return parts;
    }
}
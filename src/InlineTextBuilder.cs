namespace UnDotNet.HtmlToText;

/**
* Helps to build text from words.
*/
internal class InlineTextBuilder
{
    private List<List<string>> lines;
    private List<string> nextLineWords;
    private int nextLineAvailableChars;
    private string wrapCharacters;
    private bool forceWrapOnLimit;
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
        this.lines = new List<List<string>>();
        this.nextLineWords = new List<string>();
        this.MaxLineLength = maxLineLength != 0 ? maxLineLength : options.wordwrap is null or 0 ? int.MaxValue : options.wordwrap.Value;
        this.nextLineAvailableChars = this.MaxLineLength;
        this.wrapCharacters = options.longWordSplit.wrapCharacters ?? "";
        this.forceWrapOnLimit = options.longWordSplit.forceWrapOnLimit ?? false;
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
        if (this.nextLineAvailableChars <= 0 && !noWrap)
        {
            this.StartNewLine();
        }
        bool isLineStart = this.nextLineWords.Count == 0;
        int cost = word.Length + (isLineStart ? 0 : 1);
        // Fits into available budget
        if ((cost <= this.nextLineAvailableChars) || noWrap)
        {
            this.nextLineWords.Add(word);
            this.nextLineAvailableChars -= cost;
        }
        // Does not fit - try to split the word
        else
        {
                
            // The word is moved to a new line - prefer to wrap between words.
            List<string> words = this.SplitLongWord(word);
            if (!isLineStart)
            {
                this.StartNewLine();
            }
            this.nextLineWords.Add(words[0]);
            this.nextLineAvailableChars -= words[0].Length;
            foreach (string part in words.Skip(1))
            {
                this.StartNewLine();
                this.nextLineWords.Add(part);
                this.nextLineAvailableChars -= part.Length;
            }
        }
    }

    /**
 * Pop a word from the currently built line.
 * This doesn't affect completed lines.
 *
 * @returns { string }
 */
    public string? PopWord()
    {
        if (nextLineWords.Count == 0) return null;
        var lastWord = nextLineWords[^1];
        nextLineWords.RemoveAt(nextLineWords.Count - 1);
        bool isLineStart = this.nextLineWords.Count == 0;
        int cost = lastWord.Length + (isLineStart ? 0 : 1);
        this.nextLineAvailableChars += cost;
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
        if (this.WordBreakOpportunity && word.Length > this.nextLineAvailableChars)
        {
            this.PushWord(word, noWrap);
            this.WordBreakOpportunity = false;
        }
        else
        {
            string lastWord = this.PopWord();
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
        this.lines.Add(this.nextLineWords);
        if (n > 1)
        {
            for (int i = 0; i < n - 1; i++)
            {
                this.lines.Add(new List<string>());
            }
        }
        this.nextLineWords = new List<string>();
        this.nextLineAvailableChars = this.MaxLineLength;
    }

    /**
 * No words in this builder.
 *
 * @returns { boolean }
 */
    public bool IsEmpty()
    {
        return this.lines.Count == 0 && this.nextLineWords.Count == 0;
    }

    public void Clear()
    {
        this.lines.Clear();
        this.nextLineWords.Clear();
        this.nextLineAvailableChars = this.MaxLineLength;
    }

    /**
 * Join all lines of words inside the InlineTextBuilder into a complete string.
 *
 * @returns { string }
 */
    public override string ToString()
    {
        List<string> result = new List<string>();
        foreach (List<string> words in this.lines)
        {
            result.Add(string.Join(" ", words));
        }
        result.Add(string.Join(" ", this.nextLineWords));
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
    public List<string> SplitLongWord(string word)
    {
        List<string> parts = new List<string>();
        int idx = 0;
        while (word.Length > this.MaxLineLength)
        {
            string firstLine = word.Substring(0, this.MaxLineLength);
            string remainingChars = word.Substring(this.MaxLineLength);
                
            int splitIndex = wrapCharacters?.Length > 0 ? firstLine.LastIndexOf(wrapCharacters[idx]) : -1;
            if (splitIndex > -1)
            {
                word = firstLine.Substring(splitIndex + 1) + remainingChars;
                parts.Add(firstLine.Substring(0, splitIndex + 1));
            }
            else
            {
                idx++;
                if (idx < this.wrapCharacters.Length)
                {
                    word = firstLine + remainingChars;
                }
                else
                {
                    if (this.forceWrapOnLimit)
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
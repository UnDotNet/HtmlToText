using AngleSharp.Css.Dom;
using AngleSharp.Html.Dom;

namespace UnDotNet.HtmlToText;

public interface IHtmlToTextBuilder
{
    
}

public class BlockTextBuilder : IHtmlToTextBuilder
{
    public readonly Dictionary<string, string>? metaData;
    public Options Options { get; }
    private WhitespaceProcessor whitespaceProcessor;
    private IStackItem _stackItem;
    private TransformerStackItem? _wordTransformer;
    public Dictionary<ISelector, Selector> CssRules = new Dictionary<ISelector, Selector>();

    public Selector PickSelector(IHtmlElement element)
    {
        var results = CssRules.Where(m => m.Key.Match(element)).OrderByDescending(m => m.Key.Specificity);
        return results.First().Value;
    }

    public BlockTextBuilder(Options options, Dictionary<string, string>? metaData = null)
    {
        this.metaData = metaData;
        this.Options = options;
        this.whitespaceProcessor = new WhitespaceProcessor(options);
        this._stackItem = new BlockStackItem(options);
        this._wordTransformer = null;
    }
    /**
       * Put a word-by-word transform function onto the transformations stack.
       *
       * Mainly used for uppercasing. Can be bypassed to add unformatted text such as URLs.
       *
       * Word transformations applied before wrapping.
       *
       * @param { (str: string) => string } wordTransform Word transformation function.
       */
    public void pushWordTransform(Func<string, string> wordTransform)
    {
        this._wordTransformer = new TransformerStackItem(this._wordTransformer, wordTransform);
    }

    /**
* Remove a function from the word transformations stack.
*
* @returns { (str: string) => string } A function that was removed.
*/
    public Func<string, string> popWordTransform()
    {
        if (this._wordTransformer is null) { return null; }
        var transform = this._wordTransformer.Transform;
        this._wordTransformer = this._wordTransformer.Next;
        return transform;
    }

    public void startNoWrap()
    {
        this._stackItem.IsNoWrap = true;
    }

    public void stopNoWrap()
    {
        this._stackItem.IsNoWrap = false;
    }

    private Func<string, string>? _getCombinedWordTransformer()
    {
        Func<string, string>? wt = _wordTransformer is not null ? (string str) => applyTransformer(str, _wordTransformer) : null;
        var ce = this.Options.encodeCharacters;
        if (wt is null) return ce;
        if (ce is null) return wt;
        return (string str) => ce(wt(str));
    }

    private IStackItem _popStackItem()
    {
        var item = this._stackItem;
        this._stackItem = item.Next;
        return item;
    }

    /**
* Add a line break into currently built block.
*/
    public void addLineBreak()
    {
        if (this._stackItem is ITextStackItem textStackItem)
        {
            if (textStackItem.IsPre)
            {
                textStackItem.RawText += '\n';
            }
            else
            {
                textStackItem.InlineTextBuilder.StartNewLine();
            }
                
        }
    }

    /**
* Allow to break line in case directly following text will not fit.
*/
    public void addWordBreakOpportunity()
    {
        if (this._stackItem is ITextStackItem textStackItem)
        {
            textStackItem.InlineTextBuilder.WordBreakOpportunity = true;
        }
    }

    /**
* Add a node inline into the currently built block.
*
* @param { string } str
* Text content of a node to add.
*
* @param { object } [param1]
* Object holding the parameters of the operation.
*
* @param { boolean } [param1.noWordTransform]
* Ignore word transformers if there are any.
* Don't encode characters as well.
* (Use this for things like URL addresses).
*/
    public void addInline(string str, bool noWordTransform = false)
    {
        if (this._stackItem is not ITextStackItem textStackItem) return;
        if (textStackItem.IsPre)
        {
            textStackItem.RawText += str;
            return;
        }
        if (
            str.Length == 0 || // empty string
            (
                textStackItem.StashedLineBreaks > 0 && // stashed linebreaks make whitespace irrelevant
                !this.whitespaceProcessor.testContainsWords(str) // no words to add
            )
        ) { return; }
        if (this.Options.preserveNewlines)
        {
            var newlinesNumber = this.whitespaceProcessor.countNewlinesNoWords(str);
            if (newlinesNumber > 0)
            {
                textStackItem.InlineTextBuilder.StartNewLine(newlinesNumber);
                // keep stashedLineBreaks unchanged
                return;
            }
        }
        if (textStackItem.StashedLineBreaks > 0)
        {
            textStackItem.InlineTextBuilder.StartNewLine(textStackItem.StashedLineBreaks);
        }
        this.whitespaceProcessor.ShrinkWrapAdd(
            str,
            textStackItem.InlineTextBuilder,
            noWordTransform ? null : _getCombinedWordTransformer(),
            this._stackItem.IsNoWrap
        );
        textStackItem.StashedLineBreaks = 0; // inline text doesn't introduce line breaks
    }

    /**
* Add a string inline into the currently built block.
*
* Use this for markup elements that don't have to adhere
* to text layout rules.
*
* @param { string } str Text to add.
*/
    public void addLiteral(string str)
    {
        if (this._stackItem is not ITextStackItem textStackItem) return;
        if (str.Length == 0) { return; }
        if (textStackItem.IsPre)
        {
            textStackItem.RawText += str;
            return;
        }
        if (textStackItem.StashedLineBreaks > 0)
        {
            textStackItem.InlineTextBuilder.StartNewLine(textStackItem.StashedLineBreaks);
        }
        this.whitespaceProcessor.addLiteral(
            str,
            textStackItem.InlineTextBuilder,
            this._stackItem.IsNoWrap
        );
        textStackItem.StashedLineBreaks = 0;
    }

    /**
* Start building a new block.
*
* @param { object } [param0]
* Object holding the parameters of the block.
*
* @param { number } [param0.leadingLineBreaks]
* This block should have at least this number of line breaks to separate it from any preceding block.
*
* @param { number }  [param0.reservedLineLength]
* Reserve this number of characters on each line for block markup.
*
* @param { boolean } [param0.isPre]
* Should HTML whitespace be preserved inside this block.
*/
    public void openBlock(int leadingLineBreaks = 1, int reservedLineLength = 0, bool isPre = false )
    {
        if (_stackItem is not ITextStackItem textStackItem) return;
        var maxLineLength = Math.Max(20, textStackItem.InlineTextBuilder.MaxLineLength - reservedLineLength);
        this._stackItem = new BlockStackItem(
            this.Options,
            this._stackItem,
            leadingLineBreaks,
            maxLineLength
        );
        if (isPre) { this._stackItem.IsPre = true; }
    }

    /**
* Finalize currently built block, add it's content to the parent block.
*
* @param { object } [param0]
* Object holding the parameters of the block.
*
* @param { number } [param0.trailingLineBreaks]
* This block should have at least this number of line breaks to separate it from any following block.
*
* @param { (str: string) => string } [param0.blockTransform]
* A function to transform the block text before adding to the parent block.
* This happens after word wrap and should be used in combination with reserved line length
* in order to keep line lengths correct.
* Used for whole block markup.
*/
    public void closeBlock(int trailingLineBreaks = 1, Func<string, string>? blockTransform = null)
    {
        if (_popStackItem() is not ITextStackItem block)
        {
            throw new Exception("close block called with no text stack item");
        }
        // var block = this._popStackItem() as IStackItem;
        var blockText = blockTransform is not null ? blockTransform(GetText(block)) : GetText(block);
        AddText(_stackItem, blockText, block.LeadingLineBreaks, Math.Max(block.StashedLineBreaks, trailingLineBreaks));
    }

    /**
* Start building a new list.
*
* @param { object } [param0]
* Object holding the parameters of the list.
*
* @param { number } [param0.maxPrefixLength]
* Length of the longest list item prefix.
* If not supplied or too small then list items won't be aligned properly.
*
* @param { 'left' | 'right' } [param0.prefixAlign]
* Specify how prefixes of different lengths have to be aligned
* within a column.
*
* @param { number } [param0.interRowLineBreaks]
* Minimum number of line breaks between list items.
*
* @param { number } [param0.leadingLineBreaks]
* This list should have at least this number of line breaks to separate it from any preceding block.
*/
    public void openList(int maxPrefixLength = 0, string prefixAlign = "left", int interRowLineBreaks = 1, int leadingLineBreaks = 2)
    {
        if (_stackItem is not ITextStackItem textStackItem)
        {
            throw new Exception("open list called without a text stack item");

        }
        this._stackItem = new ListStackItem(this.Options, this._stackItem, 
            interRowLineBreaks: interRowLineBreaks,
            leadingLineBreaks: leadingLineBreaks,
            maxLineLength: textStackItem.InlineTextBuilder.MaxLineLength,
            maxPrefixLength: maxPrefixLength,
            prefixAlign: prefixAlign
        );
    }

    /**
* Start building a new list item.
*
* @param {object} param0
* Object holding the parameters of the list item.
*
* @param { string } [param0.prefix]
* Prefix for this list item (item number, bullet point, etc).
*/
    public void openListItem(string prefix = "")
    {
        if (!(this._stackItem is ListStackItem))
        {
            throw new Exception("Can't add a list item to something that is not a list! Check the formatter.");
        }
        var list = (ListStackItem)this._stackItem;
        var prefixLength = Math.Max(prefix.Length, list.MaxPrefixLength);
        var maxLineLength = Math.Max(20, list.InlineTextBuilder.MaxLineLength - prefixLength);
        this._stackItem = new ListItemStackItem(this.Options, list,
            prefix: prefix, maxLineLength: maxLineLength, leadingLineBreaks: list.InterRowLineBreaks);
    }

    /**
* Finalize currently built list item, add it's content to the parent list.
*/
    public void closeListItem()
    {
        var listItem = this._popStackItem() as ListItemStackItem;
        var list = (ListStackItem)listItem.Next;
        var prefixLength = Math.Max(listItem.Prefix.Length, list.MaxPrefixLength);
        var spacing = '\n' + new string(' ', prefixLength);
        var prefix = (list.PrefixAlign == "right")
            ? listItem.Prefix.PadLeft(prefixLength)
            : listItem.Prefix.PadRight(prefixLength);
        var text = prefix + GetText(listItem).Replace("\n", spacing);
        AddText(
            list,
            text,
            listItem.LeadingLineBreaks,
            Math.Max(listItem.StashedLineBreaks, list.InterRowLineBreaks)
        );
    }

    /**
* Finalize currently built list, add it's content to the parent block.
*
* @param { object } param0
* Object holding the parameters of the list.
*
* @param { number } [param0.trailingLineBreaks]
* This list should have at least this number of line breaks to separate it from any following block.
*/
    public void closeList(int trailingLineBreaks = 2)
    {
        if (this._popStackItem() is not ListStackItem list) return;
        var text = GetText(list);
        if (text != null)
        {
            AddText(this._stackItem, text, list.LeadingLineBreaks, trailingLineBreaks);
        }
    }

    /**
* Start building a table.
*/
    public void openTable()
    {
        this._stackItem = new TableStackItem(this._stackItem);
    }

    /**
* Start building a table row.
*/
    public void openTableRow()
    {
        if (!(this._stackItem is TableStackItem))
        {
            throw new Exception("Can't add a table row to something that is not a table! Check the formatter.");
        }
        this._stackItem = new TableRowStackItem(this._stackItem);
    }
        
    /**
       * Start building a table cell.
       *
       * @param { object } [param0]
       * Object holding the parameters of the cell.
       *
       * @param { number } [param0.maxColumnWidth]
       * Wrap cell content to this width. Fall back to global wordwrap value if undefined.
       */
    public void openTableCell(int maxColumnWidth = 0)
    {
        if (!(this._stackItem is TableRowStackItem))
        {
            throw new Exception("Can't add a table cell to something that is not a table row! Check the formatter.");
        }
        this._stackItem = new TableCellStackItem(this.Options, this._stackItem, maxColumnWidth);
    }

    /**
* Finalize currently built table cell and add it to parent table row's cells.
*
* @param { object } [param0]
* Object holding the parameters of the cell.
*
* @param { number } [param0.colspan] How many columns this cell should occupy.
* @param { number } [param0.rowspan] How many rows this cell should occupy.
*/
    public void closeTableCell(int colspan = 1, int rowspan = 1)
    {
        var cell = this._popStackItem();
        var text = GetText(cell).Trim('\n');
        if (cell.Next is not TableRowStackItem row) return;
        row.Cells.Add(new TablePrinterCell{ colspan = colspan, rowspan = rowspan, text = text });
    }

    /**
* Finalize currently built table row and add it to parent table's rows.
*/
    public void closeTableRow()
    {
        if (this._popStackItem() is not TableRowStackItem row) return;
        if (row.Next is not TableStackItem table) return;
        table.Rows.Add(row.Cells);
    }

    /**
* Finalize currently built table and add the rendered text to the parent block.
*
* @param { object } param0
* Object holding the parameters of the table.
*
* @param { TablePrinter } param0.tableToString
* A function to convert a table of stringified cells into a complete table.
*
* @param { number } [param0.leadingLineBreaks]
* This table should have at least this number of line breaks to separate if from any preceding block.
*
* @param { number } [param0.trailingLineBreaks]
* This table should have at least this number of line breaks to separate it from any following block.
*/
    public void closeTable(Func<List<List<TablePrinterCell>>,int, int, string> tableToString, int leadingLineBreaks = 2, int trailingLineBreaks = 2)
    {
        var table = this._popStackItem() as TableStackItem;
        var output = tableToString(table.Rows, 0, 0);
        if (output != null)
        {
            AddText(this._stackItem, output, leadingLineBreaks, trailingLineBreaks);
        }
    }

    public override string ToString()
    {
        return GetText(this._stackItem.GetRoot());
    }

    private string GetText(IStackItem stackItem)
    {
        if (stackItem is not ITextStackItem textStackItem)
        {
            throw new Exception("Only blocks, list items and table cells can contain text.");
        }
        return (textStackItem.InlineTextBuilder.IsEmpty())
            ? textStackItem.RawText
            : textStackItem.RawText + textStackItem.InlineTextBuilder.ToString();
    }

    private void AddText(IStackItem stackItem, string text, int leadingLineBreaks, int trailingLineBreaks)
    {
        if (stackItem is not ITextStackItem textStackItem)
        {
            throw new Exception("Only blocks, list items and table cells can contain text.");
        }
        var parentText = GetText(textStackItem);
        var lineBreaks = Math.Max(textStackItem.StashedLineBreaks, leadingLineBreaks);
        textStackItem.InlineTextBuilder.Clear();
        if (!string.IsNullOrWhiteSpace(parentText))
        {
            textStackItem.RawText = parentText + new string('\n', lineBreaks) + text;
        }
        else
        {
            textStackItem.RawText = text;
            textStackItem.LeadingLineBreaks = lineBreaks;
        }
        textStackItem.StashedLineBreaks = trailingLineBreaks;
    }

    private string applyTransformer(string str, TransformerStackItem? transformer)
    {
        return transformer is not null ? applyTransformer(transformer.Transform(str), transformer.Next) : str;
    }
}
namespace UnDotNet.HtmlToText;

internal interface IStackItem
{
    IStackItem Next { get; set; }
    bool IsPre { get; set; }
    bool IsNoWrap { get; set; }
    IStackItem GetRoot();
}

internal interface ITextStackItem : IStackItem
{
    InlineTextBuilder InlineTextBuilder { get; set; }
    string RawText { get; set; }
    int StashedLineBreaks { get; set; }
    int LeadingLineBreaks { get; set; }
}

internal abstract class StackItem : IStackItem
{
    public StackItem(IStackItem? next)
    {
        Next = next;
    }
    public virtual IStackItem? Next { get; set; }
    public bool IsPre { get; set; }
    public bool IsNoWrap { get; set; }

    public IStackItem GetRoot()
    {
        return (Next != null) ? Next : this;
    }
}

internal class BlockStackItem : StackItem, ITextStackItem
{
    public int LeadingLineBreaks { get; set; }
    public InlineTextBuilder InlineTextBuilder { get; set; }
    public string RawText { get; set; }
    public int StashedLineBreaks { get; set; }

    public BlockStackItem(Options options, IStackItem next = null, int leadingLineBreaks = 1, int maxLineLength = 0)
        : base(next)
    {
        LeadingLineBreaks = leadingLineBreaks;
        InlineTextBuilder = new InlineTextBuilder(options, maxLineLength);
        RawText = "";
        StashedLineBreaks = 0;
        IsPre = next != null && next.IsPre;
        IsNoWrap = next != null && next.IsNoWrap;
    }
}


internal class ListStackItem : BlockStackItem
{
    public int MaxPrefixLength { get; set; }
    public string PrefixAlign { get; set; }
    public int InterRowLineBreaks { get; set; }

    public ListStackItem(Options options, IStackItem next = null, int interRowLineBreaks = 1, int leadingLineBreaks = 2, int maxLineLength = 0, int maxPrefixLength = 0, string prefixAlign = "left")
        : base(options, next, leadingLineBreaks, maxLineLength)
    {
        MaxPrefixLength = maxPrefixLength;
        PrefixAlign = prefixAlign;
        InterRowLineBreaks = interRowLineBreaks;
    }
}

internal class ListItemStackItem : BlockStackItem
{
    public string Prefix { get; set; }

    public ListItemStackItem(Options options, BlockStackItem next = null, int leadingLineBreaks = 1, int maxLineLength = 0, string prefix = "")
        : base(options, next, leadingLineBreaks, maxLineLength)
    {
        Prefix = prefix;
    }
}

internal class TableStackItem : StackItem
{
    public List<List<TablePrinterCell>> Rows { get; set; }

    public TableStackItem(IStackItem? next = null)
        : base(next)
    {
        Rows = new List<List<TablePrinterCell>>();
        IsPre = next != null && next.IsPre;
        IsNoWrap = next != null && next.IsNoWrap;
    }
}

internal class TableRowStackItem : StackItem
{
    public List<TablePrinterCell> Cells { get; set; }
    public TableRowStackItem(IStackItem? next = null)
        : base(next)
    {
        Cells = new List<TablePrinterCell>();
        IsPre = next != null && next.IsPre;
        IsNoWrap = next != null && next.IsNoWrap;
    }
}

internal class TableCellStackItem : StackItem, ITextStackItem
{
    public InlineTextBuilder InlineTextBuilder { get; set; }
    public string RawText { get; set; }
    public int StashedLineBreaks { get; set; }
        
    public int LeadingLineBreaks { get; set; }

    public TableCellStackItem(Options options, IStackItem? next = null, int maxColumnWidth = 0)
        : base(next)
    {
        InlineTextBuilder = new InlineTextBuilder(options, maxColumnWidth);
        RawText = "";
        StashedLineBreaks = 0;
        IsPre = next != null && next.IsPre;
        IsNoWrap = next != null && next.IsNoWrap;
    }
}

internal class TransformerStackItem 
{

    public Func<string, string>? Transform { get; set; }
    public TransformerStackItem? Next { get; set; }

    public TransformerStackItem GetRoot()
    {
        return Next ?? this;
    }

    public TransformerStackItem(TransformerStackItem? next = null, Func<string, string>? transform = null)
    {
        Next = next;
        Transform = transform;
    }
}
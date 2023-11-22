// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
// ReSharper disable RedundantEmptyObjectOrCollectionInitializer

namespace UnDotNet.HtmlToText;

public class HtmlToTextOptions : Options
{
    public HtmlToTextOptions()
    {
        BaseElements = new()
        {
            Selectors = new() { "body" },
            OrderBy = "selectors",
            ReturnDomByDefault = true
        };
        EncodeCharacters = null;
        Formatters = new();
        Limits = new()
        {
            Ellipsis = "...",
            MaxBaseElements = null,
            MaxChildNodes = null,
            MaxDepth = null,
            MaxInputLength = 1 << 24 // int.MaxValue
        };
        LongWordSplit = new()
        {
            ForceWrapOnLimit = false,
            WrapCharacters = null
        };
        PreserveNewlines = false;
        Selectors = GetDefaultSelectors();
        WhitespaceCharacters = " \t\r\n\f\u200b";
        Wordwrap = 80;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static List<Selector> GetDefaultSelectors()
    {
        
        var list = new List<Selector>
        {
            new () {Identifier = "*", Format = "inline"},
            new () {Identifier = "script", Format = "skip" },
            new () {Identifier = "style", Format = "skip" },
            new () {Identifier = "a", Format = "anchor", Options = new ()
            {
                BaseUrl = null,
                HideLinkHrefIfSameAsText = false,
                IgnoreHref = false,
                LinkBrackets = new () {Left="[", Right= "]"},
                NoAnchorUrl = true
            }},
            new () { Identifier = "article", Format = "block", Options = new () { LeadingLineBreaks = 1, TrailingLineBreaks = 1 } },
            new () { Identifier = "aside", Format = "block", Options = new() { LeadingLineBreaks = 1, TrailingLineBreaks = 1 } },
            new () { Identifier = "blockquote", Format = "blockquote", Options = new FormatOptions { LeadingLineBreaks = 2, TrailingLineBreaks = 2, TrimEmptyLines = true } },
            new () { Identifier = "br", Format = "lineBreak"},
            new () { Identifier = "div", Format = "block", Options = new FormatOptions { LeadingLineBreaks = 1, TrailingLineBreaks = 1 } },
            new () { Identifier = "footer", Format = "block", Options = new FormatOptions { LeadingLineBreaks = 1, TrailingLineBreaks = 1 } },
            new () { Identifier = "form", Format = "block", Options = new FormatOptions { LeadingLineBreaks = 1, TrailingLineBreaks = 1 } },
            new () { Identifier = "h1", Format = "heading", Options = new FormatOptions { LeadingLineBreaks = 3, TrailingLineBreaks = 2, Uppercase = true } },
            new () { Identifier = "h2", Format = "heading", Options = new FormatOptions { LeadingLineBreaks = 3, TrailingLineBreaks = 2, Uppercase = true } },
            new () { Identifier = "h3", Format = "heading", Options = new FormatOptions { LeadingLineBreaks = 3, TrailingLineBreaks = 2, Uppercase = true } },
            new () { Identifier = "h4", Format = "heading", Options = new FormatOptions { LeadingLineBreaks = 2, TrailingLineBreaks = 2, Uppercase = true } },
            new () { Identifier = "h5", Format = "heading", Options = new FormatOptions { LeadingLineBreaks = 2, TrailingLineBreaks = 2, Uppercase = true } },
            new () { Identifier = "h6", Format = "heading", Options = new FormatOptions { LeadingLineBreaks = 2, TrailingLineBreaks = 2, Uppercase = true } },
            new () { Identifier = "header", Format = "block", Options = new FormatOptions { LeadingLineBreaks = 1, TrailingLineBreaks = 1 } },
            new () { Identifier = "hr", Format = "horizontalLine", Options = new FormatOptions { LeadingLineBreaks = 2, Length = null, TrailingLineBreaks = 2 } },
            new () { Identifier = "img", Format = "image", Options = new FormatOptions { BaseUrl = null, LinkBrackets = new () {Left="[", Right= "]"} } },
            new () { Identifier = "main", Format = "block", Options = new FormatOptions { LeadingLineBreaks = 1, TrailingLineBreaks = 1 } },
            new () { Identifier = "nav", Format = "block", Options = new FormatOptions { LeadingLineBreaks = 1, TrailingLineBreaks = 1 } },
            new () { Identifier = "ol", Format = "orderedList", Options = new FormatOptions { LeadingLineBreaks = 2, TrailingLineBreaks = 2 } },
            new () { Identifier = "p", Format = "paragraph", Options = new FormatOptions { LeadingLineBreaks = 2, TrailingLineBreaks = 2 } },
            new () { Identifier = "pre", Format = "pre", Options = new FormatOptions { LeadingLineBreaks = 2, TrailingLineBreaks = 2 } },
            new () { Identifier = "section", Format = "block", Options = new FormatOptions { LeadingLineBreaks = 1, TrailingLineBreaks = 1 } },
            new () { Identifier = "table", Format = "table", Options = new FormatOptions
            {
                ColSpacing = 3,
                LeadingLineBreaks = 2,
                MaxColumnWidth = 60,
                RowSpacing = 0,
                TrailingLineBreaks = 2,
                UppercaseHeaderCells = true
            }},
            new () { Identifier = "ul", Format = "unorderedList", Options = new FormatOptions { ItemPrefix = " * ", LeadingLineBreaks = 2, TrailingLineBreaks = 2 } },
            new () { Identifier = "wbr", Format = "wbr" }
        };

        return list; //.ToDictionary(item => item.selector);
    }

    public Selector Selector(string query)
    {
        return Selectors.FirstOrDefault(s => s.Identifier == query) ?? new Selector() {Identifier = query,Format = "", Options = new FormatOptions()};
    }
    
    public Selector Default => Selector(HtmlToText.Selectors.Default);
    public Selector Script => Selector(HtmlToText.Selectors.Script);
    public Selector Style => Selector(HtmlToText.Selectors.Style);
    public Selector A => Selector(HtmlToText.Selectors.A);
    public Selector Article => Selector(HtmlToText.Selectors.Article);
    public Selector Aside => Selector(HtmlToText.Selectors.Aside);
    public Selector BlockQuote => Selector(HtmlToText.Selectors.BlockQuote);
    public Selector Br => Selector(HtmlToText.Selectors.Br);
    public Selector Div => Selector(HtmlToText.Selectors.Div);
    public Selector Footer => Selector(HtmlToText.Selectors.Footer);
    public Selector Form => Selector(HtmlToText.Selectors.Form);
    public Selector H1 => Selector(HtmlToText.Selectors.H1);
    public Selector H2 => Selector(HtmlToText.Selectors.H2);
    public Selector H3 => Selector(HtmlToText.Selectors.H3);
    public Selector H4 => Selector(HtmlToText.Selectors.H4);
    public Selector H5 => Selector(HtmlToText.Selectors.H5);
    public Selector H6 => Selector(HtmlToText.Selectors.H6);
    public Selector Header => Selector(HtmlToText.Selectors.Header);
    public Selector Hr => Selector(HtmlToText.Selectors.Hr);
    public Selector Img => Selector(HtmlToText.Selectors.Img);
    public Selector Main => Selector(HtmlToText.Selectors.Main);
    public Selector Nav => Selector(HtmlToText.Selectors.Nav);
    public Selector Ol => Selector(HtmlToText.Selectors.Ol);
    public Selector P => Selector(HtmlToText.Selectors.P);
    public Selector Pre => Selector(HtmlToText.Selectors.Pre);
    public Selector Section => Selector(HtmlToText.Selectors.Section);
    public Selector Table => Selector(HtmlToText.Selectors.Table);
    public Selector Ul => Selector(HtmlToText.Selectors.Ul);
    public Selector Wbr => Selector(HtmlToText.Selectors.Wbr);
}

public static class Selectors
{
    public static readonly string Default = "*";
    public static readonly string Script = "script";
    public static readonly string Style = "style";
    public static readonly string A = "a";
    public static readonly string Article = "article";
    public static readonly string Aside = "aside";
    public static readonly string BlockQuote = "blockquote";
    public static readonly string Br = "br";
    public static readonly string Div = "div";
    public static readonly string Footer = "footer";
    public static readonly string Form = "form";
    public static readonly string H1 = "h1";
    public static readonly string H2 = "h2";
    public static readonly string H3 = "h3";
    public static readonly string H4 = "h4";
    public static readonly string H5 = "h5";
    public static readonly string H6 = "h6";
    public static readonly string Header = "header";
    public static readonly string Hr = "hr";
    public static readonly string Img = "img";
    public static readonly string Main = "main";
    public static readonly string Nav = "nav";
    public static readonly string Ol = "ol";
    public static readonly string P = "p";
    public static readonly string Pre = "pre";
    public static readonly string Section = "section";
    public static readonly string Table = "table";
    public static readonly string Ul = "ul";
    public static readonly string Wbr = "wbr";
}
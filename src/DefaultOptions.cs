// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
// ReSharper disable RedundantEmptyObjectOrCollectionInitializer

namespace UnDotNet.HtmlToText;

public class HtmlToTextOptions : Options
{
    public HtmlToTextOptions()
    {
        BaseElements = new()
        {
            selectors = new() { "body" },
            orderBy = "selectors",
            returnDomByDefault = true
        };
        encodeCharacters = null;
        formatters = new();
        limits = new()
        {
            ellipsis = "...",
            maxBaseElements = null,
            maxChildNodes = null,
            maxDepth = null,
            maxInputLength = 1 << 24 // int.MaxValue
        };
        longWordSplit = new()
        {
            forceWrapOnLimit = false,
            wrapCharacters = null
        };
        preserveNewlines = false;
        selectors = GetDefaultSelectors();
        whitespaceCharacters = " \t\r\n\f\u200b";
        wordwrap = 80;
    }

    public static List<Selector> GetDefaultSelectors()
    {
        
        var list = new List<Selector>
        {
            new () {selector = "*", format = "inline"},
            new () {selector = "script", format = "skip" },
            new () {selector = "style", format = "skip" },
            new () {selector = "a", format = "anchor", options = new ()
            {
                baseUrl = null,
                hideLinkHrefIfSameAsText = false,
                ignoreHref = false,
                linkBrackets = new () {left="[", right= "]"},
                noAnchorUrl = true
            }},
            new () { selector = "article", format = "block", options = new () { leadingLineBreaks = 1, trailingLineBreaks = 1 } },
            new () { selector = "aside", format = "block", options = new() { leadingLineBreaks = 1, trailingLineBreaks = 1 } },
            new () { selector = "blockquote", format = "blockquote", options = new FormatOptions { leadingLineBreaks = 2, trailingLineBreaks = 2, trimEmptyLines = true } },
            new () { selector = "br", format = "lineBreak"},
            new () { selector = "div", format = "block", options = new FormatOptions { leadingLineBreaks = 1, trailingLineBreaks = 1 } },
            new () { selector = "footer", format = "block", options = new FormatOptions { leadingLineBreaks = 1, trailingLineBreaks = 1 } },
            new () { selector = "form", format = "block", options = new FormatOptions { leadingLineBreaks = 1, trailingLineBreaks = 1 } },
            new () { selector = "h1", format = "heading", options = new FormatOptions { leadingLineBreaks = 3, trailingLineBreaks = 2, uppercase = true } },
            new () { selector = "h2", format = "heading", options = new FormatOptions { leadingLineBreaks = 3, trailingLineBreaks = 2, uppercase = true } },
            new () { selector = "h3", format = "heading", options = new FormatOptions { leadingLineBreaks = 3, trailingLineBreaks = 2, uppercase = true } },
            new () { selector = "h4", format = "heading", options = new FormatOptions { leadingLineBreaks = 2, trailingLineBreaks = 2, uppercase = true } },
            new () { selector = "h5", format = "heading", options = new FormatOptions { leadingLineBreaks = 2, trailingLineBreaks = 2, uppercase = true } },
            new () { selector = "h6", format = "heading", options = new FormatOptions { leadingLineBreaks = 2, trailingLineBreaks = 2, uppercase = true } },
            new () { selector = "header", format = "block", options = new FormatOptions { leadingLineBreaks = 1, trailingLineBreaks = 1 } },
            new () { selector = "hr", format = "horizontalLine", options = new FormatOptions { leadingLineBreaks = 2, length = null, trailingLineBreaks = 2 } },
            new () { selector = "img", format = "image", options = new FormatOptions { baseUrl = null, linkBrackets = new () {left="[", right= "]"} } },
            new () { selector = "main", format = "block", options = new FormatOptions { leadingLineBreaks = 1, trailingLineBreaks = 1 } },
            new () { selector = "nav", format = "block", options = new FormatOptions { leadingLineBreaks = 1, trailingLineBreaks = 1 } },
            new () { selector = "ol", format = "orderedList", options = new FormatOptions { leadingLineBreaks = 2, trailingLineBreaks = 2 } },
            new () { selector = "p", format = "paragraph", options = new FormatOptions { leadingLineBreaks = 2, trailingLineBreaks = 2 } },
            new () { selector = "pre", format = "pre", options = new FormatOptions { leadingLineBreaks = 2, trailingLineBreaks = 2 } },
            new () { selector = "section", format = "block", options = new FormatOptions { leadingLineBreaks = 1, trailingLineBreaks = 1 } },
            new () { selector = "table", format = "table", options = new FormatOptions
            {
                colSpacing = 3,
                leadingLineBreaks = 2,
                maxColumnWidth = 60,
                rowSpacing = 0,
                trailingLineBreaks = 2,
                uppercaseHeaderCells = true
            }},
            new () { selector = "ul", format = "unorderedList", options = new FormatOptions { itemPrefix = " * ", leadingLineBreaks = 2, trailingLineBreaks = 2 } },
            new () { selector = "wbr", format = "wbr" }
        };

        return list; //.ToDictionary(item => item.selector);
    }

    public Selector Selector(string query)
    {
        return selectors.FirstOrDefault(s => s.selector == query) ?? new Selector() {selector = query,format = "", options = new FormatOptions()};
    }
    
    public Selector Default => Selector(Selectors.Default);
    public Selector Script => Selector(Selectors.Script);
    public Selector Style => Selector(Selectors.Style);
    public Selector A => Selector(Selectors.A);
    public Selector Article => Selector(Selectors.Article);
    public Selector Aside => Selector(Selectors.Aside);
    public Selector BlockQuote => Selector(Selectors.BlockQuote);
    public Selector Br => Selector(Selectors.Br);
    public Selector Div => Selector(Selectors.Div);
    public Selector Footer => Selector(Selectors.Footer);
    public Selector Form => Selector(Selectors.Form);
    public Selector H1 => Selector(Selectors.H1);
    public Selector H2 => Selector(Selectors.H2);
    public Selector H3 => Selector(Selectors.H3);
    public Selector H4 => Selector(Selectors.H4);
    public Selector H5 => Selector(Selectors.H5);
    public Selector H6 => Selector(Selectors.H6);
    public Selector Header => Selector(Selectors.Header);
    public Selector Hr => Selector(Selectors.Hr);
    public Selector Img => Selector(Selectors.Img);
    public Selector Main => Selector(Selectors.Main);
    public Selector Nav => Selector(Selectors.Nav);
    public Selector Ol => Selector(Selectors.Ol);
    public Selector P => Selector(Selectors.P);
    public Selector Pre => Selector(Selectors.Pre);
    public Selector Section => Selector(Selectors.Section);
    public Selector Table => Selector(Selectors.Table);
    public Selector Ul => Selector(Selectors.Ul);
    public Selector Wbr => Selector(Selectors.Wbr);
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
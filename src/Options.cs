// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable MemberCanBeProtected.Global

using AngleSharp.Dom;

namespace UnDotNet.HtmlToText;

public class Options
{
    public BaseElementsOptions BaseElements { get; set; } = new();
    public Func<string, string>? EncodeCharacters { get; set; }
    public Dictionary<string, FormatCallback> Formatters { get; set; } = new();
    public LimitsOptions Limits { get; set; } = new();
    public LongWordSplitOptions LongWordSplit { get; set; } = new();
    public bool PreserveNewlines { get; set; }
    public List<Selector> Selectors { get; set; } = new();
    public string WhitespaceCharacters { get; set; } = "";
    public int? Wordwrap { get; set; }
}

public class BaseElementsOptions
{
    public List<string> Selectors { get; set; } = new();
    public string OrderBy { get; set; } = "";
    public bool ReturnDomByDefault { get; set; } = true;
}

public class LimitsOptions
{
    public string Ellipsis { get; set; } = "...";
    public int? MaxBaseElements { get; set; }
    public int? MaxChildNodes { get; set; }
    public int? MaxDepth { get; set; }
    public int MaxInputLength { get; set; } = 1 << 24;
}

public class LongWordSplitOptions
{
    public bool? ForceWrapOnLimit { get; set; }
    public string? WrapCharacters { get; set; }
}

public class Selector
{
    public string Identifier { get; set; } = "";
    public string Format { get; set; } = "";
    public FormatOptions Options { get; set; } = new FormatOptions();
}

public class BracketOptions
{
    public string Left { get; set; } = "[";
    public string Right { get; set; } = "]";
}

public class FormatOptions
{
    public int? LeadingLineBreaks { get; set; }
    public int? TrailingLineBreaks { get; set; }
    public string? BaseUrl { get; set; }
    public bool HideLinkHrefIfSameAsText { get; set; }
    public bool IgnoreHref { get; set; }
    public BracketOptions? LinkBrackets { get; set; }
    public bool NoAnchorUrl { get; set; }
    public string ItemPrefix { get; set; } = "";
    public bool Uppercase { get; set; }
    public int? Length { get; set; }
    public bool TrimEmptyLines { get; set; }
    public bool UppercaseHeaderCells { get; set; } = true;
    public int MaxColumnWidth { get; set; }
    public int? ColSpacing { get; set; }
    public int? RowSpacing { get; set; }
    public string StringLiteral { get; set; } = "";
    public string Prefix { get; set; } = "";
    public string Suffix { get; set; } = "";
    public RewriterCallback? PathRewrite { get; set; }
    public Dictionary<string, object> ExtraOptions { get; set; } = new();
}
    
public delegate string? RewriterCallback(string path, Dictionary<string, string>? metaData, IElement? elem);
public delegate void FormatCallback(IElement elem, RecursiveCallback walk, BlockTextBuilder builder, FormatOptions formatOptions);
public delegate void RecursiveCallback(RecursiveCallback walk, IEnumerable<INode> nodes, BlockTextBuilder builder);
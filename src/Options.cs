// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable MemberCanBeProtected.Global

using AngleSharp.Dom;

namespace UnDotNet.HtmlToText;

public class Options
{
    public BaseElementsOptions BaseElements { get; set; } = new();
    public Func<string, string>? encodeCharacters { get; set; }
    public Dictionary<string, FormatCallback> formatters { get; set; } = new();
    public LimitsOptions limits { get; set; }
    public LongWordSplitOptions longWordSplit { get; set; }
    public bool preserveNewlines { get; set; }
    public List<Selector> selectors { get; set; }
    public string whitespaceCharacters { get; set; }
    public int? wordwrap { get; set; }
}

public class BaseElementsOptions
{
    public List<string> selectors { get; set; } = new();
    public string orderBy { get; set; }
    public bool returnDomByDefault { get; set; } = true;
}

public class LimitsOptions
{
    public string ellipsis { get; set; } = "...";
    public int? maxBaseElements { get; set; }
    public int? maxChildNodes { get; set; }
    public int? maxDepth { get; set; }
    public int maxInputLength { get; set; } = 1 << 24;
}

public class LongWordSplitOptions
{
    public bool? forceWrapOnLimit { get; set; }
    public string? wrapCharacters { get; set; }
}

public class Selector
{
    public string selector { get; set; }
    public string format { get; set; }
    public FormatOptions options { get; set; } = new FormatOptions();
}

public class BracketOptions
{
    public string left { get; set; } = "[";
    public string right { get; set; } = "]";
}

public class FormatOptions
{
    public int? leadingLineBreaks { get; set; }
    public int? trailingLineBreaks { get; set; }
    public string? baseUrl { get; set; }
    public bool hideLinkHrefIfSameAsText { get; set; }
    public bool ignoreHref { get; set; }
    public BracketOptions? linkBrackets { get; set; }
    public bool noAnchorUrl { get; set; }
    public string itemPrefix { get; set; }
    public bool uppercase { get; set; }
    public int? length { get; set; }
    public bool trimEmptyLines { get; set; }
    public bool uppercaseHeaderCells { get; set; } = true;
    public int maxColumnWidth { get; set; }
    public int? colSpacing { get; set; }
    public int? rowSpacing { get; set; }
    public string stringLiteral { get; set; }
    public string prefix { get; set; }
    public string suffix { get; set; }
    public RewriterCallback? pathRewrite { get; set; }
    public Dictionary<string, object> ExtraOptions { get; set; } = new();
}
    
public delegate string? RewriterCallback(string path, Dictionary<string, string>? metaData, IElement? elem);
public delegate void FormatCallback(IElement elem, RecursiveCallback walk, BlockTextBuilder builder, FormatOptions formatOptions);
public delegate void RecursiveCallback(RecursiveCallback walk, IEnumerable<INode> nodes, BlockTextBuilder builder);
using System.Text.RegularExpressions;
using AngleSharp.Dom;
// ReSharper disable UnusedParameter.Local

namespace UnDotNet.HtmlToText.Tests;

[TestClass]
public class CustomFormattingTests
{
    private static string HtmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
        new HtmlToTextConverter().Convert(html, options, metadata);
    
    [TestMethod]
    public void ShouldAllowToOverrideFormattingOfExistingTags()
    {
        const string html = "<h1>TeSt</h1><h1>mOrE tEsT</h1>";
        var options = new HtmlToTextOptions
        {
            Formatters =
            {
                ["heading"] = (elem, walk, builder, formatOptions) =>
                {
                    builder.OpenBlock(leadingLineBreaks: 2);
                    builder.PushWordTransform(str => str.ToLower());
                    walk(walk, elem.ChildNodes, builder);
                    builder.PopWordTransform();
                    builder.CloseBlock(trailingLineBreaks: 2, str => 
                    {
                        var line = new string('=', str.Length);
                        return $"{line}\n{str}\n{line}";
                    });
                }
            }
        };
        var expected = "====\ntest\n====\n\n=========\nmore test\n=========";
        HtmlToText(html, options).ShouldBe(expected);
    }

    [TestMethod]
    public void ShouldAllowToSkipTagsWithDummyFormattingFunction()
    {
        var html = "<ruby>漢<rt>かん</rt>字<rt>じ</rt></ruby>";
        var expected = "漢字";
        var options = new HtmlToTextOptions();
        options.Selectors.Add(new Selector() {Identifier = "rt", Format = "skip"});
        HtmlToText(html, options).ShouldBe(expected);
    }

    [TestMethod]
    public void ShouldAllowToDefineBasicSupportForInlineTags()
    {
        var html = @"<p>a <span>b </span>c<span>  d  </span>e</p>";
        var expected = "a b c d e";
        var options = new HtmlToTextOptions();
        options.Selectors.Add(new Selector() {Identifier = "span", Format = "inline"});
        HtmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldAllowToDefineBasicSupportForBlockLevelTags()
    {
        var html = @"<widget><gadget>a</gadget><fidget>b</fidget></widget>c<budget>d</budget>e";
        var expected = "a\n\nb\n\nc\n\nd\n\ne";
        var options = new HtmlToTextOptions();
        options.Selectors.Add(new Selector() {Identifier = "budget", Format = "block"});
        options.Selectors.Add(new Selector() {Identifier = "fidget", Format = "block"});
        options.Selectors.Add(new Selector() {Identifier = "gadget", Format = "block"});
        options.Selectors.Add(new Selector() {Identifier = "widget", Format = "block"});
        HtmlToText(html, options).ShouldBe(expected);
    }

    [TestMethod]
    public void ShouldAllowToAddSupportForDifferentTags()
    {
        var html = "<div><foo>foo<br/>content</foo><bar src=\"bar.src\" /></div>";
        var expected = "[FOO]foo\ncontent[/FOO]\n[BAR src=\"bar.src\"]";
        var options = new HtmlToTextOptions();

        options.Selectors.Add(new Selector() {Identifier = "foo", Format = "formatFoo"});
        options.Selectors.Add(new Selector() {Identifier = "bar", Format = "formatBar"});
        
        options.Formatters["formatFoo"] = (elem, walk, builder, formatOptions) =>
        {
            builder.OpenBlock(leadingLineBreaks: 1);
            walk(walk, elem.ChildNodes, builder);
            builder.PopWordTransform();
            builder.CloseBlock(trailingLineBreaks: 1, str => $"[FOO]{str}[/FOO]");
        };
        options.Formatters["formatBar"] = (elem, walk, builder, formatOptions) =>
        {
            builder.AddInline($"[BAR src=\"{elem.Attributes["src"]?.Value}\"]", noWordTransform: true);
        };
        HtmlToText(html, options).ShouldBe(expected);
    }

    [TestMethod]
    public void ShouldAllowToCallExistingFormattersFromOtherFormatters()
    {
        var html = "<div>Useful</div><div>Advertisement</div><article>Handy <section><div>info</div><div>Advertisement</div></section></article><article>ads galore</article>";
        
        var options = new HtmlToTextOptions
        {
            Div =
            {
                Format = "adFreeBlock"
            },
            Article =
            {
                Format = "adFreeBlock",
                Options =
                {
                    LeadingLineBreaks = 4,
                    ExtraOptions = new Dictionary<string, object>()
                    {
                        { "filterRegExp", new Regex("^ad", RegexOptions.IgnoreCase) }
                    }
                }
            },
            Formatters =
            {
                ["adFreeBlock"] = (elem, walk, builder, formatOptions) =>
                {
                    var regExp = new Regex("advertisement", RegexOptions.IgnoreCase);
                    if (formatOptions.ExtraOptions.TryGetValue("filterRegExp", out var option))
                    {
                        regExp = (Regex)option; 
                    }
                    // Regex regExp = formatOptions.FilterRegExp ?? new Regex("advertisement", RegexOptions.IgnoreCase);
                    if (elem.ChildNodes.Any(ch => ch.NodeType == NodeType.Text && regExp.IsMatch(ch.NodeValue)))
                    {
                        // do nothing
                    }
                    else
                    {
                        var blockFormatter = builder.Options.Formatters["block"];
                        blockFormatter(elem, walk, builder, formatOptions);
                    }
                }
            }
        };

        var expected = "Useful\n\n\n\nHandy\ninfo";
        HtmlToText(html, options).ShouldBe(expected);
    }
}
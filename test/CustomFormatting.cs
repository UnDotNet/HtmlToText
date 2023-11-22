using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace UnDotNet.HtmlToText.Tests;

[TestClass]
public class CustomFormattingTests
{
    private string htmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
        new HtmlToTextConverter().Convert(html, options, metadata);
    
    [TestMethod]
    public void ShouldAllowToOverrideFormattingOfExistingTags()
    {
        string html = "<h1>TeSt</h1><h1>mOrE tEsT</h1>";
        var options = new HtmlToTextOptions();
        options.formatters["heading"] = (elem, walk, builder, formatOptions) =>
        {
            builder.openBlock(leadingLineBreaks: 2);
            builder.pushWordTransform(str => str.ToLower());
            walk(walk, elem.ChildNodes, builder);
            builder.popWordTransform();
            builder.closeBlock(trailingLineBreaks: 2, str => 
            {
                string line = new string('=', str.Length);
                return $"{line}\n{str}\n{line}";
            });
        }; 
        string expected = "====\ntest\n====\n\n=========\nmore test\n=========";
        htmlToText(html, options).ShouldBe(expected);
    }

    [TestMethod]
    public void ShouldAllowToSkipTagsWithDummyFormattingFunction()
    {
        string html = "<ruby>漢<rt>かん</rt>字<rt>じ</rt></ruby>";
        string expected = "漢字";
        var options = new HtmlToTextOptions();
        options.selectors.Add(new Selector() {selector = "rt", format = "skip"});
        htmlToText(html, options).ShouldBe(expected);
    }

    [TestMethod]
    public void ShouldAllowToDefineBasicSupportForInlineTags()
    {
        string html = @"<p>a <span>b </span>c<span>  d  </span>e</p>";
        string expected = "a b c d e";
        var options = new HtmlToTextOptions();
        options.selectors.Add(new Selector() {selector = "span", format = "inline"});
        htmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldAllowToDefineBasicSupportForBlockLevelTags()
    {
        string html = @"<widget><gadget>a</gadget><fidget>b</fidget></widget>c<budget>d</budget>e";
        string expected = "a\n\nb\n\nc\n\nd\n\ne";
        var options = new HtmlToTextOptions();
        options.selectors.Add(new Selector() {selector = "budget", format = "block"});
        options.selectors.Add(new Selector() {selector = "fidget", format = "block"});
        options.selectors.Add(new Selector() {selector = "gadget", format = "block"});
        options.selectors.Add(new Selector() {selector = "widget", format = "block"});
        htmlToText(html, options).ShouldBe(expected);
    }

    [TestMethod]
    public void ShouldAllowToAddSupportForDifferentTags()
    {
        string html = "<div><foo>foo<br/>content</foo><bar src=\"bar.src\" /></div>";
        string expected = "[FOO]foo\ncontent[/FOO]\n[BAR src=\"bar.src\"]";
        var options = new HtmlToTextOptions();

        options.selectors.Add(new Selector() {selector = "foo", format = "formatFoo"});
        options.selectors.Add(new Selector() {selector = "bar", format = "formatBar"});
        
        options.formatters["formatFoo"] = (elem, walk, builder, formatOptions) =>
        {
            builder.openBlock(leadingLineBreaks: 1);
            walk(walk, elem.ChildNodes, builder);
            builder.popWordTransform();
            builder.closeBlock(trailingLineBreaks: 1, str => $"[FOO]{str}[/FOO]");
        };
        options.formatters["formatBar"] = (elem, walk, builder, formatOptions) =>
        {
            builder.addInline($"[BAR src=\"{elem.Attributes["src"]?.Value}\"]", noWordTransform: true);
        };
        htmlToText(html, options).ShouldBe(expected);
    }

    [TestMethod]
    public void ShouldAllowToCallExistingFormattersFromOtherFormatters()
    {
        string html = "<div>Useful</div><div>Advertisement</div><article>Handy <section><div>info</div><div>Advertisement</div></section></article><article>ads galore</article>";
        
        var options = new HtmlToTextOptions();
        
        options.Div.format = "adFreeBlock";
        options.Article.format = "adFreeBlock";
        options.Article.options.leadingLineBreaks = 4;
        options.Article.options.ExtraOptions = new Dictionary<string, object>()
        {
            { "filterRegExp", new Regex("^ad", RegexOptions.IgnoreCase) }
        }; 
        
        options.formatters["adFreeBlock"] = (elem, walk, builder, formatOptions) =>
        {
            Regex regExp = new Regex("advertisement", RegexOptions.IgnoreCase);
            if (formatOptions.ExtraOptions.ContainsKey("filterRegExp"))
            {
                regExp = (Regex)formatOptions.ExtraOptions["filterRegExp"]; 
            }
            // Regex regExp = formatOptions.FilterRegExp ?? new Regex("advertisement", RegexOptions.IgnoreCase);
            if (elem.ChildNodes.Any(ch => ch.NodeType == NodeType.Text && regExp.IsMatch(ch.NodeValue)))
            {
                // do nothing
            }
            else
            {
                var blockFormatter = builder.Options.formatters["block"];
                if (blockFormatter != null)
                {
                    blockFormatter(elem, walk, builder, formatOptions);
                }
            }
        }; 
        string expected = "Useful\n\n\n\nHandy\ninfo";
        htmlToText(html, options).ShouldBe(expected);
    }
}
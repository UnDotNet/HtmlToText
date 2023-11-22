
namespace UnDotNet.HtmlToText.Tests;

[TestClass]
public class CustomSelectors
{
    private string htmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
        new HtmlToTextConverter().Convert(html, options, metadata);

    [TestMethod]
    public void ShouldMergeEntriesWithTheSameSelector()
    {
        // this test isn't really applicable in DotNet, but I included it for completeness
        string html = "<foo></foo><foo></foo><foo></foo>";
        string expected = "----------\n\n\n\n----------\n\n\n\n----------";
        var options = new HtmlToTextOptions();
        options.selectors.Add(new Selector() {selector = "foo", format = "somethingElse"});
        options.Selector("foo").options.length = 20;
        options.Selector("foo").options.leadingLineBreaks = 4;
        options.Selector("foo").options.trailingLineBreaks = 4;
        options.Selector("foo").options.length = 10;
        options.Selector("foo").format = "horizontalLine";
        htmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldPickTheMostSpecificSelector()
    {
        string html = "<hr/><hr class=\"foo\" id=\"bar\"/>";
        string expected = "---\n\n-----";
        var options = new HtmlToTextOptions()
        {
            selectors = new List<Selector>()
            {
                new () { selector = "hr", format = "horizontalLine", options = new() {length = 3}},
                new () { selector = "hr#bar", format = "horizontalLine", options = new() {length = 5}},
                new () { selector = "hr#bar", format = "horizontalLine", options = new() {length = 7}},
            }
        };
        htmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldPickTheLastSelectorOfEqualSpecificity()
    {
        // rather than picking the last selector, we follow CSS rules which follow order in the class
        string html = "<hr class=\"bar baz\"/><hr class=\"foo bar\"/><hr class=\"foo baz\"/>";
        string expected = "---\n\n-------\n\n-------";
        var options = new HtmlToTextOptions()
        {
            selectors = new List<Selector>()
            {
                new () { selector = "hr.foo", format = "horizontalLine", options = new() {length = 7}},
                new () { selector = "hr.baz", format = "horizontalLine", options = new() {length = 3}},
                new () { selector = "hr.bar", format = "horizontalLine", options = new() {length = 5}},
            }
        };
        htmlToText(html, options).ShouldBe(expected);
    }
    
    [TestMethod]
    public void ShouldAllowEscapeSequencesInSelectors()
    {
        string html = "<hr id=\"sceneI_3.1\"/><hr class=\"---\"/>";
        string expected = "---[ cut ]---\n\n---[ cut ]---";
        var options = new HtmlToTextOptions();
        options.selectors.Add(new Selector() {selector = "#sceneI_3\\.1", format = "blockString", options = new () {stringLiteral = "---[ cut ]---"} });
        options.selectors.Add(new Selector() {selector = ".\\2d -\\-", format = "blockString", options = new () {stringLiteral = "---[ cut ]---"} });
        htmlToText(html, options).ShouldBe(expected);
    }
    
}